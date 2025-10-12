using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;

    [Header("Move Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = 22f;

    [Header("Look Settings")]
    [SerializeField] private bool doLookInvert = false;
    [SerializeField] private bool doSmoothLook = false;
    [SerializeField] private float lookSmoothTime = 0.1f;
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float minLookX = -75f;
    [SerializeField] private float maxLookX = 75f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 8f;
    [SerializeField] private float crouchCheckOffset = 0.1f; // For ray

    [Header("Footstep Settings")]
    [SerializeField] private float walkStepTimeInterval = 0.4f;
    [SerializeField] private float sprintStepTimeInterval = 0.3f;
    [SerializeField] private float crouchStepTimeInterval = 0.5f;

    [Header("Multiplier Settings")]
    [SerializeField] private float walkBobMultiplier = 1f;
    [SerializeField] private float walkTiltMultiplier = 1.5f;

    [SerializeField] private float sprintBobMultiplier = 1f;
    [SerializeField] private float sprintTiltMultiplier = 1.5f;

    [SerializeField] private float crouchBobMultiplier = 1f;
    [SerializeField] private float crouchTiltMultiplier = 1.5f;

    [Header("State Settings")]
    public bool isMoving = false;
    public bool isSprinting = false;
    public bool isBlinking = false;
    public bool isCrouching = false;

    private Vector3 _moveDirection;
    private Vector2 _currentLook;
    private Vector2 _currentLookVelocity;
    private float _rotationX;

    private float _currentCharacterHeight;
    private float _heightVelocity;

    private float _footstepTimer;
    private float _currentStepTimeInterval;
    private Vector3 _lastFootstepPosition; // To track distance moved

    private Vector3 _forceMoveTarget = Vector3.zero;
    private Vector3 _forceRotateTarget = Vector3.zero;
    private bool _isForceRotating = false;

    private void Awake()
    {
        // Check for player and if there's no player, try to find the singleton/instance
        player = player != null ? player : Player.Instance;

        GameManager.Instance.UpdateCursorVisiblity();
    }

    private void Start()
    {
        //GameManager.Instance.UpdateCursorVisiblity();
        _currentCharacterHeight = player.characterController.height;
        standingHeight = _currentCharacterHeight;

        _lastFootstepPosition = transform.position; // Initialize last footstep position
        _footstepTimer = 0f; // Initialize the footstep timer
    }

    private void Update()
    {
        if (GameManager.Instance.gamePaused) return;

        bool canMove = !GameManager.Instance.disablePlayerInputs;

        if (canMove && !_isForceRotating)
        {
            HandleCrouch();
            HandleMove();
            HandleLook();
            HandleFootsteps();
            SetHeadbobMultipliers();
        }
        else if (_isForceRotating)
        {
            HandleForcedRotate();
        }

        // Get states from the components
        bool isMoving = player.playerInputs.moveInput.sqrMagnitude > 0.01f;
        bool isCrouching = player.playerInputs.crouchHeld;
        bool isSprinting = player.playerStats.currentSprint > 0 && player.playerInputs.sprintHeld;

        // Pass the state to the PlayerStats component for passive logic
        player.playerStats.SetCurrentState(isSprinting, isMoving, isCrouching);

        // Pass the data to UIIndicators for visualization
        PlayerState playerState = PlayerState.Walking;
        if (isSprinting)
        {
            playerState = PlayerState.Sprinting;
        }
        else if (isCrouching)
        {
            playerState = PlayerState.Crouching;
        }

        player.uiIndicators.UpdateIndicators(
            currentSprint: player.playerStats.currentSprint,
            maxSprint: 1f,
            playerState: playerState
        );

        SetHeadbobMultipliers();
    }

    private void HandleMove()
    {
        Vector2 moveInput = player.playerInputs.moveInput;
        Vector3 horizontal = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)) * DetermineCurrentSpeed();

        if (player.characterController.isGrounded && _moveDirection.y < 0)
        {
            _moveDirection.y = -2f;
        }
        _moveDirection.y -= gravity * Time.deltaTime;

        Vector3 finalMove = horizontal;
        finalMove.y = _moveDirection.y;

        player.characterController.Move(finalMove * Time.deltaTime);

        isMoving = moveInput.sqrMagnitude > 0.01f;
    }

    private void HandleLook()
    {
        Vector2 lookInput = player.playerInputs.lookInput * lookSpeed;
        if (doLookInvert) lookInput.y = -lookInput.y;

        Vector2 processedLook;
        if (doSmoothLook)
        {
            processedLook = Vector2.SmoothDamp(_currentLook, lookInput, ref _currentLookVelocity, lookSmoothTime);
            _currentLook = processedLook;
        }
        else
        {
            processedLook = lookInput;
        }

        _rotationX = Mathf.Clamp(_rotationX + processedLook.y, minLookX, maxLookX);

        transform.Rotate(Vector3.up * processedLook.x);

        //Vector3 currentEuler = player.cameraMain.transform.localEulerAngles;
        //player.cameraMain.transform.localEulerAngles = new Vector3(_rotationX, 0f, currentEuler.z);

        player.cameraMain.transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
    }

    private void HandleCrouch()
    {
        // Toggle crouch on input press
        if (player.playerInputs.crouchHeld)
        {
            isCrouching = true;
            isSprinting = false;
        }
        else
        {
            if (!Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight + crouchCheckOffset, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                isCrouching = false;
            }
        }

        float previousHeight = _currentCharacterHeight;

        // Determine target height
        float targetHeight = isCrouching ? crouchHeight : standingHeight;

        // Smooth height transition
        _currentCharacterHeight = Mathf.SmoothDamp(
            _currentCharacterHeight,
            targetHeight,
            ref _heightVelocity,
            1f / crouchTransitionSpeed
        );
        player.characterController.height = _currentCharacterHeight;

        // Adjust player position based on height change (for center (0,0,0))
        float heightDifference = _currentCharacterHeight - previousHeight;
        if (Mathf.Abs(heightDifference) > 0.001f)
        {
            player.characterController.transform.position += Vector3.up * (heightDifference / 2f);
        }
    }

    private void HandleFootsteps()
    {
        // Only process footsteps if moving and grounded
        if (isMoving && player.characterController.isGrounded)
        {
            // Determine the appropriate step time interval based on current speed
            if (isSprinting)
            {
                _currentStepTimeInterval = sprintStepTimeInterval;
            }
            else if (isCrouching)
            {
                _currentStepTimeInterval = crouchStepTimeInterval;
            }
            else // Walking
            {
                _currentStepTimeInterval = walkStepTimeInterval;
            }

            // Decrement the timer
            _footstepTimer -= Time.deltaTime;

            // Play footstep if timer runs out
            if (_footstepTimer <= 0f)
            {
                if (player.playerFootsteps != null)
                {
                    player.playerFootsteps.RequestFootstep();
                }
                // Reset the timer
                _footstepTimer = _currentStepTimeInterval;
            }
        }
        else
        {
            // Reset timer and position if not moving or not grounded
            _footstepTimer = 0f; // Or _currentStepTimeInterval to be ready for next movement
            _lastFootstepPosition = transform.position;
        }
    }

    private void HandleForcedRotate()
    {
        // Immediately set the rotation.
        transform.rotation = Quaternion.Euler(_forceRotateTarget);
        player.cameraMain.transform.localRotation = Quaternion.Euler(_forceRotateTarget);
        _isForceRotating = false;
    }

    private void HandleDebugKeys()
    {
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            doSmoothLook = !doSmoothLook;
            Debug.Log($"SmoothLook is now: {doSmoothLook}");
        }
    }

    private void SetHeadbobMultipliers()
    {
        if (player.playerBobbing == null) return;

        float targetBobMultiplier = walkBobMultiplier;
        float targetTiltMultiplier = walkTiltMultiplier;

        if (isSprinting)
        {
            targetBobMultiplier = sprintBobMultiplier;
            targetTiltMultiplier = sprintTiltMultiplier;
        }
        else if (isCrouching)
        {
            targetBobMultiplier = crouchBobMultiplier;
            targetTiltMultiplier = crouchTiltMultiplier;
        }

        player.playerBobbing.bobMultiplier = targetBobMultiplier;
        player.playerBobbing.tiltMultiplier = targetTiltMultiplier;
    }

    public void ForceStandUp()
    {
        if (!Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight + crouchCheckOffset, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            isCrouching = false;
            isSprinting = false; // Cannot be sprinting if standing up
            _currentCharacterHeight = standingHeight; // Immediately set height
            player.characterController.height = standingHeight;
            Vector3 currentCenter = player.characterController.center;
            player.characterController.center = new Vector3(currentCenter.x, 0f, currentCenter.z);
        }
    }

    public void ForceRotate(Vector3 targetRotation)
    {
        _forceRotateTarget = targetRotation;
        _isForceRotating = true;
    }

    public float DetermineCurrentSpeed()
    {
        //if (player.playerStats.isCrouching) return crouchSpeed;
        //if (player.playerStats.isSprinting) return sprintSpeed;
        if (player.playerInputs.crouchHeld) { return crouchSpeed; }
        if (player.playerInputs.sprintHeld) { return sprintSpeed; }
        return walkSpeed;
    }
}