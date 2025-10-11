using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SettingsGraphics : MonoBehaviour
{
    public const string CATEGORY = "Graphics";

    [HideInInspector]
    public UniversalRenderPipelineAsset urpAsset;
    public Resolution[] availableResolutions;

    [Header("References")]
    public SettingsGraphicsApplier applier;

    public TMP_Dropdown graphicsAPIDropdown;
    public TMP_Dropdown windowResolutionDropdown;
    public TMP_Dropdown windowModeDropdown;
    public Slider renderScaleSlider;
    public Toggle vSyncToggle;
    public TMP_Dropdown framerateDropdown;

    public TMP_Dropdown textureQualityDropdown;
    public TMP_Dropdown shadowQualityDropdown;
    public TMP_Dropdown antiAliasingDropdown;

    private bool _isWaitingToSave = false;

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

        GetURPAsset();
        SetGraphicsAPIOption();

        if (windowResolutionDropdown != null)
        {
            PopulateResolutionDropdown();
        }

        QualitySettings.vSyncCount = 0;

        LoadSettings();
    }

    public void SaveSettings()
    {
        var sm = SettingsManager.Instance;

        sm.SaveInt(CATEGORY, "WindowMode", windowModeDropdown.value);
        sm.SaveInt(CATEGORY, "WindowResolution", windowResolutionDropdown.value);
        sm.SaveFloat(CATEGORY, "RenderScale", renderScaleSlider.value);
        sm.SaveBool(CATEGORY, "VSync", vSyncToggle.isOn);
        sm.SaveInt(CATEGORY, "Framerate", framerateDropdown.value);

        sm.SaveInt(CATEGORY, "TextureQuality", textureQualityDropdown.value);
        sm.SaveInt(CATEGORY, "ShadowQuality", shadowQualityDropdown.value);
        sm.SaveInt(CATEGORY, "AntiAliasing", antiAliasingDropdown.value);

        sm.Save();
    }

    public void LoadSettings()
    {
        var sm = SettingsManager.Instance;

        windowModeDropdown.value = sm.LoadInt(CATEGORY, "WindowMode", 0);
        windowResolutionDropdown.value = sm.LoadInt(CATEGORY, "WindowResolution", windowResolutionDropdown.value);
        renderScaleSlider.value = sm.LoadFloat(CATEGORY, "RenderScale", 1f);
        vSyncToggle.isOn = sm.LoadBool(CATEGORY, "VSync", false);
        framerateDropdown.value = sm.LoadInt(CATEGORY, "Framerate", 0);

        textureQualityDropdown.value = sm.LoadInt(CATEGORY, "TextureQuality", 2);
        shadowQualityDropdown.value = sm.LoadInt(CATEGORY, "ShadowQuality", 3);
        antiAliasingDropdown.value = sm.LoadInt(CATEGORY, "AntiAliasing", 2);

        applier.inBatchMode = true;

        applier.ApplyWindowMode(windowModeDropdown.value);
        applier.ApplyWindowResolution(windowResolutionDropdown.value);
        applier.ApplyRenderScale(renderScaleSlider.value);
        applier.ApplyVSync(vSyncToggle.isOn);
        applier.ApplyFramerateLimit(framerateDropdown.value);
        applier.ApplyTextureQuality(textureQualityDropdown.value);
        applier.ApplyShadowQuality(shadowQualityDropdown.value);
        applier.ApplyAntiAliasing(antiAliasingDropdown.value);

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

    public void SetGraphicsAPIOption()
    {
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

    public void PopulateResolutionDropdown()
    {
        Resolution[] allResolutions = Screen.resolutions;
        List<string> options = new List<string>();
        List<Resolution> uniqueResolutions = new List<Resolution>();

        // Clear any existing options (there shouldn't be anyway tho)
        windowResolutionDropdown.ClearOptions();

        // Filter to unique resolutions (by width and height only)
        HashSet<string> seenResolutions = new HashSet<string>();

        for (int i = 0; i < allResolutions.Length; i++)
        {
            Resolution res = allResolutions[i];
            string resKey = $"{res.width}x{res.height}";

            // Only add if we haven't seen this resolution before
            if (!seenResolutions.Contains(resKey))
            {
                seenResolutions.Add(resKey);
                uniqueResolutions.Add(res);
                options.Add(resKey);
            }
            else
            {
                // If this resolution has been seen, keep the one with the highest refresh rate
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