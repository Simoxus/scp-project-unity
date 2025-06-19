using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PrimeTween;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputs))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public CharacterController characterController;
    [SerializeField] private PlayerInputs playerInputs;
    [SerializeField] private PlayerHealth playerDamage;
    [SerializeField] private PlayerBobbing playerBobbing;
    [SerializeField] private PlayerFootsteps playerFootsteps;

    [Header("Move Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = 22f;

    [Header("Look Settings")]
    public Camera cameraBrain;
    public CinemachineCamera cameraMain;
    [SerializeField] private bool doLookInvert = false;
    [SerializeField] private bool useSmoothLook = false;
    [SerializeField] private float lookSmoothTime = 0.1f;
    [SerializeField] private float lookSpeed = 2f;
    [SerializeField] private float minLookX = -75f;
    [SerializeField] private float maxLookX = 75f;

    [Header("Misc Settings")]
    public bool isMoving = false;
    public bool isSprinting = false;
    public bool isBlinking = false;
    public bool isCrouching = false;

    private Vector3 _moveDirection;
    private Vector2 _currentLook;
    private Vector2 _currentLookVelocity;
    private float _rotationX;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerFootsteps.Initialize(characterController);
        GameManager.Instance.UpdateCursorVisiblity();
    }

    private void Update()
    {
        if (GameManager.Instance.disablePlayerInputs) return;

        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            useSmoothLook = !useSmoothLook;
            Debug.Log($"SmoothLook is now: {useSmoothLook}");
        } 

        HandleMove();
        HandleLook();
        TestHUD();
        TestBlinking();
        playerFootsteps.UpdateFootsteps(isMoving, isSprinting);
    }

    private void HandleMove() // Handles "characterController" variable
    {
        Vector2 moveInput = playerInputs.moveInput;
        Vector3 horizontal = transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)) * DetermineCurrentSpeed();

        if (characterController.isGrounded && _moveDirection.y < 0)
        {
            _moveDirection.y = -2f;
        }
        _moveDirection.y -= gravity * Time.deltaTime;

        Vector3 finalMove = horizontal;
        finalMove.y = _moveDirection.y;

        characterController.Move(finalMove * Time.deltaTime);

        isMoving = moveInput.sqrMagnitude > 0.01f;
    }

    private void HandleLook() // Handles "cameraRoot" and "cameraMain" variables
    {
        Vector2 lookInput = playerInputs.lookInput * lookSpeed;
        if (doLookInvert) lookInput.y = -lookInput.y;

        Vector2 processedLook;
        if (useSmoothLook)
        {
            processedLook = Vector2.SmoothDamp(_currentLook, lookInput, ref _currentLookVelocity, lookSmoothTime);
            _currentLook = processedLook;
        }
        else
        {
            processedLook = lookInput;
        }

        _rotationX = Mathf.Clamp(_rotationX + processedLook.y, minLookX, maxLookX);

        // Apply yaw (turn body left/right)
        transform.Rotate(Vector3.up * processedLook.x);

        // Apply pitch (look up/down)
        Vector3 currentEuler = cameraMain.transform.localEulerAngles;
        cameraMain.transform.localEulerAngles = new Vector3(_rotationX, 0f, currentEuler.z);
    }

    private void TestHUD()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("Toggling HUD!");
            DisplayManager.Instance.TogglePlayerHUD();
        }
    }

    private void TestBlinking()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Making the player blink!");
            DisplayManager.Instance.MakePlayerBlink();
        }
    }

    private float DetermineCurrentSpeed() // Calculate function for finding current velocity of the player
    {
        if (isCrouching)
            return crouchSpeed;

        if (isSprinting)
            return sprintSpeed;

        return walkSpeed;
    }
}
