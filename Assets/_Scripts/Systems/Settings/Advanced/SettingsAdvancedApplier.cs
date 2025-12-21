using Cysharp.Threading.Tasks;
using UnityEngine;

public class SettingsAdvancedApplier : BaseSettingsApplier
{
    [Header("References")]
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
        if (SettingsManager.Instance != null)
        {
            bool showCrosshair = SettingsManager.Instance.LoadBool("Advanced", "ShowCrosshair", false);
            bool showHUD = SettingsManager.Instance.LoadBool("Advanced", "ShowHUD", true);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            bool showFPS = SettingsManager.Instance.LoadBool("Advanced", "ShowFPS", true);
#else
            bool showFPS = SettingsManager.Instance.LoadBool("Advanced", "ShowFPS", false);
#endif

            inBatchMode = true;

            ApplyShowCrosshair(showCrosshair);
            ApplyShowHUD(showHUD);
            ApplyShowFPS(showFPS);

            inBatchMode = false;

            _hasReapplied = true;
        }
    }

    public void ApplyShowHUD(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = UIAccess.Instance;
        if (uiAccess != null && uiAccess.canvasIndicators != null && uiAccess.canvasInteract != null)
        {
            uiAccess.canvasIndicators.SetActive(enabled);
            uiAccess.canvasInteract.SetActive(enabled);
        }

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyShowFPS(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = UIAccess.Instance;
        if (uiAccess != null && uiAccess.fpsCounter != null)
        {
            uiAccess.fpsCounter.gameObject.SetActive(enabled);
            uiAccess.fpsCounter.enabled = enabled;
        }

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyShowCrosshair(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = UIAccess.Instance;
        if (uiAccess != null && uiAccess.crosshair != null)
        {
            uiAccess.crosshair.gameObject.SetActive(enabled);
        }

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyAnimateOutlines(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        Outline.GlobalFadingEnabled = enabled;

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyShowAchievementPopups(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = UIAccess.Instance;

        // TODO: Implement achievement popup setting if needed

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyEnableConsole(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = UIAccess.Instance;
        if (uiAccess != null && uiAccess.uiDebugPopup != null)
        {
            uiAccess.uiDebugPopup.ForceClose();
            uiAccess.uiDebugPopup.enabled = enabled;
        }

        if (settingsAdvanced.openConsoleOnErrorToggle != null)
        {
            settingsAdvanced.openConsoleOnErrorToggle.gameObject.SetActive(enabled);
        }

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyOpenConsoleOnError(bool enabled)
    {
        if (settingsAdvanced.CheckIfMainMenu()) return;

        UIAccess uiAccess = UIAccess.Instance;

        // TODO: Implement logic to open console on error

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }
}
