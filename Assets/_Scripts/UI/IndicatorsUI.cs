using UnityEngine;
using UnityEngine.UI;

public class IndicatorsUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject blinkMeter;
    public Image blinkIcon;
    public BarProgress blinkBar;

    public GameObject sprintMeter;
    public Image sprintIcon;
    public BarProgress sprintBar;

    [Header("Sprite References")]
    public Sprite sprintIconSprite;
    public Sprite crouchIconSprite;

    public void UpdateIndicators(float currentSprint, float currentBlink, PlayerState playerState)
    {
        // Update blink meter (0-1 range)
        blinkBar.SetProgress(Mathf.Clamp01(currentBlink));

        // Update sprint meter (0-1 range, but clamp to 0 for display if negative)
        sprintBar.SetProgress(Mathf.Clamp01(currentSprint));

        // Update sprint/crouch icon based on player state
        switch (playerState)
        {
            case PlayerState.Sprinting:
                sprintIcon.sprite = sprintIconSprite;
                break;
            case PlayerState.Crouching:
                sprintIcon.sprite = crouchIconSprite;
                break;
            case PlayerState.Walking:
            case PlayerState.Idle:
            case PlayerState.Freefall:
                // Keep sprint icon for other states
                sprintIcon.sprite = sprintIconSprite;
                break;
        }
    }
}