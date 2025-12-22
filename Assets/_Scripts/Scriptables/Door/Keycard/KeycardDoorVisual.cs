using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class KeycardDoorVisual : MonoBehaviour
{
    [Header("Button Settings")]
    public GameObject buttonScreen;
    public GameObject buttonScreenTextObject;
    public TextMeshPro buttonScreenText;
    public MeshRenderer buttonScreenRenderer;
    public MeshRenderer buttonScreenLogo;

    [Header("Material Settings")]
    public int screenMaterialIndex = 1;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseDuration = 0.6f;
    [SerializeField] private float pulseMaxBrightness = 1.2f;

    private CancellationToken _colorChangeCts;
    private Material _screenMaterial;
    private Tween _pulseTween;
    private static readonly int OverlayColorProperty = Shader.PropertyToID("_OverlayColor");

    private void Awake()
    {
        _colorChangeCts = this.GetCancellationTokenOnDestroy();
        if (buttonScreenRenderer != null && buttonScreenRenderer.materials.Length > screenMaterialIndex)
        {
            Material[] materials = buttonScreenRenderer.materials;
            _screenMaterial = materials[screenMaterialIndex];
        }
    }

    public void ToggleLogo(bool enabled)
    {
        if (buttonScreenLogo != null)
        {
            buttonScreenLogo.enabled = enabled;
        }
    }

    public void ToggleText(bool enabled)
    {
        if (buttonScreenTextObject != null)
        {
            buttonScreenTextObject.SetActive(enabled);
        }
    }

    public void ChangeScreenText(string requestedText)
    {
        if (buttonScreenTextObject != null)
        {
            buttonScreenText.text = requestedText;
        }
    }

    public async void ChangeScreenColor(Color requestedColor, bool doTweenChange, float tweenChangeDuration = 0.35f)
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
                    onValueChange: color => _screenMaterial.SetColor(OverlayColorProperty, color),
                    ease: Ease.Linear
                ).ToYieldInstruction().ToUniTask(cancellationToken: _colorChangeCts);
            }
            else
            {
                _screenMaterial.SetColor(OverlayColorProperty, requestedColor);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void StartPulse(Color pulseColor, float? customDuration = null, float? customIntensity = null)
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
                _screenMaterial.SetColor(OverlayColorProperty, color);
            },
            ease: Ease.InOutSine,
            cycles: -1,
            cycleMode: CycleMode.Yoyo
        );
    }

    public void StopPulse()
    {
        if (_pulseTween.isAlive)
        {
            _pulseTween.Stop();
        }
    }

    public async void TransitionToPulse(Color targetColor, float transitionDuration, float pulseDuration, float pulseIntensity)
    {
        if (_screenMaterial == null) return;

        try
        {
            // Get the current color from the pulsing material
            Color startColor = _screenMaterial.GetColor(OverlayColorProperty);

            // Stop the current pulse
            StopPulse();

            // Create the brightened target color (same as moving pulse intensity)
            Color brightenedTarget = new Color(
                targetColor.r * pulseIntensity,
                targetColor.g * pulseIntensity,
                targetColor.b * pulseIntensity,
                1f
            );

            // Tween to the brightened target color
            await Tween.Custom(
                startColor,
                brightenedTarget,
                transitionDuration,
                onValueChange: color => _screenMaterial.SetColor(OverlayColorProperty, color),
                ease: Ease.InOutSine
            ).ToYieldInstruction().ToUniTask(cancellationToken: _colorChangeCts);

            // Start pulsing at the new color
            StartPulse(targetColor, pulseDuration, pulseIntensity);
        }
        catch (OperationCanceledException)
        {
        }
    }
}