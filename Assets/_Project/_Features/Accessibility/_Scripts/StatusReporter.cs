using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// On-demand status narration (TLOU's touchpad-swipe equivalent): press X to hear
/// health, stance, compass heading and the nearest reachable target, in that order —
/// e.g. "Sano. De pie. Mirando al norte. Puerta, 4 metros, adelante."
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab edits.
/// Maps to gameaccessibilityguidelines.com (Vision): "Ensure no essential information
/// is conveyed by visuals alone".
/// </summary>
public class StatusReporter : MonoBehaviour
{
    private PlayerHealth _health;
    private Player _player;
    private ProximitySonar _sonar;
    private readonly Collider[] _hits = new Collider[32];
    private string _healthText = "Sano";
    private InputAction _statusAction;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
        _player = GetComponent<Player>();

        _statusAction = new InputAction("A11yStatus", binding: "<Keyboard>/h"); // H for health, which reads first (QA preference)
        _statusAction.performed += _ => SpeakStatus();
    }

    private void OnEnable()
    {
        _statusAction.Enable();
        if (_health != null) _health.OnHealthLevelChanged += OnHealthLevelChanged;
    }

    private void OnDisable()
    {
        _statusAction.Disable();
        if (_health != null) _health.OnHealthLevelChanged -= OnHealthLevelChanged;
    }

    private void OnDestroy() => _statusAction.Dispose();

    private void OnHealthLevelChanged(PlayerHealth.HealthLevel level)
    {
        // Reuse the announcer's wording, trimmed to a noun-ish phrase for status reads
        _healthText = VitalStatusAnnouncer.HealthLevelText(level).TrimEnd('.');
    }

    private void SpeakStatus()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;
        var gameManager = Core.GameManager;
        if (gameManager != null && gameManager.gamePaused) return;

        string stance = _player != null && _player.IsInState(PlayerState.Crouching) ? "Agachado" : "De pie";
        string heading = "Mirando al " + KeyboardLook.HeadingName(transform.eulerAngles.y);
        string target = DescribeNearestTarget(manager);

        ScreenReaderOutput.Speak($"{_healthText}. {stance}. {heading}. {target}", true);
    }

    private string DescribeNearestTarget(AccessibilityManager manager)
    {
        if (_sonar == null)
        {
            _sonar = manager.GetComponent<ProximitySonar>();
            if (_sonar == null) return "Sin objetivos cerca.";
        }

        int count = _sonar.ScanInteractables(transform.position, manager.sonarRadius, _hits);
        if (!SonarLogic.TryChooseTarget(transform.position, _hits, count, out SonarTargetKind kind, out Collider chosen, out _, out float distance, _sonar.HasLineOfSight))
        {
            return "Sin objetivos cerca.";
        }

        string label = kind == SonarTargetKind.Door ? "Puerta" : A11yAltText.HumanizeName(chosen.name);
        int meters = Mathf.Max(1, Mathf.RoundToInt(distance));
        string direction = TargetNavigator.RelativeDirectionName(transform.forward, chosen.transform.position - transform.position);
        return $"{label}, {meters} {(meters == 1 ? "metro" : "metros")}, {direction}.";
    }
}
