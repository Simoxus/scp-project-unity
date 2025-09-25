using PrimeTween;
using TMPro;
using UnityEngine;

// Attach to any keycard prefab thingy
public class KeycardVisual : MonoBehaviour
{
    [Header("Button Settings")]
    public GameObject buttonScreen;
    public GameObject buttonScreenTextObject;
    public TextMeshPro buttonScreenText;
    public Light buttonEmission;
    public MeshRenderer buttonScreenLogo;

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