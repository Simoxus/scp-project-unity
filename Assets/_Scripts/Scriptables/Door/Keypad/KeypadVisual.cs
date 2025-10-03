using Cysharp.Threading.Tasks;
using PrimeTween;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;

public class KeypadVisual : MonoBehaviour
{
    [Header("Keypad Settings")]
    public string correctCode = "6767";
    public int maxCodeLength = 4;
    public float resetDelay = 1.5f;

    [Header("Keypad References")]
    public GameObject keypadScreen;
    public GameObject keypadTextObject;
    public TextMeshPro keypadText;
    public Light keypadEmission;
    public MeshRenderer keypadLogo;

    [Header("Individual Keys")]
    public GameObject[] keyMeshes;
    public GameObject enterKeyMesh;
    public GameObject clearKeyMesh;

    [Header("Tween Settings")]
    public float tweenPushTime = 0.15f;
    public float meshPushedOffset = -0.03f;

    private Dictionary<GameObject, float> _originalMeshLocalPositionZ;
    private float _originalEnterMeshLocalPositionZ;
    private float _originalClearMeshLocalPositionZ;
    private CancellationToken _destroyToken;

    private void Awake()
    {
        _destroyToken = this.GetCancellationTokenOnDestroy();
    }

    private void Start()
    {
        // Store the local Z position for each key
        _originalMeshLocalPositionZ = new Dictionary<GameObject, float>();
        _originalEnterMeshLocalPositionZ = enterKeyMesh != null ? enterKeyMesh.transform.localPosition.z : 0f;
        _originalClearMeshLocalPositionZ = clearKeyMesh != null ? clearKeyMesh.transform.localPosition.z : 0f;

        foreach (GameObject key in keyMeshes)
        {
            if (key != null)
            {
                _originalMeshLocalPositionZ[key] = key.transform.localPosition.z;
            }
        }
    }

    public async UniTask PlayTween(GameObject keyMesh)
    {
        if (keyMesh == null || !_originalMeshLocalPositionZ.ContainsKey(keyMesh))
        {
            Debug.LogWarning("Key mesh not found or not initialized in dictionary.", keyMesh);
            return;
        }

        float originalZ = _originalMeshLocalPositionZ[keyMesh];
        float pushedZ = originalZ + meshPushedOffset;

        // Tween the key forward (or backward depending on your setup)
        await Tween.LocalPositionZ(
            keyMesh.transform,
            pushedZ,
            duration: tweenPushTime,
            ease: Ease.Linear // Or your desired ease
        ).ToYieldInstruction().ToUniTask();

        // Wait for a short moment
        await UniTask.WaitForSeconds(tweenPushTime + 0.015f, ignoreTimeScale: false);

        // Tween the key back to its original position
        await Tween.LocalPositionZ(
            keyMesh.transform,
            originalZ,
            duration: tweenPushTime,
            ease: Ease.Linear
        ).ToYieldInstruction().ToUniTask();
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
        if (keypadEmission == null) return;

        try
        {
            if (doTweenChange)
            {
                await Tween.LightColor(
                    keypadEmission,
                    requestedColor,
                    tweenChangeDuration,
                    Ease.Linear
                ).ToYieldInstruction().ToUniTask(cancellationToken: _destroyToken);
            }
            else
            {
                if (keypadEmission != null)
                {
                    keypadEmission.color = requestedColor;
                }
            }
        }
        catch (OperationCanceledException)
        {
#if UNITY_EDITOR
            Debug.Log($"ChangeScreenColor was canceled because object was destroyed or scene changed.");
#endif
        }
    }
}
