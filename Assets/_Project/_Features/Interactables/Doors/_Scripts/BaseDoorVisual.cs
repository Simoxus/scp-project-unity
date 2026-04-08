using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public abstract class BaseDoorVisual : MonoBehaviour
{
    [Header("Screen References")]
    public GameObject screen;
    public MeshRenderer screenRenderer;
    public MeshRenderer screenLogo;
    public GameObject screenTextObject;
    public TextMeshPro screenText;

    [Header("Material Settings")]
    public int screenMaterialIndex = 1;

    [Header("Pulse Settings")]
    [SerializeField] protected float pulseDuration = 0.6f;
    [SerializeField] protected float pulseMaxBrightness = 1.2f;

    protected CancellationToken _colorChangeCts;
    protected Material _screenMaterial;
    protected Tween _pulseTween;
    protected static readonly int OverlayColorProperty = Shader.PropertyToID("_OverlayColor");

    protected virtual void Awake()
    {
        _colorChangeCts = this.GetCancellationTokenOnDestroy();

        if (screenRenderer != null && screenRenderer.materials.Length > screenMaterialIndex)
        {
            Material[] materials = screenRenderer.materials;
            _screenMaterial = materials[screenMaterialIndex];
        }
    }

    protected virtual void OnDestroy()
    {
        if (_pulseTween.isAlive)
        {
            _pulseTween.Stop();
        }

        if (_screenMaterial != null)
        {
            Destroy(_screenMaterial);
            _screenMaterial = null;
        }
    }

    public virtual void ToggleLogo(bool enabled)
    {
        if (screenLogo != null)
        {
            screenLogo.enabled = enabled;
        }
    }

    public virtual void ToggleText(bool enabled)
    {
        if (screenTextObject != null)
        {
            screenTextObject.SetActive(enabled);
        }
    }

    public virtual void ChangeScreenText(string requestedText)
    {
        if (screenText != null)
        {
            screenText.text = requestedText;
        }
    }

    public virtual async void ChangeScreenColor(Color requestedColor, bool doTweenChange, float tweenChangeDuration = 0.35f)
    {
        if (_screenMaterial == null) return;

        try
        {
            if (doTweenChange)
            {
                Color startColor = _screenMaterial.GetColor(OverlayColorProperty);
                await Tween.Custom(
                    startColor,
                    requestedColor,
                    tweenChangeDuration,
                    onValueChange: color =>
                    {
                        if (_screenMaterial == null) return;
                        _screenMaterial.SetColor(OverlayColorProperty, color);
                    },
                    ease: Ease.Linear
                ).ToYieldInstruction().ToUniTask(cancellationToken: _colorChangeCts);
            }
            else
            {
                _screenMaterial.SetColor(OverlayColorProperty, requestedColor);
            }
        }
        catch (OperationCanceledException ex)
        {
            Log.Exception(ex);
        }
    }

    public virtual void StartPulse(Color pulseColor, float? customDuration = null, float? customIntensity = null)
    {
        StopPulse();
        if (_screenMaterial == null) return;

        float duration = customDuration ?? pulseDuration;
        float intensity = customIntensity ?? pulseMaxBrightness;

        Color baseColor = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 1f);
        Color modifiedColor = new Color(
            pulseColor.r * intensity,
            pulseColor.g * intensity,
            pulseColor.b * intensity,
            1f
        );

        _pulseTween = Tween.Custom(
            baseColor,
            modifiedColor,
            duration,
            onValueChange: color =>
            {
                if (_screenMaterial == null) return;
                _screenMaterial.SetColor(OverlayColorProperty, color);
            },
            ease: Ease.InOutSine,
            cycles: -1,
            cycleMode: CycleMode.Yoyo
        );
    }

    public virtual void StopPulse()
    {
        if (_pulseTween.isAlive)
        {
            _pulseTween.Stop();
        }
    }

    public virtual async void TransitionToPulse(Color targetColor, float transitionDuration, float pulseDuration, float pulseIntensity)
    {
        if (_screenMaterial == null) return;

        try
        {
            // Get the current color
            Color startColor = _screenMaterial.GetColor(OverlayColorProperty);

            StopPulse();

            // Brightened
            Color brightenedTarget = new Color(
                targetColor.r * pulseIntensity,
                targetColor.g * pulseIntensity,
                targetColor.b * pulseIntensity,
                1f
            );

            await Tween.Custom(
                startColor,
                brightenedTarget,
                transitionDuration,
                onValueChange: color =>
                {
                    if (_screenMaterial == null) return;
                    _screenMaterial.SetColor(OverlayColorProperty, color);
                },
                ease: Ease.InOutSine
            ).ToYieldInstruction().ToUniTask(cancellationToken: _colorChangeCts);

            StartPulse(targetColor, pulseDuration, pulseIntensity);
        }
        catch (OperationCanceledException ex)
        {
            Log.Exception(ex);
        }
    }
}