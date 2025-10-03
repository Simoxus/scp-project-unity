using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveInt(string category, string key, int value)
    {
        PlayerPrefs.SetInt($"{category}.{key}", value);
    }

    public int LoadInt(string category, string key, int defaultValue = 0)
    {
        return PlayerPrefs.GetInt($"{category}.{key}", defaultValue);
    }

    public void SaveFloat(string category, string key, float value)
    {
        PlayerPrefs.SetFloat($"{category}.{key}", value);
    }

    public float LoadFloat(string category, string key, float defaultValue = 0f)
    {
        return PlayerPrefs.GetFloat($"{category}.{key}", defaultValue);
    }

    public void SaveBool(string category, string key, bool value)
    {
        PlayerPrefs.SetInt($"{category}.{key}", value ? 1 : 0);
    }

    public bool LoadBool(string category, string key, bool defaultValue = false)
    {
        return PlayerPrefs.GetInt($"{category}.{key}", defaultValue ? 1 : 0) == 1;
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }
}

