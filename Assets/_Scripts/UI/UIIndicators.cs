using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PlayerState
{
    Idle,
    Walking,
    Sprinting,
    Crouching
}

public class UIIndicators : MonoBehaviour
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

    public void UpdateIndicators(float currentSprint, float maxSprint, PlayerState playerState)
    {
        //blinkBar.SetProgress(Mathf.Clamp01(currentBlink / maxBlink));
        sprintBar.SetProgress(Mathf.Clamp01(currentSprint / maxSprint));

        switch (playerState)
        {
            case PlayerState.Sprinting:
                sprintIcon.sprite = sprintIconSprite;
                break;
            case PlayerState.Crouching:
                sprintIcon.sprite = crouchIconSprite;
                break;
            case PlayerState.Walking:
                break;
        }
    }
}