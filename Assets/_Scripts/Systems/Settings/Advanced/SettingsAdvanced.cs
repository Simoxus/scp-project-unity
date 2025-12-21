using UnityEngine;
using UnityEngine.UI;

public class SettingsAdvanced : BaseSettings
{
    public override string CATEGORY => "Advanced";

    [Header("References")]
    public SettingsAdvancedApplier applier;

    [Header("UI Elements")]
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
    }

    public override void SaveSettings()
    {
        SettingsManager settingsManager = SettingsManager.Instance;
        if (settingsManager == null) return;

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
        SettingsManager settingsManager = SettingsManager.Instance;
        if (settingsManager == null) return;

        applier.inBatchMode = true;

        // Load settings with different defaults for editor vs build
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
        applier.ApplyShowHUD(showHUDToggle.isOn);
        applier.ApplyShowFPS(showFPSToggle.isOn);
        applier.ApplyShowCrosshair(showCrosshairToggle.isOn);
        applier.ApplyAnimateOutlines(animateOutlinesToggle.isOn);
        applier.ApplyShowAchievementPopups(showAchievementPopupsToggle.isOn);
        applier.ApplyEnableConsole(enableConsoleToggle.isOn);
        applier.ApplyOpenConsoleOnError(openConsoleOnErrorToggle.isOn);

        applier.inBatchMode = false;
    }
}