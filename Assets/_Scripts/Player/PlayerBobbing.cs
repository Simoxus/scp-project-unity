using System;
using UnityEngine;

// up# = (Sin(Shake) / (20.0+CrouchState*20.0))*0.6
// side# = Cos(Shake / 2.0) / 35.0
// roll# = Max(Min(Sin(Shake/2)*2.5*Min(Injuries+0.25,3.0),8.0),-8.0)

[System.Serializable]
public class BobbingState
{
    [Tooltip("How fast the bob cycles/per second")]
    public float bobSpeed = 7f;
    [Tooltip("Intensity of head bobbing")]
    public float verticalBobIntensity = 1f;
    [Tooltip("Intensity of head swaying")]
    public float horizontalBobIntensity = 1f;
    [Tooltip("Intensity of camera rolling")]
    public float rollIntensity = 1f;
    [Tooltip("Maximum roll angle in degrees")]
    public float maxRollAngle = 8f;
}

public class PlayerBobbing : MonoBehaviour
{
    public event Action OnFootstepTrigger;

    [Space]
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private bool enableTilt = true;

    [Header("State Configurations")]
    [SerializeField]
    private BobbingState walkState = new BobbingState
    {
        bobSpeed = 7f,
        verticalBobIntensity = 0.6f,
        horizontalBobIntensity = 1f,
        rollIntensity = 2.5f,
        maxRollAngle = 8f
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

    private float _shake;
    private float _currentBobSpeed;
    private float _currentVerticalBobIntensity;
    private float _currentHorizontalBobIntensity;
    private float _currentRollIntensity;
    private float _currentMaxRollAngle;

    private float _currentInjuryFactor = 0.25f;

    private void Start()
    {
        if (Core.Player.CameraRoot.transform != null)
        {
            _defaultLocalPosition = Core.Player.CameraRoot.transform.localPosition;
            _defaultRotation = Core.Player.CameraRoot.transform.localRotation.eulerAngles;
        }

        if (Core.Player.Health != null)
        {
            Core.Player.Health.OnInjuryChanged += UpdateInjuryFactor;
            UpdateInjuryFactor(Core.Player.Health.GetInjuryFactor());
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
        if (Core.Player.CameraRoot.transform == null || !enabled) return;
        if (Core.Player.Controller == null) return;
        if (Core.Player.Controller.IsNoclipping) return;

        BobbingState targetState = GetCurrentTargetState();
        BlendToState(targetState, Time.deltaTime * stateTransitionSpeed);

        UpdateBobbing();
    }

    public void ResetBobbing()
    {
        _shake = 270f;
        UpdateBobbing();
    }

    private void UpdateBobbing()
    {
        // Only increment shake when movign
        if (Core.Player.Controller.IsMoving && Core.Player.CharacterController.isGrounded)
        {
            float previousShake = _shake;
            _shake += Time.deltaTime * _currentBobSpeed * 60f;

            float prevMod = previousShake % 360f;
            float currMod = _shake % 360f;
            if (prevMod < 180f && currMod >= 180f)
            {
                OnFootstepTrigger?.Invoke();
            }
        }

        float shakeRadians = _shake * Mathf.Deg2Rad;
        float shakeHalfRadians = (_shake / 2f) * Mathf.Deg2Rad;

        float crouchState = Core.Player.Controller.CrouchState;

        float up = 0f;
        float side = 0f;
        float roll = 0f;

        if (enableBobbing)
        {
            float bobDivisor = 20f + (crouchState * 20f);
            up = (Mathf.Sin(shakeRadians) / bobDivisor) * 0.6f * _currentVerticalBobIntensity;
            side = (Mathf.Cos(shakeHalfRadians) / 35f) * _currentHorizontalBobIntensity;
        }

        if (enableTilt)
        {
            float injuryFactor = Mathf.Min(_currentInjuryFactor, 3f);
            float rawRoll = Mathf.Sin(shakeHalfRadians) * 2.5f * injuryFactor;
            roll = Mathf.Clamp(rawRoll, -_currentMaxRollAngle, _currentMaxRollAngle);
            roll *= _currentRollIntensity;
        }

        Core.Player.CameraRoot.transform.localPosition = _defaultLocalPosition;
        Core.Player.CameraRoot.transform.localRotation = Quaternion.Euler(_defaultRotation);

        Core.Player.CameraRoot.transform.localRotation = Quaternion.Euler(
            _defaultRotation.x,
            _defaultRotation.y,
            _defaultRotation.z + roll * 0.5f
        );

        Vector3 offset = new Vector3(
            side,
            up,
            0f
        );

        Core.Player.CameraRoot.transform.localPosition = _defaultLocalPosition + offset;
    }

    private BobbingState GetCurrentTargetState()
    {
        switch (Core.Player.CurrentState)
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