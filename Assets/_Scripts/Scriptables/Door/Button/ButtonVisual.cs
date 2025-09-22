using Cysharp.Threading.Tasks;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Attach to any button
public class ButtonVisual : MonoBehaviour
{
    [Header("Button Settings")]
    public GameObject buttonMesh;
    public GameObject buttonScreen;
    public GameObject buttonScreenTextObject;
    public TextMeshPro buttonScreenText;
    public Light buttonEmission;
    public MeshRenderer buttonScreenLogo;

    [Header("Tween Settings")]
    public float tweenPushTime = 0.15f;
    public float meshPushedOffset = 0.01f;

    private float _originalMeshLocalPositionZ; // Change to store local Z position

    private void Start()
    {
        // Store the local Z position relative to its parent
        _originalMeshLocalPositionZ = buttonMesh.transform.localPosition.z;
    }

    public async UniTask PlayTween()
    {
        // When tweening, use localPosition instead of global position
        await Tween.LocalPositionZ(
            buttonMesh.transform,
            _originalMeshLocalPositionZ + meshPushedOffset,
            duration: tweenPushTime
        );

        await UniTask.WaitForSeconds(tweenPushTime + 0.015f, ignoreTimeScale: false);

        await Tween.LocalPositionZ( // Use LocalPositionZ here as well
            buttonMesh.transform,
            _originalMeshLocalPositionZ,
            duration: tweenPushTime
        );
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
        if (doTweenChange)
        {
            await Tween.LightColor(
                buttonEmission,
                requestedColor,
                tweenChangeDuration,
                Ease.Linear
            );
        }
        else
        {
            if (buttonEmission != null)
            {
                buttonEmission.color = requestedColor;
            }
        }
    }
}