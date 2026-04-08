using UnityEngine;
using UnityEngine.UI;

public class UIIndicators : MonoBehaviour
{
    [Space]
    public CanvasGroup canvasGroup;

    [Header("Blink")]
    public CanvasGroup blinkMeter;
    public Image blinkIcon;
    public BarProgress blinkBar;

    [Header("Sprint")]
    public CanvasGroup sprintMeter;
    public Image sprintIcon;
    public BarProgress sprintBar;

    [Header("Sprites")]
    public Sprite sprintIconSprite;
    public Sprite crouchIconSprite;

    private bool _isVisible;

    public bool IsVisible => _isVisible;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    public void Show()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        _isVisible = true;
    }

    public void Hide()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        _isVisible = false;
    }

    public void Toggle()
    {
        if (_isVisible)
            Hide();
        else
            Show();
    }

    public void SetProgress(float currentBlink, float currentSprint)
    {
        if (blinkBar != null)
        {
            blinkBar.SetProgress(Mathf.Clamp01(currentBlink));
        }

        if (sprintBar != null)
        {
            sprintBar.SetProgress(Mathf.Clamp01(currentSprint));
        }
    }

    public void SetSprintProgress(float currentSprint)
    {
        if (sprintBar != null)
        {
            sprintBar.SetProgress(Mathf.Clamp01(currentSprint));
        }
    }

    public void SetBlinkProgress(float currentBlink)
    {
        if (blinkBar != null)
        {
            blinkBar.SetProgress(Mathf.Clamp01(currentBlink));
        }
    }

    public void SetSprintIcon(PlayerState state)
    {
        if (sprintIcon == null) return;
        sprintIcon.sprite = state switch
        {
            PlayerState.Sprinting => sprintIconSprite,
            PlayerState.Crouching => crouchIconSprite,
            _ => sprintIconSprite
        };
    }

    public void ShowBlinkMeter()
    {
        if (blinkMeter != null)
            ShowCanvasGroup(blinkMeter);
    }

    public void HideBlinkMeter()
    {
        if (blinkMeter != null)
            HideCanvasGroup(blinkMeter);
    }

    public void ShowSprintMeter()
    {
        if (sprintMeter != null)
            ShowCanvasGroup(sprintMeter);
    }

    public void HideSprintMeter()
    {
        if (sprintMeter != null)
            HideCanvasGroup(sprintMeter);
    }

    // Helper methods for CanvasGroup manipulation
    private static void ShowCanvasGroup(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private static void HideCanvasGroup(CanvasGroup cg)
    {
        if (cg == null) return;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}