using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// On-demand area scan, modeled on The Last of Us Part II's Enhanced Listen Mode
/// (preferred over a continuous sonar by our blind QA: "I like firing it when I
/// actually need it"). Press B (gamepad: L3) to ping every reachable interactable in
/// range, nearest first: each target's own semantic sound plays AT its position, with
/// the pitch encoding relative height (higher = above you). Ends with a spoken count.
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab edits.
/// Maps to gameaccessibilityguidelines.com (Vision): "Provide a pingable sonar-style
/// audio map" — this is the literal "pingable" half.
/// </summary>
public class ScanPulse : MonoBehaviour
{
    [SerializeField] private float pingGap = 0.22f;
    [SerializeField] private int maxTargets = 8;
    [SerializeField] private float cooldown = 1f;

    private ProximitySonar _sonar;
    private A11yFmodAudio.A11ySound _doorSound;
    private A11yFmodAudio.A11ySound _itemSound;
    private readonly Collider[] _hits = new Collider[32];
    private float _lastScanTime = -10f;
    private InputAction _scanAction;

    private void Awake()
    {
        _doorSound = A11yFmodAudio.LoadOrGenerate("door_beep.ogg", 520f, 0.09f);
        _itemSound = A11yFmodAudio.LoadOrGenerate("item_beep.ogg", 1175f, 0.05f);

        _scanAction = new InputAction("A11yScan", binding: "<Keyboard>/b");
        _scanAction.AddBinding("<Gamepad>/leftStickPress");
        _scanAction.performed += _ => TriggerScan();
    }

    private void OnEnable() => _scanAction.Enable();
    private void OnDisable() => _scanAction.Disable();

    private void OnDestroy()
    {
        _scanAction.Dispose();
        A11yFmodAudio.Release(ref _doorSound);
        A11yFmodAudio.Release(ref _itemSound);
    }

    private void TriggerScan()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;
        if (Time.timeScale == 0f) return;
        if (Time.unscaledTime - _lastScanTime < cooldown) return;
        if (_sonar == null)
        {
            _sonar = manager.GetComponent<ProximitySonar>();
            if (_sonar == null) return;
        }

        _lastScanTime = Time.unscaledTime;
        StopAllCoroutines();
        StartCoroutine(RunScan(manager));
    }

    private IEnumerator RunScan(AccessibilityManager manager)
    {
        int count = _sonar.ScanInteractables(transform.position, manager.sonarRadius, _hits);

        // Collect visible interactables, nearest first
        var targets = new System.Collections.Generic.List<Collider>();
        for (int i = 0; i < count; i++)
        {
            var hit = _hits[i];
            if (hit == null || !hit.TryGetComponent(out IInteractable _)) continue;
            if (!_sonar.HasLineOfSight(hit)) continue;
            targets.Add(hit);
        }
        Vector3 origin = transform.position;
        targets.Sort((a, b) =>
            (a.transform.position - origin).sqrMagnitude.CompareTo((b.transform.position - origin).sqrMagnitude));

        int announced = Mathf.Min(targets.Count, maxTargets);
        for (int i = 0; i < announced; i++)
        {
            var target = targets[i];
            if (target == null) continue;
            target.TryGetComponent(out IInteractable interactable);
            var sound = SonarLogic.IsDoor(interactable) ? _doorSound : _itemSound;
            float pitch = SonarLogic.PitchForHeightDelta(target.transform.position.y - transform.position.y);
            A11yFmodAudio.PlayAt(sound, target.transform.position, manager.sonarVolume, pitch);
            yield return new WaitForSeconds(pingGap);
        }

        ScreenReaderOutput.Speak(announced == 0
            ? "Sin objetivos."
            : announced == 1 ? "1 objetivo." : $"{announced} objetivos.");
    }
}
