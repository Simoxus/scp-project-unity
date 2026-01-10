using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsInitializer : MonoBehaviour
{
    [System.Serializable]
    public class SettingsEntry
    {
        public string categoryName;
        public int priority;
        public BaseSettings settingsScript;
    }

    [SerializeField] private List<SettingsEntry> settingsEntries = new List<SettingsEntry>();

    private SettingsManager _settingsManager;
    private bool _hasAppliedSettings = false;

    private void Reset()
    {
        settingsEntries = new List<SettingsEntry>
        {
            new SettingsEntry { categoryName = "Graphics", priority = 3 },
            new SettingsEntry { categoryName = "Audio", priority = 2 },
            new SettingsEntry { categoryName = "Controls", priority = 1 },
            new SettingsEntry { categoryName = "Advanced", priority = 0 }
        };
    }

    private void Awake()
    {
        _settingsManager = SettingsManager.Instance;
        RegisterCategories();
    }

    private void Start()
    {
        // Apply settings AFTER all Awake calls are done
        ApplySettingsAsync().Forget();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _hasAppliedSettings = false;
        ApplySettingsAsync().Forget();
    }

    private void RegisterCategories()
    {
        if (_settingsManager == null) return;

        var sortedEntries = settingsEntries.OrderByDescending(e => e.priority).ToList();
        foreach (var entry in sortedEntries)
        {
            if (!string.IsNullOrEmpty(entry.categoryName))
            {
                _settingsManager.RegisterCategory(entry.categoryName, entry.priority);
            }
        }
    }

    private async UniTaskVoid ApplySettingsAsync()
    {
        if (_hasAppliedSettings) return;

        // Wait for dependencies to initialize
        await WaitForDependencies();

        var sortedEntries = settingsEntries.OrderByDescending(e => e.priority).ToList();

        foreach (var entry in sortedEntries)
        {
            if (entry.settingsScript != null)
            {
                entry.settingsScript.LoadSettings();
            }
        }

        _hasAppliedSettings = true;
    }

    private async UniTask WaitForDependencies()
    {
        // Wait for common dependencies to initialize
        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            bool allReady = true;
            bool isMainMenu = Core.GameManager && Core.GameManager.IsInMainMenu;

            if (!isMainMenu)
            {
                // Wait for Player if we're not in main menu
                if (Core.Player == null)
                {
                    allReady = false;
                }

                // Wait for UIAccess if needed
                if (UIAccess.Instance == null)
                {
                    allReady = false;
                }
            }

            if (AudioManager.Instance == null)
            {
                allReady = false;
            }

            if (allReady)
            {
                await UniTask.NextFrame();
                return;
            }

            await UniTask.WaitForSeconds(0.1f, true);
            attempts++;
        }

        Log.Warning("Timed out waiting for dependencies. Some settings may not apply correctly.");
    }

    public void ReapplyAllSettings()
    {
        _hasAppliedSettings = false;
        ApplySettingsAsync().Forget();
    }
}