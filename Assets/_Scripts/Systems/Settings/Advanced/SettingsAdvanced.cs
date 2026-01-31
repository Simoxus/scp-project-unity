using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class SettingsAdvanced : BaseSettings
{
    public override string CATEGORY => "Advanced";

    [HideInInspector] public List<Locale> availableLocales = new List<Locale>();

    [Space]
    public SettingsAdvancedApplier applier;

    [Header("UI Elements")]
    public TMP_Dropdown gameLanguageDropdown;
    public Toggle showHUDToggle;
    public Toggle showFPSToggle;
    public Toggle showCrosshairToggle;
    public Toggle animateOutlinesToggle;
    public Toggle showAchievementPopupsToggle;
    public Toggle enableConsoleToggle;
    public Toggle openConsoleOnErrorToggle;

    protected override void InitializeReferences()
    {
        if (applier == null)
        {
            applier = GetComponent<SettingsAdvancedApplier>();
        }

        if (gameLanguageDropdown != null)
        {
            PopulateLanguageDropdown();
        }
    }

    public override void SaveSettings()
    {
        SettingsManager settingsManager = Core.SettingsManager;
        if (settingsManager == null) return;

        settingsManager.SaveInt(CATEGORY, "GameLanguage", gameLanguageDropdown.value);
        settingsManager.SaveBool(CATEGORY, "ShowHUD", showHUDToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "ShowFPS", showFPSToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "ShowCrosshair", showCrosshairToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "AnimateOutlines", animateOutlinesToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "ShowAchievementPopups", showAchievementPopupsToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "EnableConsole", enableConsoleToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "OpenConsoleOnError", openConsoleOnErrorToggle.isOn);

        settingsManager.Save();
    }

    public override void LoadSettings()
    {
        SettingsManager settingsManager = Core.SettingsManager;
        if (settingsManager == null) return;

        applier.inBatchMode = true;

        // Load settings with different defaults for editor vs build
        gameLanguageDropdown.value = settingsManager.LoadInt(CATEGORY, "GameLanguage", 0);
        showHUDToggle.isOn = settingsManager.LoadBool(CATEGORY, "ShowHUD", true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        showFPSToggle.isOn = settingsManager.LoadBool(CATEGORY, "ShowFPS", true);
#else
        showFPSToggle.isOn = settingsManager.LoadBool(CATEGORY, "ShowFPS", false);
#endif

        showCrosshairToggle.isOn = settingsManager.LoadBool(CATEGORY, "ShowCrosshair", false);
        animateOutlinesToggle.isOn = settingsManager.LoadBool(CATEGORY, "AnimateOutlines", true);
        showAchievementPopupsToggle.isOn = settingsManager.LoadBool(CATEGORY, "ShowAchievementPopups", true);
        enableConsoleToggle.isOn = settingsManager.LoadBool(CATEGORY, "EnableConsole", true);
        openConsoleOnErrorToggle.isOn = settingsManager.LoadBool(CATEGORY, "OpenConsoleOnError", false);

        // Apply all settings
        applier.ApplyGameLanguage(gameLanguageDropdown.value);
        applier.ApplyShowHUD(showHUDToggle.isOn);
        applier.ApplyShowFPS(showFPSToggle.isOn);
        applier.ApplyShowCrosshair(showCrosshairToggle.isOn);
        applier.ApplyAnimateOutlines(animateOutlinesToggle.isOn);
        applier.ApplyShowAchievementPopups(showAchievementPopupsToggle.isOn);
        applier.ApplyEnableConsole(enableConsoleToggle.isOn);
        applier.ApplyOpenConsoleOnError(openConsoleOnErrorToggle.isOn);

        applier.inBatchMode = false;
    }

    public void PopulateLanguageDropdown()
    {
        gameLanguageDropdown.ClearOptions();
        availableLocales.Clear();

        List<string> options = new List<string>();
        var locales = LocalizationSettings.AvailableLocales.Locales;

        foreach (var locale in locales)
        {
            availableLocales.Add(locale);
            options.Add($"{locale.Identifier.CultureInfo.NativeName} ({locale.Identifier.Code})");
        }

        gameLanguageDropdown.AddOptions(options);

        // Set current language as the selected option
        Locale currentLocale = LocalizationSettings.SelectedLocale;
        int currentLocaleIndex = availableLocales.IndexOf(currentLocale);

        if (currentLocaleIndex >= 0)
        {
            gameLanguageDropdown.value = currentLocaleIndex;
        }
        else
        {
            gameLanguageDropdown.value = 0;
        }

        gameLanguageDropdown.RefreshShownValue();
    }
}