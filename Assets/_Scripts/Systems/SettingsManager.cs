using System;
using System.Collections.Generic;
using System.IO;
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

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private SettingsData settingsData;
    private string settingsFolder;
    private string settingsFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeSettings()
    {
        string buildFolder = Directory.GetParent(Application.dataPath).FullName;
        settingsFolder = buildFolder;
        settingsFilePath = Path.Combine(settingsFolder, "settings.json");

        LoadFromFile();
    }

    private void LoadFromFile()
    {
        if (File.Exists(settingsFilePath))
        {
            try
            {
                string json = File.ReadAllText(settingsFilePath);
                settingsData = JsonUtility.FromJson<SettingsData>(json);

                if (settingsData == null || settingsData.categories == null)
                {
                    settingsData = new SettingsData();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load settings: {e.Message}");
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
        try
        {
            string json = JsonUtility.ToJson(settingsData, true);
            File.WriteAllText(settingsFilePath, json);
        }
        catch (Exception)
        {

        }
    }

    public void ResetAllSettings()
    {
        settingsData = new SettingsData();

        if (File.Exists(settingsFilePath))
        {
            File.Delete(settingsFilePath);
        }
    }

    public void ResetCategory(string categoryName)
    {
        var category = settingsData.categories.Find(c => c.categoryName == categoryName);

        if (category != null)
        {
            settingsData.categories.Remove(category);
            Save();

            Log.VerboseInfo($"Category '{categoryName}' has been reset.");
        }
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    public void OpenSettingsFolder()
    {
        if (Directory.Exists(settingsFolder))
        {
            Application.OpenURL("file://" + settingsFolder);
        }
    }

    public string GetSettingsFolderPath()
    {
        return settingsFolder;
    }
}