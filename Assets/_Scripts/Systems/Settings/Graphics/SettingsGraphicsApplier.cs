using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SettingsGraphicsApplier : BaseSettingsApplier
{
    [Header("References")]
    [SerializeField] private SettingsGraphics settingsGraphics;

    protected override void InitializeReferences()
    {
        if (settingsGraphics == null)
        {
            settingsGraphics = GetComponent<SettingsGraphics>();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ReapplyCameraSettings();
    }

    private void ReapplyCameraSettings()
    {
        if (settingsGraphics == null || SettingsManager.Instance == null) return;

        // Get saved settings and reapply them
        int antiAliasingValue = SettingsManager.Instance.LoadInt(settingsGraphics.CATEGORY, "AntiAliasing", 2);
        float renderScaleValue = SettingsManager.Instance.LoadFloat(settingsGraphics.CATEGORY, "RenderScale", 1f);

        inBatchMode = true;

        ApplyAntiAliasing(antiAliasingValue);
        ApplyRenderScale(renderScaleValue);

        inBatchMode = false;
    }

    public void ApplyWindowResolution(int index)
    {
        if (index < 0 || index >= settingsGraphics.availableResolutions.Length) return;

        Resolution chosenResolution = settingsGraphics.availableResolutions[index];
        Screen.SetResolution(chosenResolution.width, chosenResolution.height, Screen.fullScreenMode);

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyWindowMode(int index)
    {
        switch (index)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;

                if (settingsGraphics.windowResolutionDropdown != null)
                {
                    settingsGraphics.windowResolutionDropdown.gameObject.SetActive(false);
                }

                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

                if (settingsGraphics.windowResolutionDropdown != null)
                {
                    settingsGraphics.windowResolutionDropdown.gameObject.SetActive(false);
                }

                break;

            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;

                if (settingsGraphics.windowResolutionDropdown != null)
                {
                    settingsGraphics.windowResolutionDropdown.gameObject.SetActive(true);
                }

                break;
        }

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyRenderScale(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0.1f, 1.0f);

        if (settingsGraphics.urpAsset != null)
        {
            settingsGraphics.urpAsset.renderScale = clampedValue;
        }

        if (inBatchMode == false) { settingsGraphics.SaveSettingsWithDelay(); }
    }

    public void ApplyVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyFramerateLimit(int index)
    {
        int[] framerates = { -1, 30, 60, 90, 120, 150, 180, 210, 240 };

        if (index >= 0 && index < framerates.Length)
        {
            Application.targetFrameRate = framerates[index];
        }

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyTextureQuality(int index)
    {
        switch (index)
        {
            case 0: // Quarter
                QualitySettings.globalTextureMipmapLimit = 2;
                break;
            case 1: // Half
                QualitySettings.globalTextureMipmapLimit = 1;
                break;
            case 2: // Full
                QualitySettings.globalTextureMipmapLimit = 0;
                break;
        }

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyAntiAliasing(int index)
    {
        if (Player.Instance == null) return;
        if (Player.Instance.cameraBrain == null) return;

        Camera cameraBrain = Player.Instance.cameraBrain;
        UniversalAdditionalCameraData cameraData = cameraBrain.GetUniversalAdditionalCameraData();
        UniversalRenderPipelineAsset urpAsset = settingsGraphics.urpAsset;

        if (urpAsset != null)
        {
            urpAsset.msaaSampleCount = (index == 4) ? 4 : 1;
        }

        switch (index)
        {
            case 0: // None
                cameraData.antialiasing = AntialiasingMode.None;
                break;

            case 1: // FXAA
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
                break;

            case 2: // SMAA
                cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
                break;

            case 3: // TAA
                cameraData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
                break;

            case 4: // MSAA
                cameraData.antialiasing = AntialiasingMode.None;
                break;
        }

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyRenderShadows(bool enabled)
    {
        if (Player.Instance == null) return;
        if (Player.Instance.cameraBrain == null) return;

        Camera cameraBrain = Player.Instance.cameraBrain;
        UniversalAdditionalCameraData cameraData = cameraBrain.GetUniversalAdditionalCameraData();

        cameraData.renderShadows = enabled;

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }
}
