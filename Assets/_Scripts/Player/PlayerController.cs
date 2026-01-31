using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Space]
    [SerializeField, Min(0)] private float walkSpeed = 12f;
    [SerializeField, Min(0)] private float sprintSpeed = 19.5f;
    [SerializeField, Min(0)] private float crouchSpeed = 5.4f;
    [SerializeField, Min(0)] private float gravity = 400f;

    [Header("Look Settings")]
    [SerializeField] private bool invertYAxis = false;
    [SerializeField] private bool smoothLook = true;
    [SerializeField] private float lookSmoothTime = 0.1f;
    [SerializeField] private float lookSpeed = 2.5f;
    [SerializeField] private float minLookX = -75f;
    [SerializeField] private float maxLookX = 75f;

    [Header("Sprint Settings")]
    [SerializeField] private float accelerationCurve = 13f;
    [SerializeField] private float decelerationCurve = 9f;
    [SerializeField] private float decelerationKick = 3f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2.6f;
    [SerializeField] private float crouchHeight = 1.4f;
    [SerializeField] private float crouchTransitionSpeed = 9f;
    [SerializeField] private float crouchCheckOffset = 0.1f;

    [Header("Noclip Settings")]
    [SerializeField] private float noclipSpeed = 20f;
    [SerializeField] private float noclipSprintMultiplier = 2f;

    // Movement settings
    public float WalkSpeed
    {
        get => walkSpeed;
        set => walkSpeed = value;
    }

    public float SprintSpeed
    {
        get => sprintSpeed;
        set => sprintSpeed = value;
    }

    public float CrouchSpeed
    {
        get => crouchSpeed;
        set => crouchSpeed = value;
    }

    public float Gravity
    {
        get => gravity;
        set => gravity = value;
    }

    // Look settings
    public bool InvertYAxis
    {
        get => invertYAxis;
        set => invertYAxis = value;
    }

    public bool SmoothLook
    {
        get => smoothLook;
        set => smoothLook = value;
    }

    public float LookSmoothTime
    {
        get => lookSmoothTime;
        set => lookSmoothTime = value;
    }

    public float LookSpeed
    {
        get => lookSpeed;
        set => lookSpeed = value;
    }

    // Sprint settings
    public float AccelerationCurve
    {
        get => accelerationCurve;
        set => accelerationCurve = value;
    }

    public float DecelerationCurve
    {
        get => decelerationCurve;
        set => decelerationCurve = value;
    }

    public float DecelerationKick
    {
        get => decelerationKick;
        set => decelerationKick = value;
    }

    // Crouch settings
    public float StandingHeight => standingHeight;
    public float CrouchHeight => crouchHeight;

    public float CrouchTransitionSpeed
    {
        get => crouchTransitionSpeed;
        set => crouchTransitionSpeed = value;
    }

    public float CrouchCheckOffset
    {
        get => crouchCheckOffset;
        set => crouchCheckOffset = value;
    }

    // State queries
    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching => _isCrouching;
    public bool IsNoclipping => _isNoclipping;

    private Vector3 _moveDirection;
    private Vector2 _currentLook;
    private Vector2 _currentLookVelocity;
    private float _currentSpeed;
    private float _rotationX;

    private float _currentCharacterHeight;
    private float _heightVelocity;

    private float _crouchCheckDistance;

    private Vector3 _forceRotateTarget = Vector3.zero;
    private bool _isForceRotating = false;

    private bool _isCrouching = false;
    private bool _isNoclipping = false;

    // Cached references
    private Player _player;
    private CharacterController _characterController;
    private Transform _cameraTransform;

    private void Start()
    {
        // Cache references
        _player = Core.Player;
        _characterController = _player.CharacterController;
        _cameraTransform = _player.CameraMain.transform;

        Core.GameManager.UpdateCursorVisiblity(true);

        _currentCharacterHeight = _characterController.height;
        standingHeight = _currentCharacterHeight;

        _currentSpeed = walkSpeed;
        _crouchCheckDistance = standingHeight - crouchHeight + crouchCheckOffset;
    }

    private void Update()
    {
        GameManager gameManager = Core.GameManager;
        if (gameManager && gameManager.gamePaused) return;

        PlayerInputs inputs = _player.Inputs;
        bool canMove = !gameManager.disablePlayerInputs;

        IsMoving = inputs.MoveInput.sqrMagnitude > 0.01f;
        _isCrouching = inputs.CrouchHeld;
        IsSprinting = _player.Sprint.CanSprint() && inputs.SprintHeld && !_isCrouching;

        UpdatePlayerState(IsMoving, IsSprinting);
        _player.Sprint.SetCurrentState(IsSprinting, IsMoving, _isCrouching);

        if (canMove && !_isForceRotating)
        {
            if (!_isNoclipping)
            {
                HandleCrouch();
                HandleMove();
            }
            else
            {
                HandleNoclipMove();
            }
            HandleLook();
        }
        else if (_isForceRotating)
        {
            HandleForcedRotate();
        }
    }

    private void HandleMove()
    {
        var inputs = _player.Inputs;
        Vector2 moveInput = inputs.MoveInput;
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        float targetSpeed = DetermineCurrentSpeed();
        UpdateCurrentSpeed(targetSpeed);

        Vector3 horizontal = transform.TransformDirection(moveDirection) * _currentSpeed;

        if (_characterController.isGrounded && _moveDirection.y < 0)
        {
            _moveDirection.y = -2f;
        }
        _moveDirection.y -= gravity * Time.deltaTime;

        Vector3 finalMove = horizontal;
        finalMove.y = _moveDirection.y;

        _characterController.Move(finalMove * Time.deltaTime);
    }

    private void HandleNoclipMove()
    {
        var inputs = _player.Inputs;
        Vector2 moveInput = inputs.MoveInput;
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;
        Vector3 moveDirection = (forward * moveInput.y + right * moveInput.x).normalized;

        float verticalInput = 0f;
        //if (Input.GetKey(KeyCode.Space)) verticalInput = 1f;
        if (inputs.CrouchHeld) verticalInput = -1f;

        moveDirection += Vector3.up * verticalInput;

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        float speed = noclipSpeed;
        if (inputs.SprintHeld)
        {
            speed *= noclipSprintMultiplier;
        }

        Vector3 movement = moveDirection * speed * Time.deltaTime;
        transform.position += movement;

        IsMoving = moveInput.sqrMagnitude > 0.01f || verticalInput != 0f;
    }

    private void HandleLook()
    {
        var inputs = _player.Inputs;
        Vector2 lookInput = inputs.LookInput * lookSpeed;
        if (!invertYAxis) lookInput.y = -lookInput.y;

        Vector2 processedLook;
        if (smoothLook)
        {
            processedLook = Vector2.SmoothDamp(_currentLook, lookInput, ref _currentLookVelocity, lookSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            _currentLook = processedLook;
        }
        else
        {
            processedLook = lookInput;
        }

        _rotationX = Mathf.Clamp(_rotationX + processedLook.y, minLookX, maxLookX);
        transform.Rotate(Vector3.up * processedLook.x);

        _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
    }

    private void HandleCrouch()
    {
        var inputs = _player.Inputs;

        if (inputs.CrouchHeld)
        {
            _isCrouching = true;
        }
        else if (_isCrouching)
        {
            if (!Physics.Raycast(transform.position, Vector3.up, _crouchCheckDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                _isCrouching = false;
            }
        }

        float targetHeight = _isCrouching ? crouchHeight : standingHeight;

        if (Mathf.Abs(_characterController.height - targetHeight) > 0.01f)
        {
            AdjustController(targetHeight);
        }
    }

    private void AdjustController(float targetHeight)
    {
        float previousHeight = _characterController.height;

        // Smooth the height transition
        _currentCharacterHeight = Mathf.SmoothDamp(
            _currentCharacterHeight,
            targetHeight,
            ref _heightVelocity,
            1f / crouchTransitionSpeed
        );

        _characterController.height = _currentCharacterHeight;

        float heightDifference = _currentCharacterHeight - previousHeight;
        if (Mathf.Abs(heightDifference) > 0.0001f)
        {
            Vector3 wiggle = Random.onUnitSphere * 0.05f;
            _characterController.Move((Vector3.down + wiggle) * Time.deltaTime);
        }
    }

    private void HandleForcedRotate()
    {
        transform.rotation = Quaternion.Euler(_forceRotateTarget);
        _cameraTransform.localRotation = Quaternion.Euler(_forceRotateTarget);
        _rotationX = _forceRotateTarget.x;
        _isForceRotating = false;
    }

    private void UpdateCurrentSpeed(float targetSpeed)
    {
        bool isAccelerating = targetSpeed > _currentSpeed;
        bool isDecelerating = targetSpeed < _currentSpeed;

        if (isDecelerating && targetSpeed > 0)
        {
            _currentSpeed = Mathf.Max(_currentSpeed - decelerationKick * Time.deltaTime * 60f, targetSpeed);
        }

        float curveSpeed = isAccelerating ? accelerationCurve : decelerationCurve;
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * curveSpeed);
    }

    private void UpdatePlayerState(bool isMoving, bool isSprinting)
    {
        if (_isNoclipping)
        {
            _player.CurrentState = PlayerState.Noclip;
            return;
        }

        if (!_characterController.isGrounded && _moveDirection.y < -2f)
        {
            _player.CurrentState = PlayerState.Freefall;
        }

        if (!isMoving)
        {
            _player.CurrentState = PlayerState.Idle;
        }
        else if (_isCrouching)
        {
            _player.CurrentState = PlayerState.Crouching;
        }
        else if (isSprinting)
        {
            _player.CurrentState = PlayerState.Sprinting;
        }
        else
        {
            _player.CurrentState = PlayerState.Walking;
        }
    }

    public void ForceStandUp()
    {
        if (!Physics.Raycast(transform.position, Vector3.up, _crouchCheckDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            _isCrouching = false;
            _currentCharacterHeight = standingHeight;
            _characterController.height = standingHeight;
            Vector3 currentCenter = _characterController.center;
            _characterController.center = new Vector3(currentCenter.x, 0f, currentCenter.z);
        }
    }

    public void ResetLookRotation()
    {
        _rotationX = 0f;
        _currentLook = Vector2.zero;
        _currentLookVelocity = Vector2.zero;
        _cameraTransform.localRotation = Quaternion.identity;
    }

    public float CrouchState
    {
        get
        {
            if (standingHeight == crouchHeight) return 0f;
            return Mathf.Clamp01((standingHeight - _currentCharacterHeight) / (standingHeight - crouchHeight));
        }
    }

    public void ForceRotate(Vector3 targetRotation)
    {
        _forceRotateTarget = targetRotation;
        _isForceRotating = true;
    }

    public void ResetMoveDirection()
    {
        _moveDirection = Vector3.zero;
        _currentSpeed = walkSpeed;
    }

    public float DetermineCurrentSpeed()
    {
        var inputs = _player.Inputs;
        if (inputs.CrouchHeld) return crouchSpeed;
        if (_player.Sprint.CanSprint() && inputs.SprintHeld && !_isCrouching) return sprintSpeed;
        return walkSpeed;
    }

    public void ToggleNoclip()
    {
        _isNoclipping = !_isNoclipping;

        if (_isNoclipping)
        {
            EnableNoclip();
        }
        else
        {
            DisableNoclip();
        }
    }

    private void EnableNoclip()
    {
        _characterController.enabled = false;
        _moveDirection = Vector3.zero;

        _isCrouching = false;
        _currentCharacterHeight = standingHeight;
    }

    private void DisableNoclip()
    {
        _characterController.enabled = true;
        _characterController.height = standingHeight;

        _moveDirection = Vector3.zero;
    }
}