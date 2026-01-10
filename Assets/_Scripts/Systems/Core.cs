//using Facility.Generation;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Core
{
    private static Player _player;
    private static UIAccess _ui;

    private static AudioDataAccess _audioDataAccess;

    //private static FacilityGenerator _facilityGenerator;
    //private static CullingSystem _cullingSystem;
    private static AudioManager _audioManager;
    private static CameraManager _cameraManager;
    private static EventManager _eventManager;
    private static FacilityManager _facilityManager;
    private static GameManager _gameManager;
    private static HintManager _hintManager;
    private static InventoryManager _inventoryManager;
    private static ProgressManager _progressManager;
    private static SettingsManager _settingsManager;

    private static bool _hasSubscribedToSceneEvents = false;
    private static bool _isQuitting = false;
    private static bool _isChangingScenes = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        _isQuitting = false;
        _isChangingScenes = false;
        Application.quitting += OnApplicationQuitting;
    }

    private static T GetOrCreateManager<T>(ref T field, Func<T> getInstance, string managerName) where T : class
    {
        if (field == null && !_isQuitting)
        {
            try
            {
                field = getInstance();
                if (field == null && !_isChangingScenes)
                {
                    Log.VerboseWarning($"Core: {managerName} not found in scene.");
                }
            }
            catch (NullReferenceException ex)
            {
                Log.Editor($"Core: NullReferenceException accessing {managerName} during shutdown: {ex.Message}");
            }
        }
        return field;
    }

    private static T LoadAddressableAccessor<T>(ref T field, Func<T> loader, string accessorName) where T : class
    {
        if (field == null && !_isQuitting)
        {
            try
            {
                field = loader();
                if (field == null && !_isChangingScenes)
                {
                    Log.VerboseWarning($"Core: {accessorName} could not be loaded.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Core: Error loading {accessorName}: {ex.Message}");
            }
        }
        return field;
    }

    public static Player Player
    {
        get
        {
            EnsureSceneEventSubscription();

            if (_player == null && !_isQuitting)
            {
                try
                {
                    _player = Player.Instance;

                    if (_player == null)
                    {
                        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                        if (playerObject != null)
                        {
                            _player = playerObject.GetComponent<Player>();
                        }
                    }

                    if (_player == null && !_isChangingScenes)
                    {
                        Log.VerboseWarning("Core: Could not find Player instance in scene.");
                    }
                }
                catch (NullReferenceException ex)
                {
                    Log.Editor($"Core: NullReferenceException accessing Player during shutdown: {ex.Message}");
                }
            }

            return _player;
        }
    }

    public static UIAccess UI => GetOrCreateManager(ref _ui, () => UIAccess.Instance, "UIAccess");

    public static AudioDataAccess AudioDataAccess => LoadAddressableAccessor(ref _audioDataAccess, AudioDataAccess.GetInstanceSync, "AudioDataAccess");

    //public static FacilityGenerator FacilityGenerator => GetOrCreateManager(ref _facilityGenerator, () => FacilityGenerator.Instance, "FacilityGenerator");
    //public static CullingSystem CullingSystem => GetOrCreateManager(ref _cullingSystem, () => CullingSystem.Instance, "CullingSystem");
    public static AudioManager AudioManager => GetOrCreateManager(ref _audioManager, () => AudioManager.Instance, "AudioManager");
    public static CameraManager CameraManager => GetOrCreateManager(ref _cameraManager, () => CameraManager.Instance, "CameraManager");
    public static EventManager EventManager => GetOrCreateManager(ref _eventManager, () => EventManager.Instance, "EventManager");
    public static FacilityManager FacilityManager => GetOrCreateManager(ref _facilityManager, () => FacilityManager.Instance, "FacilityManager");
    public static GameManager GameManager => GetOrCreateManager(ref _gameManager, () => GameManager.Instance, "GameManager");
    public static HintManager HintManager => GetOrCreateManager(ref _hintManager, () => HintManager.Instance, "HintManager");
    public static InventoryManager InventoryManager => GetOrCreateManager(ref _inventoryManager, () => InventoryManager.Instance, "InventoryManager");
    public static ProgressManager ProgressManager => GetOrCreateManager(ref _progressManager, () => ProgressManager.Instance, "ProgressManager");
    public static SettingsManager SettingsManager => GetOrCreateManager(ref _settingsManager, () => SettingsManager.Instance, "SettingsManager");

    public static void ClearCache()
    {
        _player = null;
        _ui = null;

        //_facilityGenerator = null;
        //_cullingSystem = null;
        _audioManager = null;
        _cameraManager = null;
        _eventManager = null;
        _facilityManager = null;
        _gameManager = null;
        _hintManager = null;
        _inventoryManager = null;
        _progressManager = null;
    }

    public static void ReleaseAddressableAssets()
    {
        if (_audioDataAccess != null)
        {
            AudioDataAccess.Release();
            _audioDataAccess = null;
        }
    }

    private static void EnsureSceneEventSubscription()
    {
        if (!_hasSubscribedToSceneEvents)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            _hasSubscribedToSceneEvents = true;
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _isChangingScenes = false;
        ClearCache();
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        _isChangingScenes = true;
    }

    private static void OnApplicationQuitting()
    {
        _isQuitting = true;
        ReleaseAddressableAssets();
        Application.quitting -= OnApplicationQuitting;
    }
}