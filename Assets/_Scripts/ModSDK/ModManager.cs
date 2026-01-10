using Cysharp.Threading.Tasks;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ModManager : Singleton<ModManager>
{
    [SerializeField] private string modsFolder = "Mods";

    private Dictionary<string, object> _registeredAPIs = new Dictionary<string, object>();
    private Dictionary<string, LuaMod> _loadedMods = new Dictionary<string, LuaMod>();
    private List<string> _loadOrder = new List<string>();

    public event Action<string> OnModLoaded;
    public event Action<string> OnModUnloaded;
    public event Action<string, string> OnModError;

    private new void Awake()
    {
        base.Awake();
        InitializeMoonSharp();
    }

    private async void Start()
    {
        await RegisterCoreAPIs();
        await LoadAllMods();
        await InitializeAllMods();
    }

    private new void OnDestroy()
    {
        base.OnDestroy();

        foreach (var mod in _loadedMods.Values)
        {
            mod.Unload().Forget();
        }
    }

    private void Update()
    {
        foreach (var mod in _loadedMods.Values)
        {
            if (mod.IsEnabled)
            {
                try
                {
                    mod.Update(Time.deltaTime);
                }
                catch (ScriptRuntimeException e)
                {
                    Log.Error($"Runtime error in mod '{mod.Info.id}': {e.DecoratedMessage}");
                    OnModError?.Invoke(mod.Info.id, e.DecoratedMessage);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        foreach (var mod in _loadedMods.Values)
        {
            if (mod.IsEnabled)
            {
                try
                {
                    mod.FixedUpdate(Time.fixedDeltaTime);
                }
                catch (ScriptRuntimeException e)
                {
                    Log.Error($"Runtime error in mod '{mod.Info.id}': {e.DecoratedMessage}");
                }
            }
        }
    }

    private void LateUpdate()
    {
        foreach (var mod in _loadedMods.Values)
        {
            if (mod.IsEnabled)
            {
                try
                {
                    mod.LateUpdate(Time.deltaTime);
                }
                catch (ScriptRuntimeException e)
                {
                    Log.Error($"Runtime error in mod '{mod.Info.id}': {e.DecoratedMessage}");
                }
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

        UserData.RegisterType<Player>();
        UserData.RegisterType<PlayerState>();
        UserData.RegisterType<PlayerHealth.HealthLevel>();

        Log.VerboseInfo("MoonSharp initialized");
    }

    private void ConfigureSandbox(Script script)
    {
        ModSandboxSettings.ConfigureSandbox(script);
    }

    private async UniTask RegisterCoreAPIs()
    {
        Log.VerboseInfo("Registering core APIs...");

        RegisterAPI("Console", new ConsoleAPI());
        RegisterAPI("Player", new PlayerAPI());
        RegisterAPI("Unity", new UnityAPI());

        Log.VerboseSuccess($"Registered {_registeredAPIs.Count} APIs");

        await UniTask.Yield();
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
        if (_registeredAPIs.TryGetValue(name, out object api))
        {
            return api as T;
        }
        return null;
    }

    private void RegisterAllAPIsToMod(LuaMod mod)
    {
        foreach (var apiEntry in _registeredAPIs)
        {
            mod.RegisterAPI(apiEntry.Key, apiEntry.Value);
        }
    }

    private string GetModsPath()
    {
#if UNITY_EDITOR
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, modsFolder);
#else
        string gameDirectory = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(gameDirectory, modsFolder);
#endif
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

        var modDirs = Directory.GetDirectories(modsPath);
        Log.VerboseSuccess($"Found {modDirs.Length} potential mods in: {modsPath}");

        List<ModInfo> modInfos = new List<ModInfo>();

        foreach (var dir in modDirs)
        {
            string modJsonPath = Path.Combine(dir, "mod.json");
            if (!File.Exists(modJsonPath))
            {
                Log.Warning($"No mod.json found in {Path.GetFileName(dir)}, skipping");
                continue;
            }

            try
            {
                string json = File.ReadAllText(modJsonPath);
                ModInfo info = JsonUtility.FromJson<ModInfo>(json);
                info.folderPath = dir;
                modInfos.Add(info);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to parse mod.json in {Path.GetFileName(dir)}: {e.Message}");
            }
        }

        modInfos = modInfos.OrderBy(m => m.loadOrder).ToList();
        foreach (var info in modInfos)
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

        Log.VerboseInfo($"Loading mod: {info.name} ({info.id}) v{info.version}");

        try
        {
            foreach (var dep in info.dependencies)
            {
                if (!_loadedMods.ContainsKey(dep))
                {
                    Log.Error($"Mod '{info.id}' requires '{dep}' which is not loaded");
                    return;
                }
            }

            LuaMod mod = new LuaMod(info);

            RegisterAllAPIsToMod(mod);
            ConfigureSandbox(mod.Script);

            string mainScriptPath = Path.Combine(info.folderPath, info.entryPoint);
            if (!File.Exists(mainScriptPath))
            {
                Log.Error($"Entry point '{info.entryPoint}' not found for mod '{info.id}'");
                return;
            }

            string mainScript = File.ReadAllText(mainScriptPath);
            await mod.Load(mainScript);

            _loadedMods[info.id] = mod;
            _loadOrder.Add(info.id);

            OnModLoaded?.Invoke(info.id);
            Log.VerboseSuccess($"Loaded mod '{info.name}'");
        }
        catch (Exception e)
        {
            Log.Error($"Failed to load mod '{info.id}': {e.Message}\n{e.StackTrace}");
            OnModError?.Invoke(info.id, e.Message);
        }
    }

    private async UniTask InitializeAllMods()
    {
        foreach (var modId in _loadOrder)
        {
            if (_loadedMods.TryGetValue(modId, out LuaMod mod))
            {
                try
                {
                    await mod.Initialize();
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to initialize mod '{modId}': {e.Message}");
                    OnModError?.Invoke(modId, e.Message);
                }
            }
        }

        Log.VerboseSuccess($"Initialized {_loadedMods.Count} mods");
    }

    public async UniTask ReloadMod(string modId)
    {
        if (!_loadedMods.TryGetValue(modId, out LuaMod mod))
        {
            Log.Warning($"Cannot reload mod '{modId}' because it's not loaded");
            return;
        }

        Log.VerboseInfo($"Reloading mod '{mod.Info.name}'");

        ModInfo modInfo = mod.Info;

        if (_registeredAPIs.TryGetValue("Console", out object api) && api is ConsoleAPI consoleAPI)
        {
            consoleAPI.CleanupCommands();
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
        Log.VerboseInfo("Reloading all mods...");

        var modIds = _loadOrder.ToList();

        foreach (var modId in modIds)
        {
            await ReloadMod(modId);
        }

        Log.VerboseSuccess("All mods reloaded!");
    }

    public void EnableMod(string modId)
    {
        if (_loadedMods.TryGetValue(modId, out LuaMod mod))
        {
            mod.Enable();
            Log.VerboseSuccess($"Enabled mod '{mod.Info.name}'");
        }
    }

    public void DisableMod(string modId)
    {
        if (_loadedMods.TryGetValue(modId, out LuaMod mod))
        {
            mod.Disable();
            Log.VerboseSuccess($"Disabled mod '{mod.Info.name}'");
        }
    }

    public LuaMod GetMod(string modId)
    {
        _loadedMods.TryGetValue(modId, out LuaMod mod);
        return mod;
    }

    public List<ModInfo> GetAllModInfo()
    {
        return _loadedMods.Values.Select(m => m.Info).ToList();
    }

    public string GetModsFolderPath()
    {
        return GetModsPath();
    }
}