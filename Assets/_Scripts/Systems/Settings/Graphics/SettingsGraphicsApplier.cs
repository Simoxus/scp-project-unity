using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SettingsGraphicsApplier : BaseSettingsApplier
{
    [SerializeField] private SettingsGraphics settingsGraphics;

    private CancellationTokenSource _cts;
    private CancellationTokenSource _renderScaleCts;
    private float _pendingRenderScale = -1f;
    private bool _isApplyingRenderScale = false;

    protected override void InitializeReferences()
    {
        if (settingsGraphics == null)
        {
            settingsGraphics = GetComponent<SettingsGraphics>();
        }

        _cts = new CancellationTokenSource();
        _renderScaleCts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _renderScaleCts?.Cancel();
        _renderScaleCts?.Dispose();
    }

    public void ApplyWindowResolution(int index)
    {
        if (index < 0 || index >= settingsGraphics.availableResolutions.Length) return;

        Resolution chosenResolution = settingsGraphics.availableResolutions[index];
        Screen.SetResolution(chosenResolution.width, chosenResolution.height, Screen.fullScreenMode);

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public void ApplyWindowMode(int index)
    {
        switch (index)
        {
            case 0: // exclusive
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                settingsGraphics.SetResolutionToNative();

                if (settingsGraphics.windowResolutionDropdown != null)
                {
                    settingsGraphics.windowResolutionDropdown.gameObject.SetActive(false);
                }

                break;
            case 1: // borderless
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;

                if (settingsGraphics.windowResolutionDropdown != null)
                {
                    settingsGraphics.windowResolutionDropdown.gameObject.SetActive(true);
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

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public void ApplyRenderScale(float value)
    {
        float clampedValue = Mathf.Clamp(value, 0.1f, 1.0f);

        _pendingRenderScale = clampedValue;
        if (_isApplyingRenderScale)
        {
            _renderScaleCts?.Cancel();
            _renderScaleCts?.Dispose();
            _renderScaleCts = new CancellationTokenSource();
        }

        ApplyRenderScaleDelayed(_renderScaleCts.Token).Forget();

        if (inBatchMode == false) settingsGraphics.SaveSettingsWithDelay();
    }

    private async UniTaskVoid ApplyRenderScaleDelayed(CancellationToken cancellationToken)
    {
        _isApplyingRenderScale = true;

        try
        {
            await UniTask.WaitForSeconds(0.1f, ignoreTimeScale: true, cancellationToken: cancellationToken);

            if (settingsGraphics.urpAsset != null && _pendingRenderScale >= 0)
            {
                settingsGraphics.urpAsset.renderScale = _pendingRenderScale;
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isApplyingRenderScale = false;
        }
    }

    public void ApplyVSync(bool enabled)
    {
        QualitySettings.vSyncCount = enabled ? 1 : 0;

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public void ApplyFramerateLimit(int index)
    {
        int[] framerates = { -1, 30, 60, 90, 120, 150, 180, 210, 240 };

        if (index >= 0 && index < framerates.Length)
        {
            Application.targetFrameRate = framerates[index];
        }

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public void ApplyFieldOfView(float value)
    {
        ApplyFieldOfViewAsync(value).Forget();
    }

    public void ApplyTextureQuality(int index)
    {
        switch (index)
        {
            case 0: // Eighth
                QualitySettings.globalTextureMipmapLimit = 3;
                break;
            case 1: // Quarter
                QualitySettings.globalTextureMipmapLimit = 2;
                break;
            case 2: // Half
                QualitySettings.globalTextureMipmapLimit = 1;
                break;
            case 3: // Full
                QualitySettings.globalTextureMipmapLimit = 0;
                break;
        }

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public void ApplyAntiAliasing(int index)
    {
        ApplyAntiAliasingAsync(index).Forget();
    }

    public void ApplyRenderShadows(bool enabled)
    {
        ApplyRenderShadowsAsync(enabled).Forget();
    }

    public void ApplyBloom(bool enabled)
    {
        if (settingsGraphics.postProcessVolume == null || settingsGraphics.postProcessVolume.profile == null) return;

        if (settingsGraphics.postProcessVolume.profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloom))
        {
            bloom.active = enabled;
        }

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public void ApplyVignette(bool enabled)
    {
        if (settingsGraphics.postProcessVolume == null || settingsGraphics.postProcessVolume.profile == null) return;

        if (settingsGraphics.postProcessVolume.profile.TryGet<UnityEngine.Rendering.Universal.Vignette>(out var vignette))
        {
            vignette.active = enabled;
        }

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public async UniTask ApplyFieldOfViewAsync(float value)
    {
        if (Core.Player == null || Core.Player.CameraMain == null)
        {
            await WaitForPlayerAsync(_cts.Token);
        }

        Core.Player.CameraMain.Lens.FieldOfView = value;

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public async UniTask ApplyAntiAliasingAsync(int index)
    {
        if (Core.Player == null || Core.Player.CameraMain == null)
        {
            await WaitForPlayerAsync(_cts.Token);
        }

        Camera cameraBrain = Core.Player.CameraBrain;
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

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    public async UniTask ApplyRenderShadowsAsync(bool enabled)
    {
        if (Core.Player == null || Core.Player.CameraMain == null)
        {
            await WaitForPlayerAsync(_cts.Token);
        }

        Camera cameraBrain = Core.Player.CameraBrain;
        UniversalAdditionalCameraData cameraData = cameraBrain.GetUniversalAdditionalCameraData();

        cameraData.renderShadows = enabled;

        if (inBatchMode == false) settingsGraphics.SaveSettings();
    }

    private async UniTask WaitForPlayerAsync(CancellationToken cancellationToken)
    {
        await UniTask.WaitUntil(
            () => Core.Player != null && Core.Player.CameraBrain != null,
            cancellationToken: cancellationToken
        );
    }
}