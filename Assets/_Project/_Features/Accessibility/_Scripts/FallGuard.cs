using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Safety net for blind navigation, born from QA: the player walked through a door at the
/// edge of the test map, fell into the void and got total silence — no footsteps, no sonar,
/// no announcement. This component: announces sustained falls, auto-rescues after falling
/// far below the last safe ground, warns when movement is blocked for a while, and lets the
/// player return to safe ground on demand (R key / gamepad dpad up).
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab edits.
/// Maps to gameaccessibilityguidelines.com (Vision): "Ensure no essential information is
/// conveyed by visuals alone"; (Motor): "Include assist modes".
/// </summary>
public class FallGuard : MonoBehaviour
{
    [SerializeField] private float fallAnnounceAfter = 1.5f; // seconds airborne before speaking up
    [SerializeField] private float voidDropDistance = 20f;   // fallen this far below safe ground = out of the map
    [SerializeField] private float safeSaveInterval = 2f;
    [SerializeField] private float stuckAfter = 2.5f;        // pushing forward this long without moving = blocked

    private CharacterController _controller;
    private Player _player;

    private Vector3 _safeRecent;
    private Vector3 _safePrevious; // rescue target: one save older, so it sits further from the cliff edge
    private bool _hasSafe;
    private float _nextSafeSave;

    private float _airborneSince = -1f;
    private bool _announcedFalling;

    private Vector3 _stuckAnchor;
    private float _stuckSince = -1f;
    private bool _announcedStuck;
    private float _nextLedgeWarnTime;

    private InputAction _rescueAction;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _player = GetComponent<Player>();

        _rescueAction = new InputAction("A11yRescue", binding: "<Keyboard>/r");
        _rescueAction.AddBinding("<Gamepad>/dpad/up");
        _rescueAction.performed += _ => Rescue();
    }

    private void OnEnable() => _rescueAction.Enable();
    private void OnDisable() => _rescueAction.Disable();
    private void OnDestroy() => _rescueAction.Dispose();

    private static bool IsGamePaused()
    {
        var gameManager = Core.GameManager;
        if (gameManager != null) return gameManager.gamePaused;
        return Time.timeScale == 0f;
    }

    private void Update()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled || _controller == null) return;
        if (IsGamePaused()) return;

        if (_controller.isGrounded)
        {
            _airborneSince = -1f;
            _announcedFalling = false;
            SaveSafeGround();
            CheckStuck();
            CheckLedgeAhead();
        }
        else
        {
            CheckFalling();
        }
    }

    // Preventive ledge warning (TLOU's Ledge Guard, warning flavor): probe the ground
    // just ahead of the movement direction; a missing floor means a drop is coming.
    private void CheckLedgeAhead()
    {
        if (Time.time < _nextLedgeWarnTime) return;

        Vector2 moveInput = _player != null && _player.Inputs != null ? _player.Inputs.MoveInput : Vector2.zero;
        if (moveInput.sqrMagnitude < 0.04f) return;

        Vector3 moveDirection = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        Vector3 probeOrigin = transform.position + moveDirection * 0.8f + Vector3.up * 0.3f;
        if (!Physics.Raycast(probeOrigin, Vector3.down, 2.3f, ~LayerMask.GetMask("Interactable", "Player", "Ignore Raycast", "TransparentFX", "UI", "Debris"), QueryTriggerInteraction.Ignore))
        {
            _nextLedgeWarnTime = Time.time + 2.5f;
            ScreenReaderOutput.Speak("Borde adelante.", true);
        }
    }

    private void SaveSafeGround()
    {
        if (Time.time < _nextSafeSave) return;
        _nextSafeSave = Time.time + safeSaveInterval;
        _safePrevious = _hasSafe ? _safeRecent : transform.position;
        _safeRecent = transform.position;
        _hasSafe = true;
    }

    private void CheckFalling()
    {
        if (_airborneSince < 0f)
        {
            _airborneSince = Time.time;
        }
        else if (!_announcedFalling && Time.time - _airborneSince > fallAnnounceAfter)
        {
            _announcedFalling = true;
            ScreenReaderOutput.Speak("Estás cayendo.", true);
        }

        if (_hasSafe && transform.position.y < Mathf.Min(_safeRecent.y, _safePrevious.y) - voidDropDistance)
        {
            ScreenReaderOutput.Speak("Caíste fuera del mapa.", true);
            Rescue();
        }
    }

    private void CheckStuck()
    {
        Vector2 moveInput = _player != null && _player.Inputs != null ? _player.Inputs.MoveInput : Vector2.zero;
        if (moveInput.sqrMagnitude < 0.04f)
        {
            _stuckSince = -1f;
            _announcedStuck = false;
            return;
        }

        if (_stuckSince < 0f || (transform.position - _stuckAnchor).sqrMagnitude >= 0.04f)
        {
            _stuckSince = Time.time;
            _stuckAnchor = transform.position;
            _announcedStuck = false;
            return;
        }

        if (!_announcedStuck && Time.time - _stuckSince > stuckAfter)
        {
            _announcedStuck = true;
            ScreenReaderOutput.Speak("Movimiento bloqueado. R vuelve a suelo seguro.");
        }
    }

    private void Rescue()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;

        if (!_hasSafe)
        {
            ScreenReaderOutput.Speak("Todavía no hay un punto seguro registrado.", true);
            return;
        }

        _controller.enabled = false;
        transform.position = _safePrevious + Vector3.up * 0.3f;
        _controller.enabled = true;

        _airborneSince = -1f;
        _announcedFalling = false;
        _stuckSince = -1f;
        _announcedStuck = false;

        ScreenReaderOutput.Speak("De vuelta en suelo seguro.", true);
    }
}
