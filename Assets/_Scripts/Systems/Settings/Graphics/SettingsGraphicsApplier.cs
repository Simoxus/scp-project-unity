using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SettingsGraphicsApplier : MonoBehaviour
{
    public bool inBatchMode = false;

    [Header("References")]
    [SerializeField] private SettingsGraphics settingsGraphics;

    private void Awake()
    {
        if (settingsGraphics == null)
        {
            settingsGraphics = GetComponent<SettingsGraphics>();
        }
    }

    private void Reset()
    {
        settingsGraphics = GetComponent<SettingsGraphics>();
    }

    public void ApplyWindowResolution(int index)
    {
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
        settingsGraphics.urpAsset.renderScale = Mathf.Clamp(value, 0.1f, 1.0f);

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
        Application.targetFrameRate = framerates[index];

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyTextureQuality(int index)
    {
        switch (index)
        {
            case 0: QualitySettings.globalTextureMipmapLimit = 2; break; // Quarter
            case 1: QualitySettings.globalTextureMipmapLimit = 1; break; // Half
            case 2: QualitySettings.globalTextureMipmapLimit = 0; break; // Full
        }

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyShadowQuality(int index)
    {
        UniversalRenderPipelineAsset urpAsset = settingsGraphics.urpAsset;

        // Off
        if (index == 0)
        {
            QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
            return;
        }

        // Low
        if (index == 1)
        {
            QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
            if (urpAsset != null)
            {
                urpAsset.mainLightShadowmapResolution = (int)UnityEngine.Rendering.Universal.ShadowResolution._512;
            }
        }
        // Medium
        else if (index == 2)
        {
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Low;
            if (urpAsset != null)
            {
                urpAsset.mainLightShadowmapResolution = (int)UnityEngine.Rendering.Universal.ShadowResolution._1024;
            }
        }
        // High
        else if (index == 3)
        {
            QualitySettings.shadows = UnityEngine.ShadowQuality.All;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.High;
            if (urpAsset != null)
            {
                urpAsset.mainLightShadowmapResolution = (int)UnityEngine.Rendering.Universal.ShadowResolution._2048;
            }
        }

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }

    public void ApplyAntiAliasing(int index)
    {
        UniversalRenderPipelineAsset urpAsset = settingsGraphics.urpAsset;
        Camera cameraBrain = Player.Instance.cameraBrain;

        if (cameraBrain != null)
        {
            var cameraData = cameraBrain.GetUniversalAdditionalCameraData();

            if (urpAsset != null)
            {
                urpAsset.msaaSampleCount = (index == 0) ? 1 : 1;
            }

            if (cameraData != null)
            {
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
                }
            }
        }

        if (inBatchMode == false) { settingsGraphics.SaveSettings(); }
    }
}
