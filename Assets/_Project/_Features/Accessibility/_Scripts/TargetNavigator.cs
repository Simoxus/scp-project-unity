using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// KOTOR-style object navigation for blind players (suggested by María Pía from the
/// KOTOR accessibility mod): Q cycles through reachable interactables (announcing type,
/// distance and relative direction), Shift+Q cycles backwards, Enter auto-walks to the
/// selected target and interacts with it. Any manual movement cancels the auto-walk.
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab edits.
/// Maps to gameaccessibilityguidelines.com (Vision): "Provide a pingable sonar-style
/// audio map" and (Motor): "Include assist modes such as auto-aim and assisted steering".
/// </summary>
public class TargetNavigator : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 4.5f;   // just under the game's own walk speed (5)
    [SerializeField] private float sprintSpeed = 8f;   // used while the game's sprint key (Shift) is held
    [SerializeField] private float turnSpeedWhileWalking = 240f; // deg/s facing the target
    [SerializeField] private float arriveDistance = 2.5f;        // just inside interaction range
    [SerializeField] private float stuckTimeout = 1.2f;          // no progress for this long = blocked

    private Player _player;
    private ProximitySonar _sonar;
    private CharacterController _controller;

    private readonly Collider[] _hits = new Collider[32];
    private readonly List<Collider> _targets = new List<Collider>();
    private int _cycleIndex = -1;
    private Collider _selected;

    private bool _autoWalking;
    private float _lastProgressTime;
    private float _lastDistance;

    private InputAction _cycleForward;
    private InputAction _cycleBackward;
    private InputAction _goAction;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _controller = GetComponent<CharacterController>();

        // KOTOR-style pair requested by QA: Q = previous, E = next. Both keys are free in
        // this project (Interact is not keyboard-bound; inventory uses its own key).
        _cycleForward = new InputAction("A11yCycleTarget", binding: "<Keyboard>/e");
        _cycleForward.AddBinding("<Gamepad>/dpad/right");

        _cycleBackward = new InputAction("A11yCycleTargetBack", binding: "<Keyboard>/q");
        _cycleBackward.AddBinding("<Gamepad>/dpad/left");

        _goAction = new InputAction("A11yGoToTarget", binding: "<Keyboard>/enter");
        _goAction.AddBinding("<Keyboard>/numpadEnter");
        _goAction.AddBinding("<Gamepad>/dpad/down");

        _cycleForward.performed += _ => Cycle(1);
        _cycleBackward.performed += _ => Cycle(-1);
        _goAction.performed += _ => StartAutoWalk();
    }

    private void OnEnable()
    {
        _cycleForward.Enable();
        _cycleBackward.Enable();
        _goAction.Enable();
    }

    private void OnDisable()
    {
        _cycleForward.Disable();
        _cycleBackward.Disable();
        _goAction.Disable();
        _autoWalking = false;
    }

    private void OnDestroy()
    {
        _cycleForward.Dispose();
        _cycleBackward.Dispose();
        _goAction.Dispose();
    }

    private static bool IsGamePaused()
    {
        var gameManager = Core.GameManager;
        if (gameManager != null) return gameManager.gamePaused;
        return Time.timeScale == 0f;
    }

    private bool IsActive()
    {
        var manager = AccessibilityManager.Instance;
        return manager != null && manager.sonarEnabled && !IsGamePaused();
    }

    private void Cycle(int step)
    {
        if (!IsActive()) return;

        RefreshTargets();
        if (_targets.Count == 0)
        {
            ScreenReaderOutput.Speak("No hay objetivos al alcance.", true);
            _selected = null;
            _cycleIndex = -1;
            return;
        }

        _cycleIndex = ((_cycleIndex + step) % _targets.Count + _targets.Count) % _targets.Count;
        _selected = _targets[_cycleIndex];
        AnnounceTarget(_selected, _cycleIndex + 1, _targets.Count);
    }

    private void RefreshTargets()
    {
        _targets.Clear();
        var manager = AccessibilityManager.Instance;
        if (manager == null || _sonar == null && (_sonar = GetSonar()) == null) return;

        int count = _sonar.ScanInteractables(transform.position, manager.sonarRadius, _hits);
        for (int i = 0; i < count; i++)
        {
            var hit = _hits[i];
            if (hit == null || !hit.TryGetComponent(out IInteractable _)) continue;
            if (!_sonar.HasLineOfSight(hit)) continue;
            _targets.Add(hit);
        }

        // Nearest first, so Q starts with the most relevant target
        Vector3 origin = transform.position;
        _targets.Sort((a, b) =>
            (a.transform.position - origin).sqrMagnitude.CompareTo((b.transform.position - origin).sqrMagnitude));

        if (_cycleIndex >= _targets.Count) _cycleIndex = -1;
    }

    private static ProximitySonar GetSonar()
    {
        return AccessibilityManager.Instance != null
            ? AccessibilityManager.Instance.GetComponent<ProximitySonar>()
            : null;
    }

    private void AnnounceTarget(Collider target, int position, int total)
    {
        target.TryGetComponent(out IInteractable interactable);
        string label = SonarLogic.IsDoor(interactable) ? "Puerta" : A11yAltText.HumanizeName(target.name);

        Vector3 toTarget = target.transform.position - transform.position;
        int meters = Mathf.Max(1, Mathf.RoundToInt(toTarget.magnitude));
        string direction = RelativeDirectionName(transform.forward, toTarget);

        ScreenReaderOutput.Speak($"{label}, {meters} {(meters == 1 ? "metro" : "metros")}, {direction}. {position} de {total}.", true);
    }

    private void StartAutoWalk()
    {
        if (!IsActive()) return;

        if (_selected == null)
        {
            // Enter with nothing selected: pick the nearest target first
            RefreshTargets();
            if (_targets.Count == 0)
            {
                ScreenReaderOutput.Speak("No hay objetivos al alcance.", true);
                return;
            }
            _cycleIndex = 0;
            _selected = _targets[0];
        }

        // Already within reach: act like the KOTOR default action and interact right away
        Vector3 toSelected = _selected.transform.position - transform.position;
        Vector3 flatToSelected = new Vector3(toSelected.x, 0f, toSelected.z);
        if (flatToSelected.magnitude <= arriveDistance)
        {
            FaceTarget(flatToSelected);
            if (_selected.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
            return;
        }

        _autoWalking = true;
        _lastProgressTime = Time.time;
        _lastDistance = float.MaxValue;
        ScreenReaderOutput.Speak("Caminando.", true);
    }

    private void Update()
    {
        if (!_autoWalking) return;
        if (!IsActive() || _selected == null || _controller == null)
        {
            _autoWalking = false;
            return;
        }

        // Manual movement cancels the assist, the player is always in charge
        if (_player != null && _player.Inputs != null && _player.Inputs.MoveInput.sqrMagnitude > 0.04f)
        {
            _autoWalking = false;
            ScreenReaderOutput.Speak("Caminata cancelada.", true);
            return;
        }

        Vector3 toTarget = _selected.transform.position - transform.position;
        Vector3 flat = new Vector3(toTarget.x, 0f, toTarget.z);
        float distance = flat.magnitude;

        if (distance <= arriveDistance)
        {
            _autoWalking = false;
            FaceTarget(flat);
            if (_selected.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact();
            }
            return;
        }

        // Watchdog: if we stopped making progress, something is in the way
        if (distance < _lastDistance - 0.05f)
        {
            _lastDistance = distance;
            _lastProgressTime = Time.time;
        }
        else if (Time.time - _lastProgressTime > stuckTimeout)
        {
            _autoWalking = false;
            ScreenReaderOutput.Speak("Camino bloqueado.", true);
            return;
        }

        // Turn toward the target while walking so audio and movement stay aligned
        float signedAngle = Vector3.SignedAngle(transform.forward, flat, Vector3.up);
        float maxTurn = turnSpeedWhileWalking * Time.deltaTime;
        transform.Rotate(Vector3.up, Mathf.Clamp(signedAngle, -maxTurn, maxTurn));

        // Holding the game's sprint key speeds up the assisted walk too
        bool sprinting = _player != null && _player.Inputs != null && _player.Inputs.SprintHeld;
        Vector3 step = flat.normalized * (sprinting ? sprintSpeed : walkSpeed) * Time.deltaTime;
        step.y = -0.5f * Time.deltaTime; // keep grounded on ramps
        _controller.Move(step);
    }

    private void FaceTarget(Vector3 flatDirection)
    {
        if (flatDirection.sqrMagnitude < 0.001f) return;
        float signedAngle = Vector3.SignedAngle(transform.forward, flatDirection, Vector3.up);
        transform.Rotate(Vector3.up, signedAngle);
    }

    /// <summary>Relative direction of a target in Spanish, 8 sectors around the player's facing.</summary>
    public static string RelativeDirectionName(Vector3 forward, Vector3 toTarget)
    {
        Vector3 flatForward = new Vector3(forward.x, 0f, forward.z);
        Vector3 flatTarget = new Vector3(toTarget.x, 0f, toTarget.z);
        if (flatForward.sqrMagnitude < 0.0001f || flatTarget.sqrMagnitude < 0.0001f) return "adelante";

        float angle = Vector3.SignedAngle(flatForward.normalized, flatTarget.normalized, Vector3.up); // -180..180
        float normalized = Mathf.Repeat(angle + 360f, 360f);
        string[] names = { "adelante", "adelante a la derecha", "a la derecha", "atrás a la derecha", "atrás", "atrás a la izquierda", "a la izquierda", "adelante a la izquierda" };
        int sector = Mathf.RoundToInt(normalized / 45f) % 8;
        return names[sector];
    }
}
