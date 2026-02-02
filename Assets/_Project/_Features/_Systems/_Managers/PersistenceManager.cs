using Cysharp.Threading.Tasks;
using Facility.Persistence;
using Facility.Persistence.Types;
using System;
using UnityEngine;

public class PersistenceManager : Singleton<PersistenceManager>
{
    [Space]
    [SerializeField] private bool autosaveEnabled = true;
    [SerializeField] private float autosaveInterval = 300f;

    private float _autosaveTimer = 0f;
    private bool _isSaving = false;
    private bool _isLoading = false;

    public bool IsSaving => _isSaving;
    public bool IsLoading => _isLoading;

    protected override void OnSingletonAwake()
    {
        if (Core.Player?.Inputs != null)
        {
            Core.Player.Inputs.OnQuickSave += HandleQuickSave;
            Core.Player.Inputs.OnQuickLoad += HandleQuickLoad;
        }
    }

    protected override void OnSingletonDestroy()
    {
        if (Core.Player?.Inputs != null)
        {
            Core.Player.Inputs.OnQuickSave -= HandleQuickSave;
            Core.Player.Inputs.OnQuickLoad -= HandleQuickLoad;
        }
    }

    private void Update()
    {
        if (autosaveEnabled && !_isSaving && Core.ProgressManager.HasActiveSite)
        {
            _autosaveTimer += Time.deltaTime;
            if (_autosaveTimer >= autosaveInterval)
            {
                _autosaveTimer = 0f;
                PerformAutosave().Forget();
            }
        }
    }

    private void HandleQuickSave()
    {
        if (_isSaving || _isLoading)
        {
            Log.Warning("Persist operation already in progress");
            return;
        }

        QuickSave().Forget();
    }

    private void HandleQuickLoad()
    {
        if (_isSaving || _isLoading)
        {
            Log.Warning("Persist operation already in progress");
            return;
        }

        QuickLoad().Forget();
    }

    public async UniTask<bool> QuickSave()
    {
        _isSaving = true;

        try
        {
            Log.Info("Quick saving...");

            string siteName = GetCurrentSiteName();
            bool success = await SaveGame(siteName, Core.ProgressManager.QuicksaveName, takeScreenshot: true);

            if (success)
            {
                Log.Success("Quick save complete!");
                _autosaveTimer = 0f;
            }

            return success;
        }
        finally
        {
            _isSaving = false;
        }
    }

    public async UniTask<bool> QuickLoad()
    {
        _isLoading = true;

        try
        {
            string siteName = GetCurrentSiteName();

            if (!Core.ProgressManager.SiteExists(siteName))
            {
                Log.Warning("No quicksave found");
                return false;
            }

            var quickSaves = Core.ProgressManager.GetQuickSavesForSite(siteName);
            if (quickSaves.Count == 0)
            {
                Log.Warning("No quicksave found");
                return false;
            }

            Log.Info("Quick loading...");

            bool success = await LoadGame(siteName, quickSaves[0]);

            if (success)
            {
                Log.Success("Quick load complete!");
                _autosaveTimer = 0f;
            }

            return success;
        }
        finally
        {
            _isLoading = false;
        }
    }

    public async UniTask<bool> SaveGame(string siteName, string saveType, bool takeScreenshot = true)
    {
        try
        {
            var metadata = CreateCurrentMetadata(siteName);

            if (!Core.ProgressManager.HasActiveSite ||
                Core.ProgressManager.CurrentSiteFolder != Core.ProgressManager.SanitizeFolderName(siteName))
            {
                await Core.ProgressManager.CreateNewSave(siteName, metadata, saveType, takeScreenshot);
            }
            else
            {
                await Core.ProgressManager.UpdateMetadata(metadata, saveType, takeScreenshot);
            }

            bool allSucceeded = true;

            if (Core.FacilityGenerator?.IsGenerated == true)
            {
                // Save facility data
                var facilityData = Core.FacilityGenerator.SaveToData();
                if (facilityData != null)
                {
                    bool success = await SavePersistData(facilityData, siteName, saveType);
                    allSucceeded &= success;
                }

                // Save navigation links data
                var navLinksData = Core.FacilityGenerator.SaveNavLinksToData();
                if (navLinksData != null)
                {
                    bool success = await SavePersistData(navLinksData, siteName, saveType);
                    allSucceeded &= success;
                }

                // Save door states data
                var doorStatesData = Core.FacilityGenerator.SaveDoorStatesToData();
                if (doorStatesData != null)
                {
                    bool success = await SavePersistData(doorStatesData, siteName, saveType);
                    allSucceeded &= success;
                }
            }

            return allSucceeded;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to save game: {e.Message}");
            return false;
        }
    }

