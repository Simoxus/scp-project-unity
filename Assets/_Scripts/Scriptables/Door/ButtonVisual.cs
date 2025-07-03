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
    public GameObject buttonScreenText;
    public Light buttonEmission;
    public MeshRenderer buttonScreenLogo;

    [Header("Tween Settings")]
    public float tweenPushTime = 0.15f;
    public float meshPushedOffset = 0.01f;

    // Change to store local Z position
    private float _originalMeshLocalPositionZ;

    private void Start()
    {
        // Store the local Z position relative to its parent
        _originalMeshLocalPositionZ = buttonMesh.transform.localPosition.z;
    }

    public async void PlayTween()
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
        buttonScreenLogo.enabled = enabled;
    }

    public void ToggleText(bool enabled)
    {
        buttonScreenText.SetActive(enabled);
    }

    public void ChangeScreenText(string requestedText)
    {
        buttonScreenText.GetComponent<TextMeshPro>().text = requestedText;
    }

    public async void ChangeScreenColor(Color requestedColor, bool doTweenChange)
    {
        if (doTweenChange)
        {
            await Tween.LightColor(
                buttonEmission,
                requestedColor,
                0.20f,
                Ease.Linear
                );
        }
        buttonEmission.color = requestedColor;
    }
}