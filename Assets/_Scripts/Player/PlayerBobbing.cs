using System;
using UnityEngine;

[System.Serializable]
public class BobbingState
{
    [Header("Animation Speed")]
    [Tooltip("How fast the bob cycles. Higher = faster bobbing")]
    public float bobSpeed = 8f;

    [Header("Bob Settings")]
    [Tooltip("How much vertical movement. SCP-CB style uses 0.6 relative to divisor")]
    public float bobAmount = 0.1f;
    [Tooltip("Divisor for vertical bobbing (higher = less bob). SCP-CB uses 20.0 for standing, 40.0 for crouching")]
    public float upDownDivisor = 20f;

    [Header("Tilt Settings")]
    [Tooltip("How strong the head tilt is")]
    public float rotationStrength = 0.7f;
    [Tooltip("Maximum tilt angle in degrees")]
    public float maxRotationAngle = 8f;
    [Tooltip("Speed of rotation relative to bob")]
    public float rotationSpeed = 0.5f;
}

public class PlayerBobbing : MonoBehaviour
{
    public event Action OnFootstepTrigger;

    [Header("References")]
    [SerializeField] private Player player;

    [Header("Behavior Settings")]
    public bool enableBobbing = true;
    public bool enableTilt = true;

    [Header("State Configurations")]
    [SerializeField]
    private BobbingState walkState = new BobbingState
    {
        bobSpeed = 7f,
        bobAmount = 0.05f,
        upDownDivisor = 20f,
        rotationStrength = 0.4f,
        maxRotationAngle = 2f,
        rotationSpeed = 0.5f
    };

    [SerializeField]
    private BobbingState sprintState = new BobbingState
    {
        bobSpeed = 13f,
        bobAmount = 0.04f,
        upDownDivisor = 15f,
        rotationStrength = 0.4f,
        maxRotationAngle = 3f,
        rotationSpeed = 0.5f
    };

    [SerializeField]
    private BobbingState crouchState = new BobbingState
    {
        bobSpeed = 6.3f,
        bobAmount = 0.08f,
        upDownDivisor = 40f,
        rotationStrength = 0.5f,
        maxRotationAngle = 6f,
        rotationSpeed = 0.5f
    };

    [Header("Injury Impact")]
    [SerializeField, Range(0f, 1f)] private float injuryBobMultiplier = 0.15f;
    [SerializeField, Range(0f, 1f)] private float injuryTiltMultiplier = 0.3f;

    [Header("Transition Settings")]
    [SerializeField] private float stateTransitionSpeed = 9f;
    [SerializeField] private float bobbingLerpSpeed = 12f;

    private Vector3 _defaultLocalPosition;
    private Vector3 _defaultRotation;

    private float _bobTimer;
    private bool _hasPlayedFootstep;

    private float _currentBobSpeed;
    private float _currentBobAmount;
    private float _currentRotationStrength;
    private float _currentMaxRotationAngle;
    private float _currentRotationSpeed;

    private Vector3 _lastAppliedPosition;
    private float _lastAppliedRotation;

    private float _cachedInjuryBobMultiplier;
    private float _cachedInjuryTiltMultiplier;

    private void Awake()
    {
        player = player != null ? player : Player.Instance;
    }

    private void Reset()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        if (player.cameraRoot.transform != null)
        {
            _defaultLocalPosition = player.cameraRoot.transform.localPosition;
            _defaultRotation = player.cameraRoot.transform.localRotation.eulerAngles;
            _lastAppliedPosition = _defaultLocalPosition;
            _lastAppliedRotation = 0f;
        }

        if (player.playerHealth != null)
        {
            player.playerHealth.OnInjuryChanged += UpdateInjuryMultiplier;
            UpdateInjuryMultiplier(player.playerHealth.GetInjuryFactor());
        }

