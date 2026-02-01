using Cysharp.Threading.Tasks;
using UnityEngine;

public class SettingsAdvancedApplier : BaseSettingsApplier
{
    [SerializeField] private SettingsAdvanced settingsAdvanced;

    private bool _hasReapplied = false;

    protected override void InitializeReferences()
    {
        if (settingsAdvanced == null)
        {
            settingsAdvanced = GetComponent<SettingsAdvanced>();
        }
    }

    private void Start()
    {
        ReapplyUISettingsDelayed().Forget();
    }

    private async UniTaskVoid ReapplyUISettingsDelayed()
    {
        if (_hasReapplied) return;

        // Wait for a few frames to ensure UI is fully initialized
        await UniTask.DelayFrame(3);

        // Reapply all UI-dependent settings
        if (Core.SettingsManager != null)
        {
            bool showCrosshair = Core.SettingsManager.LoadBool("Advanced", "ShowCrosshair", false);
            bool showHUD = Core.SettingsManager.LoadBool("Advanced", "ShowHUD", true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            bool showFPS = Core.SettingsManager.LoadBool("Advanced", "ShowFPS", true);
#else
            bool showFPS = Core.SettingsManager.LoadBool("Advanced", "ShowFPS", false);
#endif

            inBatchMode = true;

            ApplyShowCrosshair(showCrosshair);
            ApplyShowHUD(showHUD);
            ApplyShowFPS(showFPS);

            inBatchMode = false;

            _hasReapplied = true;
        }
    }

    public void ApplyGameLanguage(int index)
    {
        if (index < 0 || index >= settingsAdvanced.availableLocales.Count)
        {
            Log.VerboseWarning($"Invalid language index: {index}");
            return;
        }

        var selectedLocale = settingsAdvanced.availableLocales[index];
        LocalizationHelper.ChangeLanguage(selectedLocale.Identifier.Code);

        if (inBatchMode == false) settingsAdvanced.SaveSettings();
    }

    public void ApplyShowHUD(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = Core.UI;
        if (uiAccess != null && uiAccess.Indicators != null && uiAccess.Interact != null)
        {
            if (enabled)
            {
                uiAccess.Indicators.Show();
            }
            else
            {
                uiAccess.Indicators.Hide();
            }

            uiAccess.Interact.canvasGroup.gameObject.SetActive(enabled);
        }

        if (inBatchMode == false) settingsAdvanced.SaveSettings();
    }

    public void ApplyShowFPS(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = Core.UI;
        if (uiAccess != null && uiAccess.FpsCounter != null)
        {
            uiAccess.FpsCounter.gameObject.SetActive(enabled);
            uiAccess.FpsCounter.enabled = enabled;
        }

        if (inBatchMode == false) settingsAdvanced.SaveSettings();
    }

    public void ApplyShowCrosshair(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = Core.UI;
        if (uiAccess != null && uiAccess.Crosshair != null)
        {
            uiAccess.Crosshair.gameObject.SetActive(enabled);
        }

        if (inBatchMode == false) settingsAdvanced.SaveSettings();
    }

    public void ApplyAnimateOutlines(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        //Outline.GlobalFadingEnabled = enabled;

        if (inBatchMode == false) settingsAdvanced.SaveSettings();
    }

    public void ApplyShowAchievementPopups(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        // TODO

        if (inBatchMode == false) settingsAdvanced.SaveSettings();
    }

    public void ApplyEnableConsole(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = Core.UI;
        if (uiAccess != null && uiAccess.Console != null)
        {
            uiAccess.Console.ForceClose();
            uiAccess.Console.enabled = enabled;
        }

        if (settingsAdvanced.openConsoleOnErrorToggle != null)
        {
            settingsAdvanced.openConsoleOnErrorToggle.gameObject.SetActive(enabled);
        }

        if (inBatchMode == false) settingsAdvanced.SaveSettings();
    }

    public void ApplyOpenConsoleOnError(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        // TODO

        if (inBatchMode == false) settingsAdvanced.SaveSettings();
    }
}