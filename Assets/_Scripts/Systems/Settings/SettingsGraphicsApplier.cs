using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsGraphicsApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsGraphics graphics;

    private void Awake()
    {
        if (graphics == null)
        {
            graphics = GetComponent<SettingsGraphics>();
        }
    }

    private void Reset()
    {
        graphics = GetComponent<SettingsGraphics>();
    }

    public void ApplyWindowResolution(int index)
    {
        Resolution chosenResolution = graphics.availableResolutions[index];
        Screen.SetResolution(chosenResolution.width, chosenResolution.height, Screen.fullScreenMode);

        graphics.SaveSettings();
    }

    public void ApplyWindowMode(int index)
    {
        switch (index)
        {
            case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
            case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
            case 2: Screen.fullScreenMode = FullScreenMode.Windowed; break;
        }

        graphics.SaveSettings();
    }

    public void ApplyRenderScale(float value)
    {
        graphics.urpAsset.renderScale = Mathf.Clamp(value, 0.1f, 1.0f);

        graphics.SaveSettings();
    }

    public void ApplyVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        graphics.SaveSettings();
    }

    public void ApplyFramerateLimit(int index)
    {
        int[] framerates = { -1, 30, 60, 90, 120, 150, 210, 240 };
        Application.targetFrameRate = framerates[index];

        graphics.SaveSettings();
    }

    public void ApplyTextureQuality(int index)
    {
        switch (index)
        {
            case 0: QualitySettings.globalTextureMipmapLimit = 2; break; // Quarter
            case 1: QualitySettings.globalTextureMipmapLimit = 1; break; // Half
            case 2: QualitySettings.globalTextureMipmapLimit = 0; break; // Full
        }

        graphics.SaveSettings();
    }

    public void ApplyShadowQuality(int index)
    {
        UniversalRenderPipelineAsset urpAsset = graphics.urpAsset;

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

        graphics.SaveSettings();
    }

    public void ApplyShadowDistance(float value)
    {
        QualitySettings.shadowDistance = Mathf.Clamp(value, 0, 65);

        graphics.SaveSettings();
    }

    public void ApplyAntiAliasing(int index)
    {
        graphics.SaveSettings();
    }
}
