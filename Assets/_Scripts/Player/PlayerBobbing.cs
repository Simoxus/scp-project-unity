using System;
using UnityEngine;

[System.Serializable]
public class BobbingState
{
    [Tooltip("How fast the bob cycles per second")]
    public float bobSpeed = 7f;
    [Tooltip("Intensity of vertical head bobbing")]
    public float verticalBobIntensity = 1f;
    [Tooltip("Intensity of horizontal head swaying")]
    public float horizontalBobIntensity = 1f;
    [Tooltip("Intensity of camera rolling/tilting")]
    public float rollIntensity = 1f;
    [Tooltip("Maximum roll angle in degrees")]
    public float maxRollAngle = 8f;
}

public class PlayerBobbing : MonoBehaviour
{
    public event Action OnFootstepTrigger;

    [Header("References")]
    [SerializeField] private Player player;

    [Header("Behavior Settings")]
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private bool enableTilt = true;

    [Header("State Configurations")]
    [SerializeField]
    private BobbingState walkState = new BobbingState
    {
        bobSpeed = 7f,
        verticalBobIntensity = 1f,
        horizontalBobIntensity = 1f,
        rollIntensity = 3.5f,
        maxRollAngle = 17f
    };

    [SerializeField]
    private BobbingState sprintState = new BobbingState
    {
        bobSpeed = 10.5f,
        verticalBobIntensity = 1f,
        horizontalBobIntensity = 1f,
        rollIntensity = 4f,
        maxRollAngle = 12f
    };

    [SerializeField]
    private BobbingState crouchState = new BobbingState
    {
        bobSpeed = 6.5f,
        verticalBobIntensity = 1f,
        horizontalBobIntensity = 1f,
        rollIntensity = 5f,
        maxRollAngle = 14f
    };

    [Header("Injury Impact")]
    [SerializeField] private float injuryBaseOffset = 0.25f;
    [SerializeField] private float injuryMaxCap = 2f;

    [Header("Transition Settings")]
    [SerializeField] private float stateTransitionSpeed = 13f;
    [SerializeField] private float bobbingLerpSpeed = 14f;

    public bool EnableBobbing
    {
        get => enableBobbing;
        set => enableBobbing = value;
    }

    public bool EnableTilt
    {
        get => enableTilt;
        set => enableTilt = value;
    }

    private Vector3 _defaultLocalPosition;
    private Vector3 _defaultRotation;

    private float _bobTimer;
    private float _currentBobSpeed;
    private float _currentVerticalBobIntensity;
    private float _currentHorizontalBobIntensity;
    private float _currentRollIntensity;
    private float _currentMaxRollAngle;

    private Vector3 _lastAppliedPosition;
    private float _lastAppliedRotation;

    private float _currentInjuryFactor = 0.25f;

    private void Start()
    {
        if (player.CameraRoot.transform != null)
        {
            _defaultLocalPosition = player.CameraRoot.transform.localPosition;
            _defaultRotation = player.CameraRoot.transform.localRotation.eulerAngles;
            _lastAppliedPosition = _defaultLocalPosition;
            _lastAppliedRotation = 0f;
        }

        if (player.PlayerHealth != null)
        {
            player.PlayerHealth.OnInjuryChanged += UpdateInjuryFactor;
            UpdateInjuryFactor(player.PlayerHealth.GetInjuryFactor());
        }

        _currentBobSpeed = walkState.bobSpeed;
        _currentVerticalBobIntensity = walkState.verticalBobIntensity;
        _currentHorizontalBobIntensity = walkState.horizontalBobIntensity;
        _currentRollIntensity = walkState.rollIntensity;
        _currentMaxRollAngle = walkState.maxRollAngle;
    }

    private void UpdateInjuryFactor(float injuries)
    {
        _currentInjuryFactor = Mathf.Min(injuries + injuryBaseOffset, injuryMaxCap);
    }

