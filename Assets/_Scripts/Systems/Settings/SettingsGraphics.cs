using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsGraphics : MonoBehaviour
{
    public const string Category = "Graphics";

    [HideInInspector]
    public UniversalRenderPipelineAsset urpAsset;
    public Resolution[] availableResolutions;

    [Header("References")]
    public SettingsGraphicsApplier applier;
    public SettingsGraphicsBinder binder;

    public TMP_Dropdown windowResolutionDropdown;
    public TMP_Dropdown windowModeDropdown;
    public Slider renderScaleSlider;
    public Toggle vSyncToggle;
    public TMP_Dropdown framerateDropdown;

    public TMP_Dropdown textureQualityDropdown;
    public TMP_Dropdown shadowQualityDropdown;
    public Slider shadowDistanceSlider;
    public TMP_Dropdown antiAliasingDropdown;
    
    public UniversalRenderPipelineAsset GetURPAsset()
    {
        urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        return urpAsset;
    }

    private void Start()
    {
        if (applier == null)
        {
            applier = GetComponent<SettingsGraphicsApplier>();
        }

        if (binder == null)
        {
            binder = GetComponent<SettingsGraphicsBinder>();
        }

        GetURPAsset();

        LoadSettings();
        binder.BindAllSettings();
    }

    public void SaveSettings()
    {
        var sm = SettingsManager.Instance;

        sm.SaveInt(Category, "WindowResolution", windowResolutionDropdown.value);
        sm.SaveInt(Category, "WindowMode", windowModeDropdown.value);
        sm.SaveFloat(Category, "RenderScale", renderScaleSlider.value);
        sm.SaveBool(Category, "VSync", vSyncToggle.isOn);
        sm.SaveInt(Category, "Framerate", framerateDropdown.value);

        sm.SaveInt(Category, "TextureQuality", textureQualityDropdown.value);
        sm.SaveInt(Category, "ShadowQuality", shadowQualityDropdown.value);
        sm.SaveFloat(Category, "ShadowDistance", shadowDistanceSlider.value);
        sm.SaveInt(Category, "AntiAliasing", antiAliasingDropdown.value);

        sm.Save();
    }

    public void LoadSettings()
    {
        var sm = SettingsManager.Instance;

        windowResolutionDropdown.value = sm.LoadInt(Category, "WindowResolution", windowResolutionDropdown.value);
        windowModeDropdown.value = sm.LoadInt(Category, "WindowMode", 0);
        renderScaleSlider.value = sm.LoadFloat(Category, "RenderScale", 1f);
        vSyncToggle.isOn = sm.LoadBool(Category, "VSync", false);
        framerateDropdown.value = sm.LoadInt(Category, "Framerate", 0);

        textureQualityDropdown.value = sm.LoadInt(Category, "TextureQuality", 2);
        shadowQualityDropdown.value = sm.LoadInt(Category, "ShadowQuality", 3);
        shadowDistanceSlider.value = sm.LoadFloat(Category, "ShadowDistance", 65f);
        antiAliasingDropdown.value = sm.LoadInt(Category, "AntiAliasing", 2);
    }

    public void PopulateResolutionDropdown()
    {
        Resolution[] allResolutions = Screen.resolutions;
        List<string> options = new List<string>();
        availableResolutions = allResolutions;

        // Clear any existing options (there shouldn't be anyway tho)
        windowResolutionDropdown.ClearOptions();

        // Find the current resolution to set the default value
        int currentResolutionIndex = 0;
        int maxRefreshRate = 0;

        for (int i = 0; i < allResolutions.Length; i++)
        {
            Resolution res = allResolutions[i];

            int refreshRateHz = Mathf.RoundToInt((float)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator);

            //string option = $"{res.width}x{res.height} @ {refreshRateHz}Hz";
            string option = $"{res.width}x{res.height}";
            options.Add(option);

            if (res.width == Screen.currentResolution.width &&
                res.height == Screen.currentResolution.height &&
                refreshRateHz > maxRefreshRate)
            {
                maxRefreshRate = refreshRateHz;
                currentResolutionIndex = i;
            }
        }

        windowResolutionDropdown.AddOptions(options);

        windowResolutionDropdown.value = currentResolutionIndex;
        windowResolutionDropdown.RefreshShownValue();

        applier.ApplyWindowResolution(currentResolutionIndex);
    }
}