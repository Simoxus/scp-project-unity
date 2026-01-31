using Cysharp.Threading.Tasks;
using Facility.Persistence;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ProgressManager : Singleton<ProgressManager>
{
    [Space]
    [SerializeField] private string saveFolderName = "Saves";
    [SerializeField] private string autosaveName = "Autosave";
    [SerializeField] private string quicksaveName = "Quicksave";
    [SerializeField] private int maxSaveSlots = 3;

    [Header("Capture Settings")]
    [SerializeField] private int thumbnailReferenceWidth = 1024;
    [SerializeField] private int thumbnailReferenceHeight = 1024;

    private string SavesPath => GetSavesPath();
    private PersistDataRegistry _registry = new PersistDataRegistry();
    private string _currentSiteFolder = null;

    public event Action<string> OnSaveCompleted;
    public event Action<string> OnLoadCompleted;
    public event Action<string, string> OnSaveError;

    public string CurrentSiteFolder => _currentSiteFolder;
    public string SaveFolderName => saveFolderName;
    public string AutosaveName => autosaveName;
    public string QuicksaveName => quicksaveName;
    public bool HasActiveSite => !string.IsNullOrEmpty(_currentSiteFolder);

    protected override void OnSingletonAwake()
    {
        _registry.RegisterAllTypes();
        EnsureSavesFolderExists();
    }

    private string GetSavesPath()
    {
#if UNITY_EDITOR
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, saveFolderName);
#else
        string gameDirectory = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(gameDirectory, saveFolderName);
#endif
    }

    private void EnsureSavesFolderExists()
    {
        if (!Directory.Exists(SavesPath))
        {
            Directory.CreateDirectory(SavesPath);
            Log.Info($"Created saves folder at: {SavesPath}");
        }
    }

    public void SetCurrentSite(string siteName)
    {
        _currentSiteFolder = SanitizeFolderName(siteName);
        string sitePath = Path.Combine(SavesPath, _currentSiteFolder);

        if (!Directory.Exists(sitePath))
        {
            Directory.CreateDirectory(sitePath);
        }

        Log.Info($"Active site set to: {_currentSiteFolder}");
    }

    private string GetSlotNameForSave(string saveType)
    {
        if (saveType == autosaveName)
        {
            return autosaveName;
        }
        else // Quicksave
        {
            return $"{quicksaveName} 1";
        }
    }

    private void RotateQuickSaves()
    {
        if (!HasActiveSite) return;

        string sitePath = Path.Combine(SavesPath, _currentSiteFolder);

        // Delete the oldest save (max slot) if it exists
        string oldestPath = Path.Combine(sitePath, $"{quicksaveName} {maxSaveSlots}");
        if (Directory.Exists(oldestPath))
        {
            Directory.Delete(oldestPath, true);
        }

        // Rotate saves: Quicksave N-1 becomes Quicksave N
        for (int i = maxSaveSlots - 1; i >= 1; i--)
        {
            string currentPath = Path.Combine(sitePath, $"{quicksaveName} {i}");
            string nextPath = Path.Combine(sitePath, $"{quicksaveName} {i + 1}");

            if (Directory.Exists(currentPath))
            {
                Directory.Move(currentPath, nextPath);
            }
        }
    }

    public async UniTask<bool> CreateNewSave(string siteName, PersistMetadata metadata, string saveType, bool takeScreenshot = true)
    {
        try
        {
            SetCurrentSite(siteName);

            // Rotate quicksaves before determining slot name
            if (saveType == quicksaveName)
            {
                RotateQuickSaves();
            }

            string slotName = GetSlotNameForSave(saveType);
            string slotPath = Path.Combine(SavesPath, _currentSiteFolder, slotName);

            if (!Directory.Exists(slotPath))
            {
                Directory.CreateDirectory(slotPath);
            }

            string metadataPath = Path.Combine(slotPath, "metadata.json");
            string thumbnailPath = Path.Combine(slotPath, "thumbnail.png");

            // Serialize on background thread
            string metadataJson = await UniTask.RunOnThreadPool(() =>
                JsonConvert.SerializeObject(metadata, Formatting.Indented)
            );

            // Write file on background thread
            await File.WriteAllTextAsync(metadataPath, metadataJson).AsUniTask();

            if (takeScreenshot)
            {
                await CaptureScreenshot(thumbnailPath);
            }

            Log.Success($"Created save in {slotName}");
            OnSaveCompleted?.Invoke(siteName);
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to create save: {e.Message}");
            OnSaveError?.Invoke(siteName, e.Message);
            return false;
        }
    }

    public async UniTask<bool> UpdateMetadata(PersistMetadata metadata, string saveType, bool takeScreenshot = true)
    {
        if (!HasActiveSite) return false;

        try
        {
            // Rotate quicksaves before determining slot name
            if (saveType == quicksaveName)
            {
                RotateQuickSaves();
            }

            string slotName = GetSlotNameForSave(saveType);
            string slotPath = Path.Combine(SavesPath, _currentSiteFolder, slotName);

            if (!Directory.Exists(slotPath))
            {
                Directory.CreateDirectory(slotPath);
            }

            string metadataPath = Path.Combine(slotPath, "metadata.json");
            string thumbnailPath = Path.Combine(slotPath, "thumbnail.png");

            // Serialize on background thread
            string metadataJson = await UniTask.RunOnThreadPool(() =>
                JsonConvert.SerializeObject(metadata, Formatting.Indented)
            );

            // Write file on background thread
            await File.WriteAllTextAsync(metadataPath, metadataJson).AsUniTask();

            if (takeScreenshot)
            {
                await CaptureScreenshot(thumbnailPath);
            }

            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to update metadata: {e.Message}");
            return false;
        }
    }

    public async UniTask<bool> SaveToSiteRoot(IPersistData data, string siteName)
    {
        try
        {
            string sitePath = Path.Combine(
                GetSavesFolderPath(),
                SanitizeFolderName(siteName)
            );

            if (!Directory.Exists(sitePath))
            {
                Directory.CreateDirectory(sitePath);
            }

            string filePath = Path.Combine(sitePath, data.FileName);
            string json = data.ToJson();

            if (string.IsNullOrEmpty(json))
            {
                Log.Error($"Failed to serialize {data.PersistDataType}");
                return false;
            }

            await File.WriteAllTextAsync(filePath, json);
            Log.Info($"Saved {data.PersistDataType} to site root: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to save {data.PersistDataType} to site root: {e.Message}");
            return false;
        }
    }

    public async UniTask<bool> SaveToCurrentSite(IPersistData persistData, string saveType)
    {
        if (!HasActiveSite)
        {
            Log.Warning("No active site. Call SetCurrentSite() first.");
            return false;
        }

        string slotName = GetSlotNameForSave(saveType);
        string slotPath = Path.Combine(SavesPath, _currentSiteFolder, slotName);
        return await SaveDataToPath(slotPath, persistData);
    }

    private async UniTask<bool> SaveDataToPath(string slotPath, IPersistData persistData)
    {
        try
        {
            if (!Directory.Exists(slotPath))
            {
                Directory.CreateDirectory(slotPath);
            }

            // Serialize on background thread
            string json = await UniTask.RunOnThreadPool(() => persistData.ToJson());

            if (json == null)
            {
                return false;
            }

            string filePath = Path.Combine(slotPath, persistData.FileName);

            // Write on background thread
            await File.WriteAllTextAsync(filePath, json).AsUniTask();

            Log.Info($"Saved {persistData.PersistDataType} to {Path.GetFileName(slotPath)}");
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to save {persistData.PersistDataType}: {e.Message}");
            return false;
        }
    }

    public async UniTask<T> LoadDataFromPath<T>(string slotPath) where T : class, IPersistData
    {
        try
        {
            var temp = Activator.CreateInstance<T>();
            string filePath = Path.Combine(slotPath, temp.FileName);

            if (!File.Exists(filePath))
            {
                Log.Warning($"File '{temp.FileName}' not found in {Path.GetFileName(slotPath)}");
                return null;
            }

            string json = await File.ReadAllTextAsync(filePath);
            var data = _registry.Deserialize(temp.PersistDataType, json) as T;

            if (data != null)
            {
                Log.Info($"Loaded {temp.PersistDataType} from {Path.GetFileName(slotPath)}");
            }

            return data;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to load data: {e.Message}");
            return null;
        }
    }

    public bool LoadSite(string siteName, string saveType = null)
    {
        if (saveType == null)
        {
            saveType = $"{quicksaveName} 1";
        }

        try
        {
            SetCurrentSite(siteName);

            string slotPath = Path.Combine(SavesPath, _currentSiteFolder, saveType);

            if (!Directory.Exists(slotPath))
            {
                Log.Error($"Save not found: {saveType}");
                return false;
            }

            Log.Success($"Loaded {saveType}");
            OnLoadCompleted?.Invoke(siteName);
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to load site: {e.Message}");
            OnSaveError?.Invoke(siteName, e.Message);
            return false;
        }
    }

    private async UniTask CaptureScreenshot(string thumbnailPath)
    {
        try
        {
            await UniTask.WaitForEndOfFrame();

            // Use smaller of reference dimensions for square size
            int size = Mathf.Min(thumbnailReferenceWidth, thumbnailReferenceHeight) / 2;
            size = Mathf.Clamp(size, 256, 1024); // Square between 256x256 and 1024x1024

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Log.Warning("No main camera found for screenshot");
                return;
            }

            // Render to square texture
            RenderTexture rt = new RenderTexture(size, size, 24);
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture currentCameraRT = mainCamera.targetTexture;

            mainCamera.targetTexture = rt;
            mainCamera.Render();
            RenderTexture.active = rt;

            Texture2D screenshot = new Texture2D(size, size, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            screenshot.Apply();

            mainCamera.targetTexture = currentCameraRT;
            RenderTexture.active = currentRT;
            Destroy(rt);

            // Encode on main thread (Unity API requirement)
            byte[] bytes = screenshot.EncodeToPNG();
            Destroy(screenshot);

            // Write file on background thread
            await File.WriteAllBytesAsync(thumbnailPath, bytes).AsUniTask();

            Log.VerboseInfo($"Screenshot captured at {size}x{size}");
        }
        catch (Exception e)
        {
            Log.Warning($"Failed to capture screenshot: {e.Message}");
        }
    }

    public async UniTask<Texture2D> LoadScreenshot(string siteName, string saveType)
    {
        string slotPath = Path.Combine(SavesPath, SanitizeFolderName(siteName), saveType);
        string thumbnailPath = Path.Combine(slotPath, "thumbnail.png");

        if (!File.Exists(thumbnailPath))
        {
            return null;
        }

        try
        {
            byte[] bytes = await File.ReadAllBytesAsync(thumbnailPath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);
            return texture;
        }
        catch (Exception e)
        {
            Log.Warning($"Failed to load screenshot: {e.Message}");
            return null;
        }
    }

    public bool DeleteSite(string siteName)
    {
        try
        {
            string sitePath = Path.Combine(SavesPath, SanitizeFolderName(siteName));

            if (!Directory.Exists(sitePath))
            {
                Log.Warning($"Site folder not found: {siteName}");
                return false;
            }

            if (_currentSiteFolder == SanitizeFolderName(siteName))
            {
                _currentSiteFolder = null;
            }

            Directory.Delete(sitePath, true);
            Log.Info($"Deleted site: {siteName}");
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to delete site: {e.Message}");
            return false;
        }
    }

    public List<string> GetAllSites()
    {
        if (!Directory.Exists(SavesPath))
        {
            return new List<string>();
        }

        return Directory.GetDirectories(SavesPath)
            .Select(d => Path.GetFileName(d))
            .ToList();
    }

    public List<string> GetQuickSavesForSite(string siteName)
    {
        string sitePath = Path.Combine(SavesPath, SanitizeFolderName(siteName));

        if (!Directory.Exists(sitePath))
        {
            return new List<string>();
        }

        var saves = new List<string>();

        for (int i = 1; i <= maxSaveSlots; i++)
        {
            string slotPath = Path.Combine(sitePath, $"{quicksaveName} {i}");
            if (Directory.Exists(slotPath))
            {
                saves.Add($"{quicksaveName} {i}");
            }
        }

        return saves;
    }

    public bool SiteExists(string siteName)
    {
        string sitePath = Path.Combine(SavesPath, SanitizeFolderName(siteName));
        return Directory.Exists(sitePath);
    }

    public bool AutosaveExists(string siteName)
    {
        string autosavePath = Path.Combine(SavesPath, SanitizeFolderName(siteName), autosaveName);
        return Directory.Exists(autosavePath) && File.Exists(Path.Combine(autosavePath, "facility.json"));
    }

    public string SanitizeFolderName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Unknown";
        }

        char[] invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalidChars));
    }

    public string GetSavesFolderPath()
    {
        return SavesPath;
    }
}