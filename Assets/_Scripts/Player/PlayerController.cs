using EditorAttributes;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;

    [Header("Move Settings")]
    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float sprintSpeed = 18f;
    [SerializeField] private float crouchSpeed = 5f;
    [SerializeField] private float gravity = 140f;

    [Header("Look Settings")]
    public bool doLookInvert = false;
    public bool doSmoothLook = false;
    public float lookSmoothTime = 0.1f;
    public float lookSpeed = 2.5f;
    [SerializeField] private float minLookX = -75f;
    [SerializeField] private float maxLookX = 75f;

    [Header("Crouch Settings")]
    [SerializeField] private float standingHeight = 2.6f;
    [SerializeField] private float crouchHeight = 1.4f;
    [SerializeField] private float crouchTransitionSpeed = 9f;
    [SerializeField] private float crouchCheckOffset = 0.1f;

    [Header("State Settings")]
    [ReadOnly] public bool isMoving = false;
    [ReadOnly] public bool isSprinting = false;
    [ReadOnly] public bool isBlinking = false;
    [ReadOnly] public bool isCrouching = false;

    private bool _cameraLocked = false;

    private Vector3 _moveDirection;
    private Vector2 _currentLook;
    private Vector2 _currentLookVelocity;
    private float _rotationX;

    private float _currentCharacterHeight;
    private float _heightVelocity;

    private Vector3 _forceRotateTarget = Vector3.zero;
    private bool _isForceRotating = false;

    private void Awake()
    {
        player = player != null ? player : Player.Instance;
    }

    private void Start()
    {
        GameManager.Instance.UpdateCursorVisiblity(true);

        _currentCharacterHeight = player.characterController.height;
        standingHeight = _currentCharacterHeight;
    }

    private void Update()
    {
        if (GameManager.Instance && GameManager.Instance.gamePaused) return;

        bool canMove = !GameManager.Instance.disablePlayerInputs;
        isMoving = player.playerInputs.MoveInput.sqrMagnitude > 0.01f;
        isCrouching = player.playerInputs.CrouchHeld;
        isSprinting = player.playerStats.CanSprint() &&
                  player.playerInputs.SprintHeld &&
                  !isCrouching;

        UpdatePlayerState();

        player.playerStats.SetCurrentState(isSprinting, isMoving, isCrouching);

        if (canMove && !_isForceRotating)
        {
            HandleCrouch();
            HandleMove();
            HandleLook();
        }
        else if (_isForceRotating)
        {
            HandleForcedRotate();
        }
    }

    private void HandleMove()
    {
        Vector2 moveInput = player.playerInputs.MoveInput;
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);

        if (moveDirection.sqrMagnitude > 1f)
        {
            moveDirection.Normalize();
        }

        Vector3 horizontal = transform.TransformDirection(moveDirection) * DetermineCurrentSpeed();

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
        if (Input.GetKeyDown(KeyCode.L)) _cameraLocked = !_cameraLocked;
        if (_cameraLocked) return;

        Vector2 lookInput = player.playerInputs.LookInput * lookSpeed;
        if (doLookInvert) lookInput.y = -lookInput.y;

        Vector2 processedLook;
        if (doSmoothLook)
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

        player.cameraMain.transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
    }

    private void HandleCrouch()
    {
        if (player.playerInputs.CrouchHeld)
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
        float targetHeight = isCrouching ? crouchHeight : standingHeight;

        _currentCharacterHeight = Mathf.SmoothDamp(
            _currentCharacterHeight,
            targetHeight,
            ref _heightVelocity,
            1f / crouchTransitionSpeed
        );
        player.characterController.height = _currentCharacterHeight;

        float heightDifference = _currentCharacterHeight - previousHeight;
        if (Mathf.Abs(heightDifference) > 0.001f)
        {
            player.characterController.transform.position += Vector3.up * (heightDifference / 2f);
        }
    }

    private void HandleForcedRotate()
    {
        transform.rotation = Quaternion.Euler(_forceRotateTarget);
        player.cameraMain.transform.localRotation = Quaternion.Euler(_forceRotateTarget);
        _isForceRotating = false;
    }

    private void UpdatePlayerState()
    {
        if (!isMoving)
        {
            player.currentState = PlayerState.Idle;
        }
        else if (isCrouching)
        {
            player.currentState = PlayerState.Crouching;
        }
        else if (isSprinting)
        {
            player.currentState = PlayerState.Sprinting;
        }
        else
        {
            player.currentState = PlayerState.Walking;
        }

        if (!player.characterController.isGrounded && _moveDirection.y < -2f)
        {
            player.currentState = PlayerState.Freefall;
        }
    }

    public void ForceStandUp()
    {
        if (!Physics.Raycast(transform.position, Vector3.up, standingHeight - crouchHeight + crouchCheckOffset, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            isCrouching = false;
            isSprinting = false;
            _currentCharacterHeight = standingHeight;
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
        if (player.playerInputs.CrouchHeld) return crouchSpeed;
        if (isSprinting && player.playerStats.CanSprint()) return sprintSpeed;
        return walkSpeed;
    }
}