using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ModManager : Singleton<ModManager>
{
    [SerializeField] private string modsFolder = "Mods";

    private readonly Dictionary<string, object> _registeredAPIs = new();
    private readonly Dictionary<string, LuaMod> _loadedMods = new();
    private readonly List<string> _loadOrder = new();
    private readonly List<(string Name, Type Type)> _perModAPITypes = new();

    public event Action<string> OnModLoaded;
    public event Action<string> OnModUnloaded;
    public event Action<string, string> OnModError;

    protected override void OnSingletonAwake()
    {
        InitializeMoonSharp();
        DiscoverAPIs();
    }

    private async void Start()
    {
        await LoadAllMods();
        await InitializeAllMods();
    }

    protected override void OnSingletonDestroy()
    {
        foreach (var mod in _loadedMods.Values)
        {
            mod.Unload().Forget();
        }
    }

    private void Update()
    {
        foreach (var mod in _loadedMods.Values)
        {
            if (!mod.IsEnabled) { continue; }

            try
            {
                mod.Update(Time.deltaTime);
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex, message: ex.DecoratedMessage);
                OnModError?.Invoke(mod.Info.id, ex.DecoratedMessage);
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (var mod in _loadedMods.Values)
        {
            if (!mod.IsEnabled) { continue; }

            try
            {
                mod.FixedUpdate(Time.fixedDeltaTime);
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex, message: ex.DecoratedMessage);
            }
        }
    }

    private void LateUpdate()
    {
        foreach (var mod in _loadedMods.Values)
        {
            if (!mod.IsEnabled) { continue; }

            try
            {
                mod.LateUpdate(Time.deltaTime);
            }
            catch (ScriptRuntimeException ex)
            {
                Log.Exception(ex, message: ex.DecoratedMessage);
            }
        }
    }

    private void InitializeMoonSharp()
    {
        UserData.RegisterAssembly();

        UserData.RegisterType<Vector3>();
        UserData.RegisterType<Vector2>();
        UserData.RegisterType<Quaternion>();
        UserData.RegisterType<Color>();
        UserData.RegisterType<Transform>();
        UserData.RegisterType<GameObject>();
        UserData.RegisterType<RaycastHit>();
        UserData.RegisterType<Mathf>();
        UserData.RegisterType<Physics>();
        UserData.RegisterType<Input>();
        UserData.RegisterType<Screen>();
        UserData.RegisterType<Player>();
        UserData.RegisterType<PlayerState>();
        UserData.RegisterType<PlayerHealth.HealthLevel>();

        Log.VerboseSuccess("MoonSharp initialized");
    }

    private void DiscoverAPIs()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
            .Where(t => t.GetCustomAttribute<ModAPIAttribute>() != null && !t.IsAbstract);

        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<ModAPIAttribute>();

            if (attr.PerMod)
            {
                _perModAPITypes.Add((attr.Name, type));
            }
            else
            {
                RegisterAPI(attr.Name, Activator.CreateInstance(type));
            }
        }

        Log.VerboseSuccess($"Discovered {_registeredAPIs.Count} global APIs, {_perModAPITypes.Count} per-mod APIs");
    }

    public void RegisterAPI(string name, object api)
    {
        if (_registeredAPIs.ContainsKey(name))
        {
            Log.VerboseWarning($"API '{name}' already registered, overwriting");
        }

        _registeredAPIs[name] = api;

        foreach (var mod in _loadedMods.Values)
        {
            mod.RegisterAPI(name, api);
        }
    }

    public T GetAPI<T>(string name) where T : class
    {
        _registeredAPIs.TryGetValue(name, out object api);
        return api as T;
    }

    private void RegisterAllAPIsToMod(LuaMod mod)
    {
        foreach (var (name, api) in _registeredAPIs)
        {
            mod.RegisterAPI(name, api);
        }

        foreach (var (name, type) in _perModAPITypes)
        {
            mod.RegisterAPI(name, Activator.CreateInstance(type, mod.Info.id));
        }
    }

    private string GetModsPath()
    {
        string root = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(root, modsFolder);
    }

    private async UniTask LoadAllMods()
    {
        string modsPath = GetModsPath();

        if (!Directory.Exists(modsPath))
        {
            Log.VerboseInfo($"Mods folder not found at {modsPath}, creating");
            Directory.CreateDirectory(modsPath);
            return;
        }

        var modInfos = new List<ModInfo>();

        foreach (var dir in Directory.GetDirectories(modsPath))
        {
            string modJsonPath = Path.Combine(dir, "mod.json");

            if (!File.Exists(modJsonPath))
            {
                Log.Warning($"No mod.json in {Path.GetFileName(dir)}, skipping");
                continue;
            }

            try
            {
                ModInfo info = JsonUtility.FromJson<ModInfo>(File.ReadAllText(modJsonPath));
                info.folderPath = dir;
                modInfos.Add(info);
            }
            catch (Exception ex)
            {
                Log.Exception(ex, message: ex.Message);
            }
        }

        foreach (var info in modInfos.OrderBy(m => m.loadOrder))
        {
            await LoadMod(info);
        }
    }

    private async UniTask LoadMod(ModInfo info)
    {
        if (_loadedMods.ContainsKey(info.id))
        {
            Log.VerboseWarning($"Mod '{info.id}' already loaded");
            return;
        }

        foreach (var dep in info.dependencies)
        {
            if (!_loadedMods.ContainsKey(dep))
            {
                Log.Error($"Mod '{info.id}' requires '{dep}' which is not loaded");
                return;
            }
        }

        try
        {
            LuaMod mod = new LuaMod(info);
            RegisterAllAPIsToMod(mod);
            ModSandboxSettings.ConfigureSandbox(mod.Script, info.name);

            string mainScriptPath = Path.Combine(info.folderPath, info.entryPoint);

            if (!File.Exists(mainScriptPath))
            {
                Log.Error($"Entry point '{info.entryPoint}' not found for mod '{info.id}'");
                return;
            }

            await mod.Load(File.ReadAllText(mainScriptPath));

            _loadedMods[info.id] = mod;
            _loadOrder.Add(info.id);

            OnModLoaded?.Invoke(info.id);
            Log.VerboseSuccess($"Loaded mod '{info.name}'");
        }
        catch (Exception ex)
        {
            Log.Exception(ex, message: ex.Message);
            OnModError?.Invoke(info.id, ex.Message);
        }
    }

    private async UniTask InitializeAllMods()
    {
        foreach (var modId in _loadOrder)
        {
            if (!_loadedMods.TryGetValue(modId, out LuaMod mod)) { continue; }

            try
            {
                await mod.Initialize();
            }
            catch (Exception ex)
            {
                Log.Exception(ex, message: ex.Message);
                OnModError?.Invoke(modId, ex.Message);
            }
        }

        Log.VerboseSuccess($"Loaded {_loadedMods.Count} mods");
    }

    public async UniTask ReloadMod(string modId)
    {
        if (!_loadedMods.TryGetValue(modId, out LuaMod mod))
        {
            Log.Warning($"Cannot reload mod '{modId}'; not loaded");
            return;
        }

        ModInfo modInfo = mod.Info;

        foreach (var api in _registeredAPIs.Values.OfType<IModAPICleanup>())
        {
            api.OnModUnloaded(modId);
        }

        await mod.Unload();
        _loadedMods.Remove(modId);
        _loadOrder.Remove(modId);
        OnModUnloaded?.Invoke(modId);

        await LoadMod(modInfo);

        if (_loadedMods.ContainsKey(modId))
        {
            await _loadedMods[modId].Initialize();
        }
    }

    public async UniTask ReloadAllMods()
    {
        foreach (var modId in _loadOrder.ToList())
        {
            await ReloadMod(modId);
        }

        Log.VerboseSuccess("All mods reloaded");
    }

    public void EnableMod(string modId)
    {
        if (_loadedMods.TryGetValue(modId, out LuaMod mod))
        {
            mod.Enable();
        }
    }

    public void DisableMod(string modId)
    {
        if (_loadedMods.TryGetValue(modId, out LuaMod mod))
        {
            mod.Disable();
        }
    }

    public LuaMod GetMod(string modId)
    {
        _loadedMods.TryGetValue(modId, out LuaMod mod);
        return mod;
    }

    public List<ModInfo> GetAllModInfo() => _loadedMods.Values.Select(m => m.Info).ToList();
    public string GetModsFolderPath() => GetModsPath();
}