using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
	[Header("Input Actions")]
    [SerializeField] private InputActionAsset playerInputAsset;

    public Vector2 moveInput { get; private set; }
    public Vector2 lookInput { get; private set; }
    public bool sprintHeld { get; private set; }
    public bool crouchHeld { get; private set; }
    public bool blinkPressed { get; private set; }

    private InputActionMap _playerActionMap;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _sprintAction;
    private InputAction _blinkAction;
    private InputAction _crouchAction;

    private void Awake() // Cache input actions to get better performance
    {
        _playerActionMap = playerInputAsset.FindActionMap("Player");

        _moveAction = _playerActionMap.FindAction("Move");
        _lookAction = _playerActionMap.FindAction("Look");
        _sprintAction = _playerActionMap.FindAction("Sprint");
        _blinkAction = _playerActionMap.FindAction("Crouch");
        _crouchAction = _playerActionMap.FindAction("Blink");
    }

    private void OnEnable() // Function to connect events after enabling
    {
        _playerActionMap.Enable();

        _sprintAction.performed += ctx => sprintHeld = true;
        _sprintAction.canceled += ctx => sprintHeld = false;

        _blinkAction.performed += ctx => blinkPressed = true;

        _crouchAction.performed += ctx => crouchHeld = true;
        _crouchAction.canceled += ctx => crouchHeld = false;
    }

    private void OnDisable() // Function to disconnect events before disabling
    {
        _sprintAction.performed -= ctx => sprintHeld = true;
        _sprintAction.canceled -= ctx => sprintHeld = false;

        _blinkAction.performed -= ctx => blinkPressed = true;

        _crouchAction.performed -= ctx => crouchHeld = true;
        _crouchAction.canceled -= ctx => crouchHeld = false;

        _playerActionMap.Disable();
    }

    private void Update() // Constantly read the values of the player's controls
    {
        moveInput = _moveAction.ReadValue<Vector2>();
        lookInput = _lookAction.ReadValue<Vector2>();
    }

    public void ResetBlink()
    {
        blinkPressed = false;
    }
}