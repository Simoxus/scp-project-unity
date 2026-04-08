using Cysharp.Threading.Tasks;
using FMODUnity;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[ModAPI("Resource", perMod: true)]
[MoonSharpUserData]
public class ResourceAPI : IModAPICleanup
{
    private readonly string _resourcesPath;

    private readonly List<string> _loadedBankPaths = new List<string>();
    private readonly List<AsyncOperationHandle> _loadedHandles = new List<AsyncOperationHandle>();
    private readonly List<AsyncOperationHandle> _loadedCatalogs = new List<AsyncOperationHandle>();

    private readonly string _modId;
    public ResourceAPI(string modId)
    {
        _modId = modId;

        LuaMod mod = Core.ModManager?.GetMod(modId);
        string modFolder = mod?.Info.folderPath ?? string.Empty;
        _resourcesPath = string.IsNullOrEmpty(modFolder)
            ? string.Empty
            : Path.Combine(modFolder, "resources");
    }

    public void OnModUnloaded(string modId) => Cleanup();

    [MoonSharpVisible(true)]
    [LuaDoc("Loads an FMOD bank file from the mod's folder.")]
    [LuaParam("fileName", "Bank filename relative to the mod folder")]
    public void LoadBank(string fileName)
    {
        if (string.IsNullOrEmpty(_resourcesPath)) return;

        string fullPath = Path.Combine(_resourcesPath, fileName);
        if (!File.Exists(fullPath)) return;
        if (_loadedBankPaths.Contains(fullPath)) return;

        try
        {
            RuntimeManager.LoadBank(fullPath, loadSamples: true);
            _loadedBankPaths.Add(fullPath);
        }
        catch (BankLoadException ex)
        {
            Log.Exception(ex);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Unloads a previously loaded FMOD bank by filename.")]
    [LuaParam("fileName", "Bank filename used when loading")]
    public void UnloadBank(string fileName)
    {
        if (string.IsNullOrEmpty(_resourcesPath)) return;

        string fullPath = Path.Combine(_resourcesPath, fileName);
        if (!_loadedBankPaths.Contains(fullPath)) return;

        try
        {
            RuntimeManager.UnloadBank(fullPath);
            _loadedBankPaths.Remove(fullPath);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Returns true if the named FMOD bank is currently loaded by this mod.")]
    [LuaParam("fileName", "Bank filename to check")]
    public bool IsBankLoaded(string fileName)
    {
        string fullPath = Path.Combine(_resourcesPath, fileName);
        return _loadedBankPaths.Contains(fullPath);
    }


    [MoonSharpVisible(true)]
    [LuaDoc("Loads an Addressables content catalog from the mod's folder; Assets are not available until this completes.")]
    [LuaParam("fileName", "Catalog filename relative to the mod folder")]
    public async UniTask LoadCatalog(string fileName)
    {
        if (string.IsNullOrEmpty(_resourcesPath)) return;

        string fullPath = Path.Combine(_resourcesPath, fileName);
        if (!File.Exists(fullPath)) return;

        var handle = Addressables.LoadContentCatalogAsync(fullPath);
        await handle.ToUniTask();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedCatalogs.Add(handle);
        }
        else
        {
            Addressables.Release(handle);
        }
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Loads a GameObject asset by Addressable key and returns it. Must be awaited. Returns nil if the load fails.")]
    [LuaParam("key", "Addressable key")]
    public async UniTask<GameObject> LoadGameObject(string key)
    {
        return await LoadAssetInternal<GameObject>(key);
    }

    [MoonSharpVisible(true)]
    [LuaDoc("Loads a Texture2D asset by Addressable key and returns it. Must be awaited. Returns nil if the load fails.")]
    [LuaParam("key", "Addressable key")]
    public async UniTask<Texture2D> LoadTexture(string key)
    {
        return await LoadAssetInternal<Texture2D>(key);
    }

    private async UniTask<T> LoadAssetInternal<T>(string key) where T : UnityEngine.Object
    {
        var handle = Addressables.LoadAssetAsync<T>(key);
        await handle.ToUniTask();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _loadedHandles.Add(handle);
            return handle.Result;
        }

        Addressables.Release(handle);
        return null;
    }

    private void Cleanup()
    {
        foreach (var bankPath in _loadedBankPaths)
        {
            try
            {
                RuntimeManager.UnloadBank(bankPath);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        _loadedBankPaths.Clear();

        foreach (var handle in _loadedHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        _loadedHandles.Clear();

        foreach (var handle in _loadedCatalogs)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        _loadedCatalogs.Clear();
    }
}