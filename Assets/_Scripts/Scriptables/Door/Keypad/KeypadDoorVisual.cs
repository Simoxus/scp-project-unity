using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class KeypadDoorVisual : MonoBehaviour
{
    [Header("Keypad Settings")]
    public string correctCode = "6767";
    public int maxCodeLength = 4;
    public float resetDelay = 1.5f;

    [Header("Keypad References")]
    public GameObject keypadScreen;
    public GameObject keypadTextObject;
    public TextMeshPro keypadText;
    public MeshRenderer keypadScreenRenderer;
    public MeshRenderer keypadLogo;

    [Header("Material Settings")]
    public int screenMaterialIndex = 1;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseDuration = 0.6f;
    [SerializeField] private float pulseMaxBrightness = 1.2f;

    [Header("Individual Keys")]
    public GameObject[] keyMeshes;
    public GameObject enterKeyMesh;
    public GameObject clearKeyMesh;

    [Header("Tween Settings")]
    public float tweenPushTime = 0.15f;
    public float meshPushedOffset = -0.03f;

    private Dictionary<GameObject, float> _originalKeyPositions;
    private CancellationToken _colorChangeCts;
    private Material _screenMaterial;
    private Tween _pulseTween;
    private static readonly int OverlayColorProperty = Shader.PropertyToID("_OverlayColor");

    private void Awake()
    {
        _colorChangeCts = this.GetCancellationTokenOnDestroy();
        if (keypadScreenRenderer != null && keypadScreenRenderer.materials.Length > screenMaterialIndex)
        {
            Material[] materials = keypadScreenRenderer.materials;
            _screenMaterial = materials[screenMaterialIndex];
        }
    }

    private void Start()
    {
        _originalKeyPositions = new Dictionary<GameObject, float>();

        // Add number keys
        foreach (GameObject key in keyMeshes)
        {
            if (key != null)
            {
                _originalKeyPositions[key] = key.transform.localPosition.z;
            }
        }

        // Add special keys
        if (enterKeyMesh != null)
        {
            _originalKeyPositions[enterKeyMesh] = enterKeyMesh.transform.localPosition.z;
        }

        if (clearKeyMesh != null)
        {
            _originalKeyPositions[clearKeyMesh] = clearKeyMesh.transform.localPosition.z;
        }
    }

    // Generic key press animation - works for any key
    private async UniTask PlayKeyPressAnimation(GameObject keyMesh)
    {
        if (keyMesh == null || !_originalKeyPositions.ContainsKey(keyMesh)) return;

        float originalZ = _originalKeyPositions[keyMesh];
        float pushedZ = originalZ + meshPushedOffset;

        // Push key in
        await Tween.LocalPositionZ(
            keyMesh.transform,
            pushedZ,
            duration: tweenPushTime,
            ease: Ease.Linear
        ).ToYieldInstruction().ToUniTask();

        await UniTask.WaitForSeconds(tweenPushTime + 0.015f, ignoreTimeScale: false);

        // Push key out
        await Tween.LocalPositionZ(
            keyMesh.transform,
            originalZ,
            duration: tweenPushTime,
            ease: Ease.Linear
        ).ToYieldInstruction().ToUniTask();
    }

    public async UniTask PlayNumberKeyTween(GameObject keyMesh)
    {
        await PlayKeyPressAnimation(keyMesh);
    }

    public async UniTask PlayEnterKeyTween()
    {
        await PlayKeyPressAnimation(enterKeyMesh);
    }

    public async UniTask PlayClearKeyTween()
    {
        await PlayKeyPressAnimation(clearKeyMesh);
    }

    public void ToggleLogo(bool enabled)
    {
        if (keypadLogo != null)
        {
            keypadLogo.enabled = enabled;
        }
    }

    public void ToggleText(bool enabled)
    {
        if (keypadTextObject != null)
        {
            keypadTextObject.SetActive(enabled);
        }
    }

    public void ChangeScreenText(string requestedText)
    {
        if (keypadTextObject != null)
        {
            keypadText.text = requestedText;
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