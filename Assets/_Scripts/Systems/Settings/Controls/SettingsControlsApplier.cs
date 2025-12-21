using UnityEngine;

public class SettingsControlsApplier : BaseSettingsApplier
{
    [Header("References")]
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
        if (settingsControls.CheckIfMainMenu() || settingsControls.player == null) return;

        settingsControls.player.playerController.doLookInvert = enabled;

        if (!inBatchMode) settingsControls.SaveSettings();
    }

    public void ApplyMouseSensitivity(float value)
    {
        if (settingsControls.CheckIfMainMenu() || settingsControls.player == null) return;

        settingsControls.player.playerController.lookSpeed = Mathf.Clamp(value, 0.1f, 5f);

        if (!inBatchMode) settingsControls.SaveSettingsWithDelay();
    }

    public void ApplyCameraSmoothing(bool enabled)
    {
        if (settingsControls.CheckIfMainMenu() || settingsControls.player == null) return;

        settingsControls.player.playerController.doSmoothLook = enabled;

        if (!inBatchMode) settingsControls.SaveSettings();
    }

    public void ApplyControllerSensitivity(float value)
    {
        if (settingsControls.CheckIfMainMenu() || settingsControls.player == null) return;

        // TODO: Implement controller sensitivity

        if (!inBatchMode) settingsControls.SaveSettingsWithDelay();
    }

    public void ApplyControllerSmoothing(bool enabled)
    {
        if (settingsControls.CheckIfMainMenu() || settingsControls.player == null) return;

        // TODO: Implement controller smoothing

        if (!inBatchMode) settingsControls.SaveSettings();
    }

    public void ApplyControllerRumble(bool enabled)
    {
        if (settingsControls.CheckIfMainMenu() || settingsControls.player == null) return;

        // TODO: Implement controller rumble

        if (!inBatchMode) settingsControls.SaveSettings();
    }
}
