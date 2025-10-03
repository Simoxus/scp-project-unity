using UnityEngine;
using UnityEngine.Rendering.Universal;

using ShadowQuality = UnityEngine.ShadowQuality;
using ShadowResolution = UnityEngine.ShadowResolution;

public class SettingsGraphicsBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SettingsGraphics graphics;
    private UniversalRenderPipelineAsset _urpAsset => graphics.urpAsset;

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

    public void BindAllSettings()
    {
        BindWindowMode();
        BindRenderScale();
        BindVSync();
        BindFramerate();
        BindTextureQuality();
        BindShadowQuality();
        BindShadowDistance();
        BindAntiAliasing();
    }

    private void BindWindowMode()
    {
        if (graphics.windowModeDropdown == null) { return; }

        switch (Screen.fullScreenMode)
        {
            case FullScreenMode.ExclusiveFullScreen: graphics.windowModeDropdown.value = 0; break;
            case FullScreenMode.FullScreenWindow: graphics.windowModeDropdown.value = 1; break;
            case FullScreenMode.Windowed: graphics.windowModeDropdown.value = 2; break;
        }

        graphics.windowModeDropdown.RefreshShownValue();
    }

    private void BindRenderScale()
    {
        if (graphics.renderScaleSlider == null) { return; }
        graphics.renderScaleSlider.value = _urpAsset.renderScale;
    }

    private void BindVSync()
    {
        if (graphics.vSyncToggle == null) return;
        graphics.vSyncToggle.isOn = (QualitySettings.vSyncCount > 0);
    }

    private void BindFramerate()
    {
        if (graphics.framerateDropdown == null) return;

        int[] options = { -1, 30, 60, 90, 120, 150, 210, 240 };
        int currentTarget = Application.targetFrameRate;

        int index = System.Array.IndexOf(options, currentTarget);
        if (index < 0) index = 0; // just do a fallback to "Unlocked"

        graphics.framerateDropdown.value = index;
        graphics.framerateDropdown.RefreshShownValue();
    }

    private void BindTextureQuality()
    {
        if (graphics.textureQualityDropdown == null) return;

        switch (QualitySettings.globalTextureMipmapLimit)
        {
            case 2: graphics.textureQualityDropdown.value = 0; break; // Quarter
            case 1: graphics.textureQualityDropdown.value = 1; break; // Half
            case 0: graphics.textureQualityDropdown.value = 2; break; // Full
        }

        graphics.textureQualityDropdown.RefreshShownValue();
    }

    private void BindShadowQuality()
    {
        if (graphics.shadowQualityDropdown == null) return;

        if (QualitySettings.shadows == ShadowQuality.Disable)
            graphics.shadowQualityDropdown.value = 0;
        else if (QualitySettings.shadows == ShadowQuality.HardOnly)
            graphics.shadowQualityDropdown.value = 1;
        else if (QualitySettings.shadowResolution == ShadowResolution.Low)
            graphics.shadowQualityDropdown.value = 2;
        else
            graphics.shadowQualityDropdown.value = 3;

        graphics.shadowQualityDropdown.RefreshShownValue();
    }

    private void BindShadowDistance()
    {
        if (graphics.shadowDistanceSlider == null) return;
        graphics.shadowDistanceSlider.value = QualitySettings.shadowDistance;
    }

    private void BindAntiAliasing()
    {
        
    }
}
