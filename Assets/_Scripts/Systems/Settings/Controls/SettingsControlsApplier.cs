using UnityEngine;

public class SettingsControlsApplier : MonoBehaviour
{
    public bool inBatchMode = false;

    [Header("References")]
    [SerializeField] private SettingsControls settingsControls;

    private void Awake()
    {
        if (settingsControls == null)
        {
            settingsControls = GetComponent<SettingsControls>();
        }
    }

    private void Reset()
    {
        settingsControls = GetComponent<SettingsControls>();
    }

    public void ApplyInvertYAxis(bool enabled)
    {
        settingsControls.player.playerController.doLookInvert = enabled;

        if (inBatchMode == false) { settingsControls.SaveSettings(); }
    }

    public void ApplyMouseSensitivity(float value)
    {
        settingsControls.player.playerController.lookSpeed = Mathf.Clamp(value, 0.1f, 5f);

        if (inBatchMode == false) { settingsControls.SaveSettingsWithDelay(); }
    }

    public void ApplyCameraSmoothing(bool enabled)
    {
        settingsControls.player.playerController.doSmoothLook = enabled;

        if (inBatchMode == false) { settingsControls.SaveSettings(); }
    }

    public void ApplyControllerSensitivity(float value)
    {
        //settingsControls.player.playerController.lookSpeed = Mathf.Clamp(value, 0.1f, 5f);

        if (inBatchMode == false) { settingsControls.SaveSettingsWithDelay(); }
    }

    public void ApplyControllerSmoothing(bool enabled)
    {
        //settingsControls.player.playerController.doSmoothLook = enabled;

        if (inBatchMode == false) { settingsControls.SaveSettings(); }
    }

    public void ApplyControllerRumble(bool enabled)
    {
        //settingsControls.player.playerController.rumble = enabled;

        if (inBatchMode == false) { settingsControls.SaveSettings(); }
    }
}
