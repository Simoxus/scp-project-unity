using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Camera Refs")]
    public GameObject CameraRoot;
    public GameObject CameraBrain;
    public CinemachineCamera CameraMain;

    [Header("Char Move")]
    public bool IsMoving;

    public float WalkSpeed = 17f;
    public float SprintSpeed = 26f;

    private bool _hasPlayedFootstep;

    [Header("Camera Look")]
    public float Sensitivity = 1.5f;
    public float FieldOfView = 75f;

    public float TopClamp = 70.0f;
    public float BottomClamp = -75.0f;

    [Header("Gravity")]
    public float GravityPull = -1250f;
    public float FallTimeout = 0f;

    [Header("Jump")]
    public float JumpHeight = 0f;
    public float JumpTimeout = 0f;

    [Header("Ground Verify")]
    public bool Grounded = true;
    public float GroundedOffset = 4.5f;
    public float GroundedRadius = 3.2f;
    public LayerMask GroundLayers;

    // Private Vars: Camera Refs
    private float _cinemachineTargetPitch;

    // Private Vars: Movement
    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // Private Vars: Timeout DeltaTime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private PlayerInput _playerInput;

    private CharacterController _controller;
    private PlayerInputs _input;

    private const float _threshold = 0.01f;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    private void Awake()
    {
        // If no camera, find it
        if (CameraBrain == null)
        {
            CameraBrain = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<PlayerInputs>();
        _playerInput = GetComponent<PlayerInput>();

        if (_input == null)
        {
            Debug.LogError("PlayerInputs component not found on Player.");
        }

        // Reset timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();

        IsMoving = _input.move != Vector2.zero &&
                new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude > 0.1f;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {
        // If there is an input,
        if (_input.look.sqrMagnitude >= _threshold)
        {
            // Don't multiply mouse input by Time.deltaTime
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetPitch += _input.look.y * Sensitivity * deltaTimeMultiplier;
            _rotationVelocity = _input.look.x * Sensitivity * deltaTimeMultiplier;

            // Clamp the target pitch for Cinemachine
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Update Cinemachine's camera target pitch
            CameraRoot.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // Rotate player left & right
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        // Set target speed depending on walk speed, sprint speed, and if sprint key is pressed
        float targetSpeed = _input.IsSprinting ? SprintSpeed : WalkSpeed;

        // NOTE: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // If no input is detected, set the targetSpeed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // Reference to player's current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // NOTE: The T in Lerp is clamped, so we don't need to clamp our speed
            // NOTE: SpeedChangeRate/acceleration has been removed for now. To add it back, just do deltaTime * SpeedChangeRate
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime);

            // Round speed to three decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // Normalise player's input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // If there is a move input detected, rotate the player while they are moving
        if (_input.move != Vector2.zero)
        {
            // Move
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }

        // Move the player
        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // Reset fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // Stop the velocity dropping forever, even when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.IsJumping && _jumpTimeoutDelta <= 0.0f)
            {
                // The square root of H * -2 * G is EQUAL TO how much velocity is needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * GravityPull);
            }

            // Jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            // Reset jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // Fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            // If not grounded, do not jump
            _input.IsJumping = false;
        }

        // Apply more GravityPull over time if under terminal velocity (* by delta time 2x to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += GravityPull * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}
