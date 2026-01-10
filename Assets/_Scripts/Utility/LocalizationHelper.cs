using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class LocalizationHelper
{
    private static readonly Dictionary<string, StringTable> _tableCache = new();
    private static bool _isInitialized = false;

    public static event Action LocaleChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (_isInitialized) return;

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
        _isInitialized = true;
    }

    private static void OnLocaleChanged(Locale newLocale)
    {
        _tableCache.Clear();
        Log.Editor($"Language changed to: {newLocale.name}");
        LocaleChanged?.Invoke();
    }

    public static string GetString(string tableName, string key)
    {
        if (!TryGetTable(tableName, out var table))
        {
            Log.Warning($"Table '{tableName}' not found!");
            return null;
        }

        var entry = table.GetEntry(key);
        if (entry == null)
        {
            Log.Warning($"Key '{key}' not found in table '{tableName}'!");
            return null;
        }

        return entry.GetLocalizedString();
    }

    public static string GetString(string tableName, string key, params object[] args)
    {
        var template = GetString(tableName, key);
        if (template == null) return null;

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException e)
        {
            Log.Error($"Format error for key '{key}': {e.Message}");
            return template;
        }
    }

    public static string GetStringOrDefault(string tableName, string key, string defaultValue = "")
    {
        return GetString(tableName, key) ?? defaultValue;
    }

    public static bool TryGetString(string tableName, string key, out string result)
    {
        result = GetString(tableName, key);
        return result != null;
    }

    public static async UniTask<string> GetStringAsync(string tableName, string key)
    {
        var handle = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(tableName, key);

        if (handle.IsDone)
        {
            return handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null;
        }

        await handle.ToUniTask();

        if (handle.Status == AsyncOperationStatus.Failed)
        {
            Log.Warning($"Failed to load key '{key}' from table '{tableName}'");
            return null;
        }

        return handle.Result;
    }

    public static async UniTask<string> GetStringAsync(string tableName, string key, params object[] args)
    {
        var template = await GetStringAsync(tableName, key);
        return FormatString(template, key, args);
    }

    private static string FormatString(string template, string key, object[] args)
    {
        if (template == null) return null;
        if (args == null || args.Length == 0) return template;

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException e)
        {
            Log.Error($"Format error for key '{key}': {e.Message}");
            return template;
        }
    }

    private static bool TryGetTable(string tableName, out StringTable table)
    {
        // Check cache first
        if (_tableCache.TryGetValue(tableName, out table))
        {
            return table != null;
        }

        table = LocalizationSettings.StringDatabase.GetTable(tableName);
        _tableCache[tableName] = table;
        return table != null;
    }

    public static void ChangeLanguage(string localeCode)
    {
        var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);
        if (locale != null)
        {
            LocalizationSettings.SelectedLocale = locale;
        }
        else
        {
            Log.Error($"Locale '{localeCode}' not found!");
        }
    }

    public static string GetCurrentLanguage()
    {
        return LocalizationSettings.SelectedLocale?.Identifier.Code ?? "en";
    }

    public static bool HasKey(string tableName, string key)
    {
        return TryGetTable(tableName, out var table) && table.GetEntry(key) != null;
    }

    public static void ClearCache()
    {
        _tableCache.Clear();
    }
}