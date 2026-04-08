using System;
using UnityEngine;

// up# = (Sin(Shake) / (20.0+CrouchState*20.0))*0.6
// side# = Cos(Shake / 2.0) / 35.0
// roll# = Max(Min(Sin(Shake/2)*2.5*Min(Injuries+0.25,3.0),8.0),-8.0)

[System.Serializable]
public class BobbingState
{
    public float bobMultiplier = 1f;
    public float verticalIntensity = 1f;
    public float horizontalIntensity = 1f;
    public float rollMultiplier = 1f;
}

public class PlayerBobbing : MonoBehaviour
{
    public event Action OnFootstepTrigger;

    [Space]
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private bool enableTilt = true;

    [Header("Curves")]
    [SerializeField] private float bobSpeed = 7f;
    [SerializeField] private float rollTransitionSpeed = 8f;
    [Space]
    [SerializeField] private AnimationCurve upDownCurve;
    [SerializeField] private AnimationCurve sideCurve;
    [SerializeField] private AnimationCurve rollCurve;

    [Header("States")]
    [SerializeField] private BobbingState walkState = new BobbingState { bobMultiplier = 1f, verticalIntensity = 1f, horizontalIntensity = 1f, rollMultiplier = 1f };
    [SerializeField] private BobbingState sprintState = new BobbingState { bobMultiplier = 1f, verticalIntensity = 1f, horizontalIntensity = 1f, rollMultiplier = 1f };
    [SerializeField] private BobbingState crouchState = new BobbingState { bobMultiplier = 1f, verticalIntensity = 1f, horizontalIntensity = 1f, rollMultiplier = 1f };

    [Header("Injury Impact")]
    [SerializeField] private float injuryBaseOffset = 0.25f;
    [SerializeField] private float injuryMaxCap = 3f;

    public bool EnableBobbing { get => enableBobbing; set => enableBobbing = value; }
    public bool EnableTilt { get => enableTilt; set => enableTilt = value; }

    private Vector3 _defaultLocalPosition;
    private Vector3 _defaultRotation;

    private float _bobbingTime = 0f;
    private float _prevBobbingTime = 0f;
    private float _currentRollMultiplier = 1f;
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

        UpdateBobbing();
    }

    public void ResetBobbing()
    {
        _bobbingTime = 0.75f;
        _prevBobbingTime = 0.75f;
        UpdateBobbing();
    }

    private BobbingState GetCurrentState()
    {
        switch (Core.Player.CurrentState)
        {
            case PlayerState.Sprinting: return sprintState;
            case PlayerState.Crouching: return crouchState;
            default: return walkState;
        }
    }

    private void UpdateBobbing()
    {
        BobbingState state = GetCurrentState();

        if (Core.Player.Controller.IsMoving && Core.Player.CharacterController.isGrounded)
        {
            _prevBobbingTime = _bobbingTime;

            float currentSpeed = Core.Player.Controller.DetermineCurrentSpeed();
            float walkSpeed = Core.Player.Controller.WalkSpeed;
            float speedScale = currentSpeed / walkSpeed;

            _bobbingTime += Time.deltaTime * bobSpeed * speedScale * state.bobMultiplier;

            float prev = _prevBobbingTime % 1f;
            float curr = _bobbingTime % 1f;
            if (prev < 0.5f && curr >= 0.5f)
            {
                OnFootstepTrigger?.Invoke();
            }
        }

        float t = Mathf.Repeat(_bobbingTime, 1f);
        float tHalf = Mathf.Repeat(_bobbingTime / 2f, 1f);
        float crouchVal = Core.Player.Controller.CrouchState;

        float up = 0f;
        float side = 0f;
        float roll = 0f;

        if (enableBobbing)
        {
            float bobDivisor = 20f + (crouchVal * 20f);
            up = (upDownCurve.Evaluate(t) / bobDivisor) * 0.6f * state.verticalIntensity;
            side = sideCurve.Evaluate(tHalf) / 35f * state.horizontalIntensity;
        }

        _currentRollMultiplier = Mathf.Lerp(_currentRollMultiplier, state.rollMultiplier, Time.deltaTime * rollTransitionSpeed);
        if (enableTilt)
        {
            float rawRoll = rollCurve.Evaluate(tHalf) * 2.5f * _currentInjuryFactor * _currentRollMultiplier;
            roll = Mathf.Clamp(rawRoll, -8f, 8f);
        }

        Core.Player.CameraRoot.transform.localRotation = Quaternion.Euler(
            _defaultRotation.x,
            _defaultRotation.y,
            _defaultRotation.z + roll
        );

        Core.Player.CameraRoot.transform.localPosition = _defaultLocalPosition + new Vector3(side, up, 0f);
    }
}