    private async UniTask<bool> SavePersistData(IPersistData data, string siteName, string saveType)
    {
        if (data.SavePerSlot)
        {
            // Save to save slot (Autosave, Quicksave, etc.)
            Log.Info($"Saving {data.PersistDataType} to slot '{saveType}' (SavePerSlot=true)");
            return await Core.ProgressManager.SaveToCurrentSite(data, saveType);
        }
        else
        {
            // Save to site root (shared across all saves)
            Log.Info($"Saving {data.PersistDataType} to site root (SavePerSlot=false)");
            return await Core.ProgressManager.SaveToSiteRoot(data, siteName);
        }
    }

    public async UniTask<bool> LoadGame(string siteName, string saveType)
    {
        try
        {
            Core.ProgressManager.LoadSite(siteName, saveType);

            bool allSucceeded = true;

            var facilityData = await LoadPersistData<FacilityPersistData>(siteName, saveType);
            var navLinksData = await LoadPersistData<NavLinksPersistData>(siteName, saveType);
            var doorStatesData = await LoadPersistData<DoorStatesPersistData>(siteName, saveType);

            if (facilityData != null && Core.FacilityGenerator != null)
            {
                bool success = Core.FacilityGenerator.LoadFromData(
                    facilityData,
                    navLinksData,
                    doorStatesData
                );
                allSucceeded &= success;
            }

            return allSucceeded;
        }
        catch (Exception e)
        {
            Log.Error($"Failed to load game: {e.Message}");
            return false;
        }
    }

    private async UniTask<T> LoadPersistData<T>(string siteName, string saveType) where T : class, IPersistData
    {
        T tempInstance = Activator.CreateInstance<T>();

        if (tempInstance.SavePerSlot)
        {
            // Load from save slot
            string slotPath = System.IO.Path.Combine(
                Core.ProgressManager.GetSavesFolderPath(),
                Core.ProgressManager.SanitizeFolderName(siteName),
                saveType
            );
            Log.Info($"Loading {tempInstance.PersistDataType} from slot '{saveType}' (SavePerSlot=true)");
            return await Core.ProgressManager.LoadDataFromPath<T>(slotPath);
        }
        else
        {
            // Load from site root
            string sitePath = System.IO.Path.Combine(
                Core.ProgressManager.GetSavesFolderPath(),
                Core.ProgressManager.SanitizeFolderName(siteName)
            );
            Log.Info($"Loading {tempInstance.PersistDataType} from site root (SavePerSlot=false)");
            return await Core.ProgressManager.LoadDataFromPath<T>(sitePath);
        }
    }

    public async UniTask<bool> ManualAutosave(bool resetTimer = true)
    {
        if (_isSaving || _isLoading)
        {
            return false;
        }

        _isSaving = true;

        try
        {
            Log.Info("Manual autosaving...");
            string siteName = GetCurrentSiteName();
            bool success = await SaveGame(siteName, Core.ProgressManager.AutosaveName, takeScreenshot: false);

            if (success)
            {
                Log.Success("Manual autosave complete!");
                if (resetTimer)
                {
                    _autosaveTimer = 0f;
                }
            }

            return success;
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async UniTask PerformAutosave()
    {
        _isSaving = true;

        try
        {
            Log.Info("Autosaving...");
            string siteName = GetCurrentSiteName();
            await SaveGame(siteName, Core.ProgressManager.AutosaveName, takeScreenshot: false);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private string GetCurrentSiteName()
    {
        if (Core.ProgressManager.HasActiveSite)
        {
            return Core.ProgressManager.CurrentSiteFolder;
        }

        if (Core.FacilityGenerator?.IsGenerated == true)
        {
            return Core.FacilityGenerator.CurrentSeedString;
        }

        return $"Site_{DateTime.Now:yyyyMMdd_HHmmss}";
    }

    private PersistMetadata CreateCurrentMetadata(string siteName)
    {
        Vector3 playerPos = Vector3.zero;
        if (Core.Player?.Controller != null)
        {
            playerPos = Core.Player.Controller.transform.position;
        }

        var metadata = new PersistMetadata
        {
            saveName = siteName,
            saveTime = DateTime.Now,
            playerPosition = playerPos.ToString()
        };

        if (Core.FacilityGenerator?.IsGenerated == true)
        {
            metadata.seedString = Core.FacilityGenerator.CurrentSeedString;
            metadata.seed = Core.FacilityGenerator.CurrentSeed;
        }

        return metadata;
    }
}