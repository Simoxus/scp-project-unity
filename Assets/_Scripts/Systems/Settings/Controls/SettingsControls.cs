using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsControls : MonoBehaviour
{
    public const string CATEGORY = "Controls";

    [Header("References")]
    public SettingsControlsApplier applier;
    public Player player;

    public Toggle invertYAxisToggle;

    public Slider mouseSensitivitySlider;
    public Toggle cameraSmoothingToggle;

    public Slider controllerSensitivitySlider;
    public Toggle controllerSmoothingToggle;
    public Toggle controllerRumbleToggle;

    public TMP_Dropdown headbobbingStyleDropdown;

    private bool _isWaitingToSave = false;

    private void Awake()
    {
        player = player != null ? player : Player.Instance;
    }

    private void Start()
    {
        if (applier == null)
        {
            applier = GetComponent<SettingsControlsApplier>();
        }

        LoadSettings();
    }

    public void SaveSettings()
    {
        var sm = SettingsManager.Instance;

        sm.SaveBool(CATEGORY, "InvertYAxis", invertYAxisToggle.isOn);

        sm.SaveFloat(CATEGORY, "MouseSensitivity", mouseSensitivitySlider.value);
        sm.SaveBool(CATEGORY, "CameraSmoothing", cameraSmoothingToggle.isOn);

        sm.SaveFloat(CATEGORY, "ControllerSensitivity", controllerSensitivitySlider.value);
        sm.SaveBool(CATEGORY, "ControllerSmoothing", controllerSmoothingToggle.isOn);
        sm.SaveBool(CATEGORY, "ControllerRumble", controllerRumbleToggle.isOn);

        sm.Save();
    }

    public void LoadSettings()
    {
        var sm = SettingsManager.Instance;

        invertYAxisToggle.isOn = sm.LoadBool(CATEGORY, "InvertYAxis", false);

        mouseSensitivitySlider.value = sm.LoadFloat(CATEGORY, "MouseSensitivity", 2.5f);
        cameraSmoothingToggle.isOn = sm.LoadBool(CATEGORY, "CameraSmoothing", true);

        controllerSensitivitySlider.value = sm.LoadFloat(CATEGORY, "ControllerSensitivity", 2f);
        controllerSmoothingToggle.isOn = sm.LoadBool(CATEGORY, "ControllerSmoothing", true);
        controllerRumbleToggle.isOn = sm.LoadBool(CATEGORY, "ControllerRumble", true);

        applier.inBatchMode = true;

        applier.ApplyInvertYAxis(invertYAxisToggle.isOn);
        applier.ApplyMouseSensitivity(mouseSensitivitySlider.value);
        applier.ApplyCameraSmoothing(cameraSmoothingToggle.isOn);
        applier.ApplyControllerSensitivity(controllerSensitivitySlider.value);
        applier.ApplyControllerSmoothing(controllerSmoothingToggle.isOn);
        applier.ApplyControllerRumble(controllerRumbleToggle.isOn);

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