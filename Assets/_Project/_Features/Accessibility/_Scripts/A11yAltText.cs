using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Alt-text registry for visual content (images with baked-in text, scene descriptions).
/// Data lives in Resources/A11y/alt_texts.json so QA can edit descriptions without recompiling.
/// Keys are case-insensitive; "scene:{name}" keys describe whole screens on load.
/// </summary>
public static class A11yAltText
{
    [Serializable]
    public class AltTextEntry
    {
        public string key;
        public string text;
    }

    [Serializable]
    public class AltTextData
    {
        public AltTextEntry[] entries;
    }

    private static Dictionary<string, string> _entries;

    public static bool TryGet(string key, out string text)
    {
        EnsureLoaded();
        if (!string.IsNullOrEmpty(key) && _entries.TryGetValue(key.Trim().ToLowerInvariant(), out text))
        {
            return true;
        }
        text = null;
        return false;
    }

    /// <summary>Fallback when no alt text is registered: turn an internal object name into something speakable.</summary>
    public static string HumanizeName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return string.Empty;
        return rawName.Replace("(Clone)", string.Empty).Replace('_', ' ').Trim();
    }

    private static void EnsureLoaded()
    {
        if (_entries != null) return;
        _entries = new Dictionary<string, string>();

        var json = Resources.Load<TextAsset>("A11y/alt_texts");
        if (json == null)
        {
            Debug.LogWarning("[Accessibility] Resources/A11y/alt_texts.json not found; alt text lookups will all fall back to object names.");
            return;
        }

        var data = JsonUtility.FromJson<AltTextData>(json.text);
        if (data?.entries == null) return;

        foreach (var entry in data.entries)
        {
            if (string.IsNullOrEmpty(entry.key) || string.IsNullOrEmpty(entry.text)) continue;
            _entries[entry.key.Trim().ToLowerInvariant()] = entry.text;
        }
    }
}
