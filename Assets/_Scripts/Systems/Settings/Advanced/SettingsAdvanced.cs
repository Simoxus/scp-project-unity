using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SettingsAdvanced : MonoBehaviour
{
    public const string CATEGORY = "Advanced";

    [Header("References")]
    public SettingsAdvancedApplier applier;

    public Toggle showHUDToggle;
    public Toggle showFPSToggle;
    public Toggle showCrosshairToggle;
    public Toggle showAchievementPopupsToggle;

    public Toggle enableConsoleToggle;
    public Toggle openConsoleOnErrorToggle;

    private bool _isWaitingToSave = false;

    private void Start()
    {
        if (applier == null)
        {
            applier = GetComponent<SettingsAdvancedApplier>();
        }

        LoadSettings();
    }

    public void SaveSettings()
    {
        var sm = SettingsManager.Instance;

        sm.SaveBool(CATEGORY, "ShowHUD", showHUDToggle.isOn);
        sm.SaveBool(CATEGORY, "ShowFPS", showFPSToggle.isOn);
        sm.SaveBool(CATEGORY, "ShowCrosshair", showCrosshairToggle.isOn);
        sm.SaveBool(CATEGORY, "ShowAchievementPopups", showAchievementPopupsToggle.isOn);

        sm.SaveBool(CATEGORY, "EnableConsole", enableConsoleToggle.isOn);
        sm.SaveBool(CATEGORY, "OpenConsoleOnError", openConsoleOnErrorToggle.isOn);

        sm.Save();
    }

    public void LoadSettings()
    {
        var sm = SettingsManager.Instance;

        showHUDToggle.isOn = sm.LoadBool(CATEGORY, "ShowHUD", true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        showFPSToggle.isOn = sm.LoadBool(CATEGORY, "ShowFPS", true);
#else
        showFPSToggle.isOn = sm.LoadBool(CATEGORY, "ShowFPS", false);
#endif

        showCrosshairToggle.isOn = sm.LoadBool(CATEGORY, "ShowCrosshair", true);
        showAchievementPopupsToggle.isOn = sm.LoadBool(CATEGORY, "ShowAchievementPopups", true);

        enableConsoleToggle.isOn = sm.LoadBool(CATEGORY, "EnableConsole", true);
        openConsoleOnErrorToggle.isOn = sm.LoadBool(CATEGORY, "OpenConsoleOnError", false);

        applier.inBatchMode = true;

        applier.ApplyShowHUD(showHUDToggle.isOn);
        applier.ApplyShowFPS(showFPSToggle.isOn);
        applier.ApplyShowCrosshair(showCrosshairToggle.isOn);
        applier.ApplyShowAchievementPopups(showAchievementPopupsToggle.isOn);
        applier.ApplyEnableConsole(enableConsoleToggle.isOn);
        applier.ApplyOpenConsoleOnError(openConsoleOnErrorToggle.isOn);

        applier.inBatchMode = false;

        SaveSettings();
    }

    public void ResetCategorySettings()
    {
        if (SettingsManager.Instance == null) return;

        SettingsManager.Instance.ResetCategory(CATEGORY);

        LoadSettings();
        SaveSettings();
    }

    public async void SaveSettingsWithDelay(float delay = 0.5f)
    {
        if (_isWaitingToSave) { return; }

        _isWaitingToSave = true;

        float elapsedTime = 0f;
        while (elapsedTime < delay)
        {
            await UniTask.Yield();
            elapsedTime += Time.unscaledDeltaTime;

            // If another call comes in, reset the saving timer
            if (!_isWaitingToSave) { return; }
        }

        SaveSettings();
        _isWaitingToSave = false;
    }
}