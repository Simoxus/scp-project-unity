using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerFreecam : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAccess player;
    [SerializeField] private CinemachineCamera cameraFree;

    [Header("Movement Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float sprintSpeed = 30f;

    [Header("Look Settings")]
    [SerializeField] private bool doLookInvert = false;
    [SerializeField] private bool doSmoothLook = false;
    [SerializeField] private float lookSmoothTime = 0.1f;
    [SerializeField] private float lookSpeed = 1.2f;
    [SerializeField] private float minLookX = -90f;
    [SerializeField] private float maxLookX = 90f;

    private Vector2 _currentLook;
    private Vector2 _currentLookVelocity;
    private float _rotationX;

    private void Update()
    {
        if (GameManager.Instance.disablePlayerInputs) return;

        HandleMovement();
        HandleLook();
        HandleSmoothLookToggle();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = player.playerInputs.moveInput;
        float currentSpeed = speed;

        if (player.playerInputs.sprintHeld)
        {
            currentSpeed = sprintSpeed;
        }
        else if (player.playerInputs.crouchHeld) // Check for crouch
        {
            currentSpeed = crouchSpeed;
        }

        // Get camera's forward and right vectors
        Vector3 cameraForward = cameraFree.transform.forward;
        Vector3 cameraRight = cameraFree.transform.right;

        // Flatten the vectors to the horizontal plane
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate horizontal movement relative to camera
        Vector3 horizontalMove = cameraForward * moveInput.y + cameraRight * moveInput.x;

        // Handle vertical movement independently
        Vector3 verticalMove = Vector3.zero;
        if (Keyboard.current.qKey.isPressed)
        {
            verticalMove = Vector3.down;
        }
        if (Keyboard.current.eKey.isPressed)
        {
            verticalMove = Vector3.up;
        }

        // Combine horizontal and vertical movement
        Vector3 finalMoveDirection = horizontalMove + verticalMove;

        transform.position += finalMoveDirection.normalized * currentSpeed * Time.deltaTime;
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
        cameraFree.transform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
    }

    private void HandleSmoothLookToggle()
    {
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            doSmoothLook = !doSmoothLook;
            Debug.Log($"SmoothLook is now: {doSmoothLook} (Freecam)");
        }
    }
}