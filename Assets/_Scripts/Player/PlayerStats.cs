using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;

    [Header("Sprint Settings")]
    //[SerializeField] private float sprintThreshold = 0.02f;
    [SerializeField] private float sprintDrainRate = 0.2f;
    [SerializeField] private float sprintRegenRate = 0.1f;
    [SerializeField] private float sprintRegenDelay = 0.5f;

    [Header("Blink Settings")]
    [SerializeField] private float blinkCost = 0.25f;

    public float currentSprint = 1f;

    private float _sprintRegenDelayTimer = 0f;
    //private bool _sprintNeedsReset = false;

    private bool _isSprinting = false;
    private bool _isMoving = false;
    private bool _isCrouching = false;
    private bool _isBlinking = false;

    // Called by PlayerController to update the current states
    public void SetCurrentState(bool isSprinting, bool isMoving, bool isCrouching)
    {
        _isSprinting = isSprinting;
        _isMoving = isMoving;
        _isCrouching = isCrouching;
    }

    private void Awake()
    {
        // Check for player and if there's no player, try to find the singleton/instance
        player = player != null ? player : Player.Instance;
    }

    private void Update()
    {
        HandleSprint();
        HandleBlink();
    }

    private void HandleSprint()
    {
        // Handle stamina drain
        if (_isSprinting && _isMoving && !_isCrouching)
        {
            currentSprint -= sprintDrainRate * Time.deltaTime;
            _sprintRegenDelayTimer = sprintRegenDelay;
        }
        // Handle stamina regen
        else
        {
            if (_sprintRegenDelayTimer > 0f)
            {
                _sprintRegenDelayTimer -= Time.deltaTime;
            }
            else
            {
                if (!_isCrouching && !_isBlinking)
                {
                    currentSprint += sprintRegenRate * Time.deltaTime;
                }
            }
        }
        currentSprint = Mathf.Clamp01(currentSprint);
    }

    private void HandleBlink()
    {
        _isBlinking = player.playerInputs.blinkPressed && currentSprint >= blinkCost;
        if (_isBlinking)
        {
            currentSprint -= blinkCost;
            _sprintRegenDelayTimer = sprintRegenDelay;
            player.playerInputs.ResetBlink();
        }
    }
}