        InitializeCurrentState(walkState);
    }

    private void UpdateInjuryMultiplier(float injuries)
    {
        _cachedInjuryBobMultiplier = 1f + (injuries * injuryBobMultiplier);
        _cachedInjuryTiltMultiplier = 1f + (injuries * injuryTiltMultiplier);
    }

    private void Update()
    {
        if (player.cameraRoot.transform == null || !enabled) return;
        if (player.playerController == null) return;

        BobbingState targetState = GetCurrentTargetState();
        BlendToState(targetState, Time.deltaTime * stateTransitionSpeed);

        if (player.playerController.isMoving && player.characterController.isGrounded)
        {
            UpdateBobbing();
        }
        else
        {
            ReturnToRest();
        }
    }

    private void UpdateBobbing()
    {
        _bobTimer += Time.deltaTime * _currentBobSpeed;

        float bobOffset = 0f;
        float rotationOffset = 0f;
        float sideOffset = 0f;

        if (enableBobbing)
        {
            bobOffset = Mathf.Sin(_bobTimer) * _currentBobAmount * _cachedInjuryBobMultiplier;
            sideOffset = Mathf.Cos(_bobTimer / 2f) / 35f * _cachedInjuryBobMultiplier;
        }

        if (enableTilt)
        {
            rotationOffset = Mathf.Sin(_bobTimer * _currentRotationSpeed) *
                           _currentMaxRotationAngle * _currentRotationStrength * _cachedInjuryTiltMultiplier;
        }

        Vector3 targetPosition = _defaultLocalPosition;
        targetPosition.y += bobOffset;
        targetPosition.x += sideOffset;

        float lerpFactor = Time.deltaTime * bobbingLerpSpeed;
        _lastAppliedPosition = Vector3.Lerp(_lastAppliedPosition, targetPosition, lerpFactor);
        _lastAppliedRotation = Mathf.Lerp(_lastAppliedRotation, rotationOffset, lerpFactor);

        player.cameraRoot.transform.localPosition = _lastAppliedPosition;
        player.cameraRoot.transform.localRotation = Quaternion.Euler(
            _defaultRotation.x,
            _defaultRotation.y,
            _defaultRotation.z + _lastAppliedRotation
        );

        if (bobOffset < 0 && !_hasPlayedFootstep)
        {
            OnFootstepTrigger?.Invoke();
            _hasPlayedFootstep = true;
        }
        else if (bobOffset >= 0)
        {
            _hasPlayedFootstep = false;
        }
    }

    private void ReturnToRest()
    {
        _bobTimer = Mathf.Lerp(_bobTimer, 0f, Time.deltaTime * 5f);

        _lastAppliedPosition = Vector3.Lerp(
            _lastAppliedPosition,
            _defaultLocalPosition,
            Time.deltaTime * stateTransitionSpeed * 1.5f
        );

        _lastAppliedRotation = Mathf.Lerp(
            _lastAppliedRotation,
            0f,
            Time.deltaTime * stateTransitionSpeed * 1.5f
        );

        player.cameraRoot.transform.localPosition = _lastAppliedPosition;
        player.cameraRoot.transform.localRotation = Quaternion.Euler(
            _defaultRotation.x,
            _defaultRotation.y,
            _defaultRotation.z + _lastAppliedRotation
        );

        _hasPlayedFootstep = false;
    }

    private BobbingState GetCurrentTargetState()
    {
        switch (player.currentState)
        {
            case PlayerState.Idle:
            case PlayerState.Walking:
                return walkState;
            case PlayerState.Sprinting:
                return sprintState;
            case PlayerState.Crouching:
                return crouchState;
            case PlayerState.Freefall:
            default:
                return walkState;
        }
    }

    private void InitializeCurrentState(BobbingState state)
    {
        _currentBobSpeed = state.bobSpeed;
        _currentBobAmount = state.bobAmount;
        _currentRotationStrength = state.rotationStrength;
        _currentMaxRotationAngle = state.maxRotationAngle;
        _currentRotationSpeed = state.rotationSpeed;
    }

    private void BlendToState(BobbingState target, float blendFactor)
    {
        _currentBobSpeed = Mathf.Lerp(_currentBobSpeed, target.bobSpeed, blendFactor);
        _currentBobAmount = Mathf.Lerp(_currentBobAmount, target.bobAmount, blendFactor);
        _currentRotationStrength = Mathf.Lerp(_currentRotationStrength, target.rotationStrength, blendFactor);
        _currentMaxRotationAngle = Mathf.Lerp(_currentMaxRotationAngle, target.maxRotationAngle, blendFactor);
        _currentRotationSpeed = Mathf.Lerp(_currentRotationSpeed, target.rotationSpeed, blendFactor);
    }
}