    private void Update()
    {
        if (player.CameraRoot.transform == null || !enabled) return;
        if (player.PlayerController == null) return;
        if (player.PlayerController.IsNoclipping) return;

        BobbingState targetState = GetCurrentTargetState();
        BlendToState(targetState, Time.deltaTime * stateTransitionSpeed);

        if (player.PlayerController.IsMoving && player.CharacterController.isGrounded)
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
        float previousTimer = _bobTimer;

        float degreesPerSecond = _currentBobSpeed * 60f;
        _bobTimer += Time.deltaTime * degreesPerSecond;

        if (_bobTimer >= 720f)
        {
            _bobTimer = _bobTimer % 720f;
        }

        float prevMod = previousTimer % 360f;
        float currMod = _bobTimer % 360f;

        if (prevMod < 180f && currMod >= 180f)
        {
            OnFootstepTrigger?.Invoke();
        }

        float shakeRadians = _bobTimer * Mathf.Deg2Rad;
        float shakeRadiansHalf = (_bobTimer / 2f) * Mathf.Deg2Rad;

        float bobOffset = 0f;
        float sideOffset = 0f;
        float rotationOffset = 0f;

        float crouchState = player.PlayerController.CrouchState;

        if (enableBobbing)
        {
            float bobDivisor = 20f + (crouchState * 20f);
            bobOffset = (Mathf.Sin(shakeRadians) / bobDivisor * 0.6f) * _currentVerticalBobIntensity;
            sideOffset = (Mathf.Cos(shakeRadiansHalf) / 35f) * _currentHorizontalBobIntensity;
        }

        if (enableTilt)
        {
            float injuryFactor = Mathf.Min(_currentInjuryFactor, 3f);
            float rawTilt = Mathf.Sin(shakeRadiansHalf) * 2.5f * injuryFactor;
            float clampedTilt = Mathf.Clamp(rawTilt, -_currentMaxRollAngle, _currentMaxRollAngle);
            rotationOffset = clampedTilt * _currentRollIntensity;
        }

        Vector3 targetPosition = _defaultLocalPosition;
        targetPosition.y += bobOffset;
        targetPosition.x += sideOffset;

        float lerpFactor = Time.deltaTime * bobbingLerpSpeed;
        _lastAppliedPosition = Vector3.Lerp(_lastAppliedPosition, targetPosition, lerpFactor);
        _lastAppliedRotation = Mathf.Lerp(_lastAppliedRotation, rotationOffset * 0.5f, lerpFactor);

        player.CameraRoot.transform.localPosition = _lastAppliedPosition;
        player.CameraRoot.transform.localRotation = Quaternion.Euler(
            _defaultRotation.x,
            _defaultRotation.y,
            _defaultRotation.z + _lastAppliedRotation
        );
    }

    private void ReturnToRest()
    {
        _bobTimer = Mathf.Lerp(_bobTimer, 0f, Time.deltaTime * 3f);

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

        player.CameraRoot.transform.localPosition = _lastAppliedPosition;
        player.CameraRoot.transform.localRotation = Quaternion.Euler(
            _defaultRotation.x,
            _defaultRotation.y,
            _defaultRotation.z + _lastAppliedRotation
        );
    }

    private BobbingState GetCurrentTargetState()
    {
        switch (player.CurrentState)
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

    private void BlendToState(BobbingState target, float blendFactor)
    {
        _currentBobSpeed = Mathf.Lerp(_currentBobSpeed, target.bobSpeed, blendFactor);
        _currentVerticalBobIntensity = Mathf.Lerp(_currentVerticalBobIntensity, target.verticalBobIntensity, blendFactor);
        _currentHorizontalBobIntensity = Mathf.Lerp(_currentHorizontalBobIntensity, target.horizontalBobIntensity, blendFactor);
        _currentRollIntensity = Mathf.Lerp(_currentRollIntensity, target.rollIntensity, blendFactor);
        _currentMaxRollAngle = Mathf.Lerp(_currentMaxRollAngle, target.maxRollAngle, blendFactor);
    }
}