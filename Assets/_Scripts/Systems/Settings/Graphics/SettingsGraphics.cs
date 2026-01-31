using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsGraphics : BaseSettings
{
    public override string CATEGORY => "Graphics";

    [HideInInspector]
    public UniversalRenderPipelineAsset urpAsset;
    public Resolution[] availableResolutions;

    [Space]
    public SettingsGraphicsApplier applier;
    public Volume postProcessVolume;

    [Header("UI Elements")]
    public TMP_Dropdown graphicsAPIDropdown;
    public TMP_Dropdown windowResolutionDropdown;
    public TMP_Dropdown windowModeDropdown;
    public Slider renderScaleSlider;
    public Toggle vSyncToggle;
    public TMP_Dropdown framerateDropdown;
    public Slider fieldOfViewSlider;
    public TMP_Dropdown textureQualityDropdown;
    public TMP_Dropdown antiAliasingDropdown;
    public Toggle renderShadowsToggle;
    public Toggle bloomToggle;
    public Toggle vignetteToggle;

    protected override void InitializeReferences()
    {
        if (applier == null)
        {
            applier = GetComponent<SettingsGraphicsApplier>();
        }

        if (windowResolutionDropdown != null)
        {
            PopulateResolutionDropdown();
        }

        GetURPAsset();
        SetGraphicsAPIOption();

        QualitySettings.vSyncCount = 0;
    }

    public UniversalRenderPipelineAsset GetURPAsset()
    {
        urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        return urpAsset;
    }

    public override void SaveSettings()
    {
        SettingsManager settingsManager = Core.SettingsManager;
        if (settingsManager == null) return;

        settingsManager.SaveInt(CATEGORY, "WindowMode", windowModeDropdown.value);
        settingsManager.SaveInt(CATEGORY, "WindowResolution", windowResolutionDropdown.value);
        settingsManager.SaveFloat(CATEGORY, "RenderScale", renderScaleSlider.value);
        settingsManager.SaveBool(CATEGORY, "VSync", vSyncToggle.isOn);
        settingsManager.SaveInt(CATEGORY, "Framerate", framerateDropdown.value);
        settingsManager.SaveFloat(CATEGORY, "FieldOfView", fieldOfViewSlider.value);
        settingsManager.SaveInt(CATEGORY, "TextureQuality", textureQualityDropdown.value);
        settingsManager.SaveInt(CATEGORY, "AntiAliasing", antiAliasingDropdown.value);
        settingsManager.SaveBool(CATEGORY, "RenderShadows", renderShadowsToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "Bloom", bloomToggle.isOn);
        settingsManager.SaveBool(CATEGORY, "Vignette", vignetteToggle.isOn);

        settingsManager.Save();
    }

    public override void LoadSettings()
    {
        applier.inBatchMode = true;
        LoadSettingsAsync().Forget();
    }

    public override async UniTask LoadSettingsAsync()
    {
        SettingsManager settingsManager = Core.SettingsManager;
        if (settingsManager == null)
        {
            applier.inBatchMode = false;
            return;
        }

        windowModeDropdown.value = settingsManager.LoadInt(CATEGORY, "WindowMode", 0);
        windowResolutionDropdown.value = settingsManager.LoadInt(CATEGORY, "WindowResolution", windowResolutionDropdown.value);
        renderScaleSlider.value = settingsManager.LoadFloat(CATEGORY, "RenderScale", 1f);
        vSyncToggle.isOn = settingsManager.LoadBool(CATEGORY, "VSync", false);
        framerateDropdown.value = settingsManager.LoadInt(CATEGORY, "Framerate", 0);
        fieldOfViewSlider.value = settingsManager.LoadFloat(CATEGORY, "FieldOfView", 70f);
        textureQualityDropdown.value = settingsManager.LoadInt(CATEGORY, "TextureQuality", 3);
        antiAliasingDropdown.value = settingsManager.LoadInt(CATEGORY, "AntiAliasing", 3);
        renderShadowsToggle.isOn = settingsManager.LoadBool(CATEGORY, "RenderShadows", true);
        bloomToggle.isOn = settingsManager.LoadBool(CATEGORY, "Bloom", true);
        vignetteToggle.isOn = settingsManager.LoadBool(CATEGORY, "Vignette", true);

        applier.ApplyWindowMode(windowModeDropdown.value);
        applier.ApplyWindowResolution(windowResolutionDropdown.value);
        applier.ApplyRenderScale(renderScaleSlider.value);
        applier.ApplyVSync(vSyncToggle.isOn);
        applier.ApplyFramerateLimit(framerateDropdown.value);
        await applier.ApplyFieldOfViewAsync(fieldOfViewSlider.value);
        applier.ApplyTextureQuality(textureQualityDropdown.value);
        await applier.ApplyAntiAliasingAsync(antiAliasingDropdown.value);
        await applier.ApplyRenderShadowsAsync(renderShadowsToggle.isOn);
        applier.ApplyBloom(bloomToggle.isOn);
        applier.ApplyVignette(vignetteToggle.isOn);

        applier.inBatchMode = false;
    }

    public void SetGraphicsAPIOption()
    {
        if (graphicsAPIDropdown == null) return;

        string currentAPI = SystemInfo.graphicsDeviceType.ToString();

        switch (currentAPI)
        {
            case "Direct3D11":
                graphicsAPIDropdown.value = 0;
                break;
            case "Direct3D12":
                graphicsAPIDropdown.value = 1;
                break;
            case "Vulkan":
                graphicsAPIDropdown.value = 2;
                break;
            case "Metal":
                graphicsAPIDropdown.value = 3;
                break;
            case "OpenGLCore":
                graphicsAPIDropdown.value = 4;
                break;
            default:
                break;
        }
    }

    public void SetResolutionToNative()
    {
        if (availableResolutions == null || availableResolutions.Length == 0) return;

        int nativeResolutionIndex = 0;
        for (int i = 0; i < availableResolutions.Length; i++)
        {
            if (availableResolutions[i].width == Screen.currentResolution.width &&
                availableResolutions[i].height == Screen.currentResolution.height)
            {
                nativeResolutionIndex = i;
                break;
            }
        }

        // Update the dropdown UI value
        if (windowResolutionDropdown != null)
        {
            windowResolutionDropdown.value = nativeResolutionIndex;
            windowResolutionDropdown.RefreshShownValue();
        }

        // Apply the resolution
        applier.ApplyWindowResolution(nativeResolutionIndex);
    }

    public void PopulateResolutionDropdown()
    {
        Resolution[] allResolutions = Screen.resolutions;
        List<string> options = new List<string>();
        List<Resolution> uniqueResolutions = new List<Resolution>();

        windowResolutionDropdown.ClearOptions();

        HashSet<string> seenResolutions = new HashSet<string>();

        for (int i = 0; i < allResolutions.Length; i++)
        {
            Resolution res = allResolutions[i];
            string resKey = $"{res.width}x{res.height}";

            if (!seenResolutions.Contains(resKey))
            {
                seenResolutions.Add(resKey);
                uniqueResolutions.Add(res);
                options.Add(resKey);
            }
            else
            {
                int existingIndex = uniqueResolutions.FindIndex(r => r.width == res.width && r.height == res.height);
                if (existingIndex >= 0)
                {
                    int existingRefreshRate = Mathf.RoundToInt((float)uniqueResolutions[existingIndex].refreshRateRatio.numerator / uniqueResolutions[existingIndex].refreshRateRatio.denominator);
                    int currentRefreshRate = Mathf.RoundToInt((float)res.refreshRateRatio.numerator / res.refreshRateRatio.denominator);

                    if (currentRefreshRate > existingRefreshRate)
                    {
                        uniqueResolutions[existingIndex] = res;
                    }
                }
            }
        }

        availableResolutions = uniqueResolutions.ToArray();

        int currentResolutionIndex = 0;
        for (int i = 0; i < uniqueResolutions.Count; i++)
        {
            if (uniqueResolutions[i].width == Screen.currentResolution.width &&
                uniqueResolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
                break;
            }
        }

        windowResolutionDropdown.AddOptions(options);
        windowResolutionDropdown.value = currentResolutionIndex;
        windowResolutionDropdown.RefreshShownValue();
    }
}