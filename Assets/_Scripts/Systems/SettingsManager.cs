using System;
using UnityEngine;
using Unity.Cinemachine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Graphics Settings")]
    public int resolutionIndex = 0; // Index into Screen.resolutions array
    public FullScreenMode fullScreenMode = FullScreenMode.FullScreenWindow;

    [Header("Gameplay Settings")]
    [Range(0.1f, 10f)]
    public float mouseSensitivity = 1.0f;
    public bool invertMouseY = false;

    public event Action OnGraphicsSettingsChanged;
    public event Action OnGameplaySettingsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep settings persistent across scenes

        // Load settings when the game starts
        LoadSettings();
        ApplyAllSettings(); // Apply the loaded (or default) settings immediately
    }

    private void OnApplicationQuit()
    {
        // Save settings when the application closes
        SaveSettings();
    }

    public void SaveSettings()
    {
        SettingsPersistence.SetInt(SettingsPersistence.Keys.ResolutionIndex, resolutionIndex);
        SettingsPersistence.SetInt(SettingsPersistence.Keys.FullScreenMode, (int)fullScreenMode);

        SettingsPersistence.SetFloat(SettingsPersistence.Keys.MouseSensitivity, mouseSensitivity);
        SettingsPersistence.SetBool(SettingsPersistence.Keys.InvertMouseY, invertMouseY);

        SettingsPersistence.Save(); // Ensures all changes are written to disk
        Debug.Log("SettingsManager has saved user preferences.");
    }

    public void LoadSettings()
    {
        resolutionIndex = SettingsPersistence.GetInt(SettingsPersistence.Keys.ResolutionIndex, GetDefaultResolutionIndex());
        fullScreenMode = (FullScreenMode)SettingsPersistence.GetInt(SettingsPersistence.Keys.FullScreenMode, (int)FullScreenMode.ExclusiveFullScreen);

        mouseSensitivity = SettingsPersistence.GetFloat(SettingsPersistence.Keys.MouseSensitivity, 1.0f);
        invertMouseY = SettingsPersistence.GetBool(SettingsPersistence.Keys.InvertMouseY, false);

        Debug.Log("SettingManager has loaded user preferences.");
    }

    public void ApplyAllSettings()
    {
        ApplyGraphicsSettings();
        ApplyGameplaySettings(); // This would typically involve notifying PlayerManager
        Debug.Log("SettingsManager has applied all settings.");
    }

    private void ApplyGraphicsSettings()
    {
        // Apply resolution and fullscreen mode
        if (Screen.resolutions.Length > 0 && resolutionIndex >= 0 && resolutionIndex < Screen.resolutions.Length)
        {
            Resolution selectedResolution = Screen.resolutions[resolutionIndex];
            Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullScreenMode);
        }
        else
        {
            Debug.LogWarning("SettingsManager preference has invalid resolution index, falling back to current screen resolution.");
            Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullScreenMode);
        }

        // Trigger a general graphics settings changed event
        OnGraphicsSettingsChanged?.Invoke();
    }

    private void ApplyGameplaySettings()
    {
        OnGameplaySettingsChanged?.Invoke();
    }

    public void SetResolution(int index)
    {
        resolutionIndex = index;
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SetFullScreenMode(FullScreenMode mode)
    {
        fullScreenMode = mode;
        ApplyGraphicsSettings();
        SaveSettings();
    }

    public void SetMouseSensitivity(float value)
    {
        mouseSensitivity = value;
        ApplyGameplaySettings();
        SaveSettings();
    }

    public void SetInvertMouseY(bool value)
    {
        invertMouseY = value;
        ApplyGameplaySettings();
        SaveSettings();
    }

    // Helper to get a good default resolution if PlayerPrefs doesn't have one
    private int GetDefaultResolutionIndex()
    {
        Resolution currentRes = Screen.currentResolution;
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            if (Screen.resolutions[i].width == currentRes.width &&
                Screen.resolutions[i].height == currentRes.height)
            {
                return i;
            }
        }
        return 0; // Fallback to the first resolution
    }

    public static class SettingsPersistence
    {
        // Nested static class for PlayerPrefs keys to avoid magic strings
        public static class Keys
        {
            // Graphics
            public const string ResolutionIndex = "Settings_ResolutionIndex";
            public const string FullScreenMode = "Settings_FullScreenMode";
            public const string FieldOfView = "Settings_FieldOfView";

            // Gameplay
            public const string MouseSensitivity = "Settings_MouseSensitivity";
            public const string InvertMouseY = "Settings_InvertMouseY";

            // Add more keys as you add more settings
        }

        // --- Generic Getters ---
        public static int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public static float GetFloat(string key, float defaultValue)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public static string GetString(string key, string defaultValue)
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            // PlayerPrefs doesn't have native bools, so we store 0 for false, 1 for true
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        // --- Generic Setters ---
        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public static void SetBool(string key, bool value)
        {
            // Store bool as int (0 or 1)
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        /// <summary>
        /// Writes all modified PlayerPrefs to disk. Call this after making changes.
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Deletes all keys and values from PlayerPrefs. Use with caution!
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("All PlayerPrefs deleted.");
        }
    }
}