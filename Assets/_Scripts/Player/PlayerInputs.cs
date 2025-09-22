using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputs : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset playerInputAsset;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool sprintHeld { get; private set; }
    public bool crouchHeld { get; private set; }
    public bool blinkPressed { get; private set; }
    public bool crouchPressed { get; private set; }

    public event Action OnInteractPressed;
    public event Action OnPauseInputPerformed;

    private InputActionMap _playerActionMap;
    private InputActionMap _keypadActionMap;
    private InputActionMap _uiActionMap;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _sprintAction;
    private InputAction _blinkAction;
    private InputAction _crouchAction;
    private InputAction _pauseAction;
    private InputAction _interactAction;

    public event Action<string> OnKeypadInput;
    private InputAction _keypadNumberAction;
    private InputAction _keypadEnterAction;
    private InputAction _keypadClearAction;

    private void Awake()
    {
        _playerActionMap = playerInputAsset.FindActionMap("Player");
        _keypadActionMap = playerInputAsset.FindActionMap("Keypad");
        _uiActionMap = playerInputAsset.FindActionMap("UI");

        _moveAction = _playerActionMap.FindAction("Move");
        _lookAction = _playerActionMap.FindAction("Look");
        _sprintAction = _playerActionMap.FindAction("Sprint");
        _blinkAction = _playerActionMap.FindAction("Blink");
        _crouchAction = _playerActionMap.FindAction("Crouch");
        _interactAction = _playerActionMap.FindAction("Interact");

        _keypadNumberAction = _keypadActionMap.FindAction("Number");
        _keypadEnterAction = _keypadActionMap.FindAction("Enter");
        _keypadClearAction = _keypadActionMap.FindAction("Clear");

        _pauseAction = _uiActionMap.FindAction("Pause");
    }

    private void OnEnable()
    {
        _playerActionMap.Enable();
        _keypadActionMap.Disable();
        _uiActionMap.Enable();

        _sprintAction.performed += OnSprintPerformed;
        _sprintAction.canceled += OnSprintCanceled;

        _crouchAction.performed += OnCrouchPerformed;
        _crouchAction.canceled += OnCrouchCanceled;

        _blinkAction.performed += OnBlinkPerformed;

        _interactAction.performed += OnInteractPerformed;

        _pauseAction.performed += OnPausePerformedHandler;

        // Subscribe to keypad events
        _keypadNumberAction.performed += OnKeypadNumberPerformed;
        _keypadEnterAction.performed += OnKeypadEnterPerformed;
        _keypadClearAction.performed += OnKeypadClearPerformed;

    }

    private void OnDisable()
    {
        _sprintAction.performed -= OnSprintPerformed;
        _sprintAction.canceled -= OnSprintCanceled;

        _crouchAction.performed -= OnCrouchPerformed;
        _crouchAction.canceled -= OnCrouchCanceled;

        _blinkAction.performed -= OnBlinkPerformed;

        _interactAction.performed -= OnInteractPerformed;

        _pauseAction.performed -= OnPausePerformedHandler;

        _keypadNumberAction.performed -= OnKeypadNumberPerformed;
        _keypadEnterAction.performed -= OnKeypadEnterPerformed;
        _keypadClearAction.performed -= OnKeypadClearPerformed;

        _playerActionMap.Disable();
        _uiActionMap.Disable();
    }

    private void Start()
    {
        EnableGameplayInputs();
        EnableUIAssistedInputs();
    }

    private void Update()
    {
        moveInput = _moveAction.ReadValue<Vector2>();
        lookInput = _lookAction.ReadValue<Vector2>();
    }

    public void ResetCrouchPressed()
    {
        crouchPressed = false;
    }

    public void ResetBlink()
    {
        blinkPressed = false;
    }

    public void EnableGameplayInputs() => _playerActionMap.Enable();
    public void DisableGameplayInputs() => _playerActionMap.Disable();

    public void EnableKeypadInputs() => _keypadActionMap.Enable();
    public void DisableKeypadInputs() => _keypadActionMap.Disable();

    public void EnableUIAssistedInputs() => _uiActionMap.Enable();
    public void DisableUIAssistedInputs() => _uiActionMap.Disable();

    private void OnSprintPerformed(InputAction.CallbackContext ctx) => sprintHeld = true;
    private void OnSprintCanceled(InputAction.CallbackContext ctx) => sprintHeld = false;

    private void OnCrouchPerformed(InputAction.CallbackContext ctx)
    {
        crouchHeld = true;
        crouchPressed = true;
    }
    private void OnCrouchCanceled(InputAction.CallbackContext ctx) => crouchHeld = false;

    private void OnBlinkPerformed(InputAction.CallbackContext ctx) => blinkPressed = true;

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        OnInteractPressed?.Invoke();
    }

    private void OnPausePerformedHandler(InputAction.CallbackContext ctx)
    {
        OnPauseInputPerformed?.Invoke();
    }

    private void OnKeypadNumberPerformed(InputAction.CallbackContext ctx)
    {
        OnKeypadInput?.Invoke(ctx.control.name);
    }

    private void OnKeypadEnterPerformed(InputAction.CallbackContext ctx)
    {
        OnKeypadInput?.Invoke("Enter");
    }

    private void OnKeypadClearPerformed(InputAction.CallbackContext ctx)
    {
        OnKeypadInput?.Invoke("Clear");
    }
}