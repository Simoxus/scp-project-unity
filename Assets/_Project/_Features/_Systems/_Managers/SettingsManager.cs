using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class SettingSetting
{
    public string key;
    public string value;
}

[Serializable]
public class SettingCategory
{
    public string categoryName;
    public List<SettingSetting> settings = new List<SettingSetting>();
}

[Serializable]
public class SettingsData
{
    public List<SettingCategory> categories = new List<SettingCategory>();
}

public class SettingsManager : Singleton<SettingsManager>
{
    private const string SETTINGS_FILE_NAME = "settings.json";

    private SettingsData settingsData;
    private string settingsFolder;
    private string settingsFilePath;
    private Dictionary<string, int> categoryPriorityMap = new Dictionary<string, int>();
    private bool isInitialized = false;

    protected override void OnSingletonAwake()
    {
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        if (isInitialized) return;

        string buildFolder = Directory.GetParent(Application.dataPath).FullName;
        settingsFolder = buildFolder;
        settingsFilePath = Path.Combine(settingsFolder, SETTINGS_FILE_NAME);

        LoadFromFile();
        isInitialized = true;
    }

    // Ensure initialization before any operation
    private void EnsureInitialized()
    {
        if (!isInitialized)
        {
            InitializeSettings();
        }
    }

    private void LoadFromFile()
    {
        if (File.Exists(settingsFilePath))
        {
            try
            {
                string json = File.ReadAllText(settingsFilePath);
                settingsData = JsonConvert.DeserializeObject<SettingsData>(json);

                if (settingsData == null || settingsData.categories == null)
                {
                    settingsData = new SettingsData();
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                settingsData = new SettingsData();
            }
        }
        else
        {
            settingsData = new SettingsData();
        }
    }

    private SettingCategory GetOrCreateCategory(string categoryName)
    {
        EnsureInitialized();

        var category = settingsData.categories.Find(c => c.categoryName == categoryName);

        if (category == null)
        {
            category = new SettingCategory { categoryName = categoryName };
            settingsData.categories.Add(category);
        }

        return category;
    }

    private void SetValue(string category, string key, string value)
    {
        EnsureInitialized();

        var cat = GetOrCreateCategory(category);
        var setting = cat.settings.Find(s => s.key == key);

        if (setting == null)
        {
            cat.settings.Add(new SettingSetting { key = key, value = value });
        }
        else
        {
            setting.value = value;
        }
    }

    private string GetValue(string category, string key, string defaultValue)
    {
        EnsureInitialized();

        var cat = settingsData.categories.Find(c => c.categoryName == category);

        if (cat != null)
        {
            var setting = cat.settings.Find(s => s.key == key);
            if (setting != null)
            {
                return setting.value;
            }
        }

        return defaultValue;
    }

    public void SaveInt(string category, string key, int value)
    {
        SetValue(category, key, value.ToString());
    }

    public int LoadInt(string category, string key, int defaultValue = 0)
    {
        string value = GetValue(category, key, defaultValue.ToString());
        return int.TryParse(value, out int result) ? result : defaultValue;
    }

    public void SaveFloat(string category, string key, float value)
    {
        SetValue(category, key, value.ToString());
    }

    public float LoadFloat(string category, string key, float defaultValue = 0f)
    {
        string value = GetValue(category, key, defaultValue.ToString());
        return float.TryParse(value, out float result) ? result : defaultValue;
    }

    public void SaveBool(string category, string key, bool value)
    {
        SetValue(category, key, value.ToString());
    }

    public bool LoadBool(string category, string key, bool defaultValue = false)
    {
        string value = GetValue(category, key, defaultValue.ToString());
        return bool.TryParse(value, out bool result) ? result : defaultValue;
    }

    public void SaveString(string category, string key, string value)
    {
        SetValue(category, key, value);
    }

    public string LoadString(string category, string key, string defaultValue = "")
    {
        return GetValue(category, key, defaultValue);
    }

    public void Save()
    {
        EnsureInitialized();

        try
        {
            settingsData.categories = settingsData.categories
                .OrderByDescending(c => categoryPriorityMap.ContainsKey(c.categoryName)
                    ? categoryPriorityMap[c.categoryName]
                    : int.MinValue)
                .ToList();

            string json = JsonConvert.SerializeObject(settingsData, Formatting.Indented);
            File.WriteAllText(settingsFilePath, json);
        }
        catch (Exception ex)
        {
            Log.Exception(ex);
        }
    }

    public void RegisterCategory(string categoryName, int priority)
    {
        EnsureInitialized();

        if (!categoryPriorityMap.ContainsKey(categoryName))
        {
            categoryPriorityMap[categoryName] = priority;
        }

        GetOrCreateCategory(categoryName);
    }

    public void ResetCategory(string categoryName)
    {
        EnsureInitialized();

        var category = settingsData.categories.Find(c => c.categoryName == categoryName);

        if (category != null)
        {
            settingsData.categories.Remove(category);
            Save();

            Log.VerboseInfo($"Settings category '{categoryName}' has been reset");
        }
    }

    public void ResetAllSettings()
    {
        EnsureInitialized();

        settingsData = new SettingsData();

        if (File.Exists(settingsFilePath))
        {
            File.Delete(settingsFilePath);
            Log.VerboseInfo($"'{SETTINGS_FILE_NAME}' has been deleted successfully");
        }
    }

    protected override void OnSingletonApplicationQuit()
    {
        Save();
    }

    public void OpenSettingsFolder()
    {
        EnsureInitialized();

        if (Directory.Exists(settingsFolder))
        {
            Application.OpenURL("file://" + settingsFolder);
            Log.VerboseInfo($"Requesting that path to '{SETTINGS_FILE_NAME}' is opened");
        }
    }

    public string GetSettingsFolderPath()
    {
        EnsureInitialized();

        Log.VerboseInfo($"Current '{SETTINGS_FILE_NAME}' folder path: {settingsFolder}");
        return settingsFolder;
    }
}