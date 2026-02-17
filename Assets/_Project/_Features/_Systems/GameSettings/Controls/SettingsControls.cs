using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsControls : BaseSettings
{
    public override string CATEGORY => "Controls";

    [Space]
    public SettingsControlsApplier applier;

    [Header("UI Elements")]
    public Toggle invertYAxisToggle;
    public Slider mouseSensitivitySlider;
    public Toggle cameraSmoothingToggle;
    public Slider controllerSensitivitySlider;
    public Toggle controllerSmoothingToggle;
    public Toggle controllerRumbleToggle;
    public TMP_Dropdown headbobbingStyleDropdown;

    protected override void InitializeReferences()
    {
        if (applier == null) applier = GetComponent<SettingsControlsApplier>();
    }

    public override void SaveSettings()
    {
        SettingsManager settingsManager = Core.SettingsManager;
        if (settingsManager == null) return;

        settingsManager.SaveBool(CATEGORY, "InvertYAxis", invertYAxisToggle.isOn);
        settingsManager.SaveFloat(CATEGORY, "MouseSensitivity", mouseSensitivitySlider.value);
        settingsManager.SaveBool(CATEGORY, "CameraSmoothing", cameraSmoothingToggle.isOn);
        settingsManager.SaveFloat(CATEGORY, "ControllerSensitivity", controllerSensitivitySlider.value);
        settingsManager.SaveBool(CATEGORY, "ControllerSmoothing", controllerSmoothingToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "ControllerRumble", controllerRumbleToggle.isOn);

        settingsManager.Save();
    }

    public override void LoadSettings()
    {
        SettingsManager settingsManager = Core.SettingsManager;
        if (settingsManager == null) return;

        applier.inBatchMode = true;

        invertYAxisToggle.SetIsOnWithoutNotify(settingsManager.LoadBool(CATEGORY, "InvertYAxis", false));
        mouseSensitivitySlider.SetValueWithoutNotify(settingsManager.LoadFloat(CATEGORY, "MouseSensitivity", 2.5f));
        cameraSmoothingToggle.SetIsOnWithoutNotify(settingsManager.LoadBool(CATEGORY, "CameraSmoothing", true));
        controllerSensitivitySlider.SetValueWithoutNotify(settingsManager.LoadFloat(CATEGORY, "ControllerSensitivity", 2f));
        controllerSmoothingToggle.SetIsOnWithoutNotify(settingsManager.LoadBool(CATEGORY, "ControllerSmoothing", true));
        controllerRumbleToggle.SetIsOnWithoutNotify(settingsManager.LoadBool(CATEGORY, "ControllerRumble", true));

        applier.ApplyInvertYAxis(invertYAxisToggle.isOn);
        applier.ApplyMouseSensitivity(mouseSensitivitySlider.value);
        applier.ApplyCameraSmoothing(cameraSmoothingToggle.isOn);
        applier.ApplyControllerSensitivity(controllerSensitivitySlider.value);
        applier.ApplyControllerSmoothing(controllerSmoothingToggle.isOn);
        applier.ApplyControllerRumble(controllerRumbleToggle.isOn);

        applier.inBatchMode = false;
    }
}