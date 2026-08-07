using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mouse-free camera control for blind players, who navigate by keyboard only:
/// comma/period = smooth turn (hold), Ctrl+comma/period = 45-degree snap turn (announced),
/// M = half turn (announced), N = speak current compass heading.
/// Yaw is applied additively to the player transform, which PlayerController tolerates
/// (its own look also uses transform.Rotate). Pitch is deliberately not mapped: without
/// mouse input the vertical view never drifts, so it stays level on its own.
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab or asset edits.
/// Maps to gameaccessibilityguidelines.com (Motor): "Ensure that all areas of the user
/// interface can be accessed using the same input method"; compass output also supports
/// (Vision): "Ensure no essential information is conveyed by visuals alone".
/// </summary>
public class KeyboardLook : MonoBehaviour
{
    [SerializeField] private float turnSpeed = 90f; // degrees per second, persisted as A11y_TurnSpeed
    [SerializeField] private float snapAngle = 45f;

    private const string TurnSpeedKey = "A11y_TurnSpeed";

    private InputAction _turnLeft;
    private InputAction _turnRight;
    private InputAction _snapLeft;
    private InputAction _snapRight;
    private InputAction _halfTurn;
    private InputAction _compass;

    private void Awake()
    {
        turnSpeed = PlayerPrefs.GetFloat(TurnSpeedKey, turnSpeed);

        _turnLeft = new InputAction("A11yTurnLeft", binding: "<Keyboard>/comma");
        _turnRight = new InputAction("A11yTurnRight", binding: "<Keyboard>/period");

        // Shift as modifier, NOT Ctrl: this game binds Crouch to leftCtrl, and QA found that
        // Ctrl+turn was silently crouching the player (which also blocks sprinting).
        // Shift is Sprint (hold), which is harmless on a stationary snap turn.
        _snapLeft = new InputAction("A11ySnapLeft");
        _snapLeft.AddCompositeBinding("ButtonWithOneModifier")
            .With("Modifier", "<Keyboard>/leftShift")
            .With("Button", "<Keyboard>/comma");

        _snapRight = new InputAction("A11ySnapRight");
        _snapRight.AddCompositeBinding("ButtonWithOneModifier")
            .With("Modifier", "<Keyboard>/leftShift")
            .With("Button", "<Keyboard>/period");

        _halfTurn = new InputAction("A11yHalfTurn", binding: "<Keyboard>/m");
        _compass = new InputAction("A11yCompass", binding: "<Keyboard>/n");
        _compass.AddBinding("<Gamepad>/rightStickPress"); // R3 speaks the heading on pads

        _snapLeft.performed += _ => SnapTurn(-snapAngle);
        _snapRight.performed += _ => SnapTurn(snapAngle);
        _halfTurn.performed += _ => SnapTurn(180f);
        _compass.performed += _ => AnnounceHeading("Mirando al ");
    }

    private void OnEnable()
    {
        _turnLeft.Enable();
        _turnRight.Enable();
        _snapLeft.Enable();
        _snapRight.Enable();
        _halfTurn.Enable();
        _compass.Enable();
    }

    private void OnDisable()
    {
        _turnLeft.Disable();
        _turnRight.Disable();
        _snapLeft.Disable();
        _snapRight.Disable();
        _halfTurn.Disable();
        _compass.Disable();
    }

    private void OnDestroy()
    {
        _turnLeft.Dispose();
        _turnRight.Dispose();
        _snapLeft.Dispose();
        _snapRight.Dispose();
        _halfTurn.Dispose();
        _compass.Dispose();
    }

    private void Update()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;

        var gameManager = Core.GameManager;
        if (gameManager != null && gameManager.gamePaused) return;

        // Shift+key is the snap gesture; don't also smooth-turn on the same press
        var keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed)) return;

        float direction = 0f;
        if (_turnLeft.IsPressed()) direction -= 1f;
        if (_turnRight.IsPressed()) direction += 1f;
        if (direction != 0f)
        {
            transform.Rotate(Vector3.up, direction * turnSpeed * Time.deltaTime);
        }
    }

    private void SnapTurn(float degrees)
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;

        var gameManager = Core.GameManager;
        if (gameManager != null && gameManager.gamePaused) return;

        transform.Rotate(Vector3.up, degrees);
        AnnounceHeading(string.Empty);
    }

    private void AnnounceHeading(string prefix)
    {
        ScreenReaderOutput.Speak(prefix + HeadingName(transform.eulerAngles.y) + ".", true);
    }

    public void SetTurnSpeed(float degreesPerSecond)
    {
        turnSpeed = Mathf.Clamp(degreesPerSecond, 30f, 240f);
        PlayerPrefs.SetFloat(TurnSpeedKey, turnSpeed);
        PlayerPrefs.Save();
    }

    /// <summary>World yaw in degrees to an 8-way Spanish compass name (0 = north = world +Z).</summary>
    public static string HeadingName(float yawDegrees)
    {
        string[] names = { "norte", "noreste", "este", "sudeste", "sur", "sudoeste", "oeste", "noroeste" };
        float normalized = Mathf.Repeat(yawDegrees, 360f);
        int sector = Mathf.RoundToInt(normalized / 45f) % 8;
        return names[sector];
    }
}
