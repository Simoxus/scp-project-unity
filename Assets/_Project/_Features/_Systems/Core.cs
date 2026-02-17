using Facility.Generation;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Core
{
    public const string COMMAND_NAMESPACE = "Console.Commands";
    public const string PERSIST_DATA_NAMESPACE = "Facility.Persistence.Types";

    private static Player _player;
    private static UIAccess _ui;

    private static AudioDataAccess _audioDataAccess;

    private static FacilityGenerator _facilityGenerator;
    private static CullingSystem _cullingSystem;
    private static AudioManager _audioManager;
    private static CameraManager _cameraManager;
    private static ConsoleManager _consoleManager;
    private static EventManager _eventManager;
    private static FacilityManager _facilityManager;
    private static GameManager _gameManager;
    private static HintManager _hintManager;
    private static LoadingManager _loadingManager;
    private static ModManager _modManager;
    private static MusicManager _musicManager;
    private static ProgressManager _progressManager;
    private static PersistenceManager _persistenceManager;
    private static SettingsManager _settingsManager;

    private static bool _hasSubscribedToSceneEvents = false;
    private static bool _isQuitting = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        _isQuitting = false;
        Application.quitting += OnApplicationQuitting;
    }

    private static T GetOrCreateManager<T>(ref T field, Func<T> getInstance, string managerName) where T : class
    {
        if (field == null && !_isQuitting)
        {
            try
            {
                field = getInstance();
            }
            catch (NullReferenceException ex)
            {
                Log.Editor($"NullReferenceException accessing {managerName} during shutdown: {ex.Message}");
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
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading {accessorName}: {ex.Message}");
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
                }
                catch (NullReferenceException ex)
                {
                    Log.Editor($"NullReferenceException accessing Player during shutdown: {ex.Message}");
                }
            }

            return _player;
        }
    }

    public static UIAccess UI => GetOrCreateManager(ref _ui, () => UIAccess.Instance, "UIAccess");

    public static AudioDataAccess AudioDataAccess => LoadAddressableAccessor(ref _audioDataAccess, AudioDataAccess.GetInstanceSync, "AudioDataAccess");

    public static FacilityGenerator FacilityGenerator => GetOrCreateManager(ref _facilityGenerator, () => FacilityGenerator.Instance, "FacilityGenerator");
    public static CullingSystem CullingSystem => GetOrCreateManager(ref _cullingSystem, () => CullingSystem.Instance, "CullingSystem");
    public static AudioManager AudioManager => GetOrCreateManager(ref _audioManager, () => AudioManager.Instance, "AudioManager");
    public static CameraManager CameraManager => GetOrCreateManager(ref _cameraManager, () => CameraManager.Instance, "CameraManager");
    public static ConsoleManager ConsoleManager => GetOrCreateManager(ref _consoleManager, () => ConsoleManager.Instance, "ConsoleManager");
    public static EventManager EventManager => GetOrCreateManager(ref _eventManager, () => EventManager.Instance, "EventManager");
    public static FacilityManager FacilityManager => GetOrCreateManager(ref _facilityManager, () => FacilityManager.Instance, "FacilityManager");
    public static GameManager GameManager => GetOrCreateManager(ref _gameManager, () => GameManager.Instance, "GameManager");
    public static HintManager HintManager => GetOrCreateManager(ref _hintManager, () => HintManager.Instance, "HintManager");
    public static LoadingManager LoadingManager => GetOrCreateManager(ref _loadingManager, () => LoadingManager.Instance, "LoadingManager");
    public static ModManager ModManager => GetOrCreateManager(ref _modManager, () => ModManager.Instance, "ModManager");
    public static MusicManager MusicManager => GetOrCreateManager(ref _musicManager, () => MusicManager.Instance, "MusicManager");
    public static PersistenceManager PersistenceManager => GetOrCreateManager(ref _persistenceManager, () => PersistenceManager.Instance, "PersistenceManager");
    public static ProgressManager ProgressManager => GetOrCreateManager(ref _progressManager, () => ProgressManager.Instance, "ProgressManager");
    public static SettingsManager SettingsManager => GetOrCreateManager(ref _settingsManager, () => SettingsManager.Instance, "SettingsManager");

    public static void ClearCache()
    {
        _player = null;
        _ui = null;

        _facilityGenerator = null;
        _cullingSystem = null;
        _audioManager = null;
        _cameraManager = null;
        _consoleManager = null;
        _eventManager = null;
        _facilityManager = null;
        _gameManager = null;
        _hintManager = null;
        _modManager = null;
        _musicManager = null;
        _persistenceManager = null;
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
            _hasSubscribedToSceneEvents = true;
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ClearCache();
    }

    private static void OnApplicationQuitting()
    {
        _isQuitting = true;
        ReleaseAddressableAssets();
        Application.quitting -= OnApplicationQuitting;
    }
}