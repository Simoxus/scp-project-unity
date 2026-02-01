using UnityEngine;

public class SettingsControlsApplier : BaseSettingsApplier
{
    [SerializeField] private SettingsControls settingsControls;

    protected override void InitializeReferences()
    {
        if (settingsControls == null)
        {
            settingsControls = GetComponent<SettingsControls>();
        }
    }

    public void ApplyInvertYAxis(bool enabled)
    {
        if (settingsControls.CheckIfMainMenu()) return;

        Core.Player.Controller.InvertYAxis = enabled;

        if (!inBatchMode) settingsControls.SaveSettings();
    }

    public void ApplyMouseSensitivity(float value)
    {
        if (settingsControls.CheckIfMainMenu()) return;

        Core.Player.Controller.LookSpeed = Mathf.Clamp(value, 0.1f, 5f);

        if (!inBatchMode) settingsControls.SaveSettingsWithDelay();
    }

    public void ApplyCameraSmoothing(bool enabled)
    {
        if (settingsControls.CheckIfMainMenu()) return;

        Core.Player.Controller.SmoothLook = enabled;

        if (!inBatchMode) settingsControls.SaveSettings();
    }

    public void ApplyControllerSensitivity(float value)
    {
        if (settingsControls.CheckIfMainMenu()) return;

        // TODO: Implement controller sensitivity

        if (!inBatchMode) settingsControls.SaveSettingsWithDelay();
    }

    public void ApplyControllerSmoothing(bool enabled)
    {
        if (settingsControls.CheckIfMainMenu()) return;

        // TODO: Implement controller smoothing

        if (!inBatchMode) settingsControls.SaveSettings();
    }

    public void ApplyControllerRumble(bool enabled)
    {
        if (settingsControls.CheckIfMainMenu()) return;

        VibrationHelper.IsVibrationEnabled = enabled;

        if (!enabled)
        {
            VibrationHelper.Stop();
        }

        if (!inBatchMode) settingsControls.SaveSettings();
    }
}
