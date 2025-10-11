using UnityEngine;

public class SettingsAdvancedApplier : MonoBehaviour
{
    public bool inBatchMode = false;

    [Header("References")]
    [SerializeField] private SettingsAdvanced settingsAdvanced;

    private void Awake()
    {
        if (settingsAdvanced == null)
        {
            settingsAdvanced = GetComponent<SettingsAdvanced>();
        }
    }

    private void Reset()
    {
        settingsAdvanced = GetComponent<SettingsAdvanced>();
    }

    public void ApplyShowHUD(bool enabled)
    {
        UIAccess uiAccess = UIAccess.Instance;

        if (uiAccess && uiAccess.canvasIndicators != null)
        {
            uiAccess.canvasIndicators.SetActive(enabled);
        }

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyShowFPS(bool enabled)
    {
        UIAccess uiAccess = UIAccess.Instance;

        if (uiAccess.fpsCounter != null)
        {
            uiAccess.fpsCounter.gameObject.SetActive(enabled);
            uiAccess.fpsCounter.enabled = enabled;
        }

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyShowCrosshair(bool enabled)
    {
        UIAccess uiAccess = UIAccess.Instance;

        if (uiAccess && uiAccess.crosshair != null)
        {
            uiAccess.crosshair.gameObject.SetActive(enabled);
        }

        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyShowAchievementPopups(bool enabled)
    {
        UIAccess uiAccess = UIAccess.Instance;



        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }

    public void ApplyEnableConsole(bool enabled)
    {
        UIAccess uiAccess = UIAccess.Instance;

        if (uiAccess && uiAccess.uiDebugPopup != null)
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
        UIAccess uiAccess = UIAccess.Instance;



        if (inBatchMode == false) { settingsAdvanced.SaveSettings(); }
    }
}
