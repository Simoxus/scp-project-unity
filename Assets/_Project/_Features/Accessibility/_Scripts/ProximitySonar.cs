using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatic proximity sonar for blind / low-vision players: emits a spatialized beep from the
/// nearest interactable, parking-sensor style (faster = closer). Doors and items use distinct timbres,
/// and a double beep signals the target is within interaction range. Audio goes through FMOD Core
/// (the project disables Unity's audio engine).
/// Maps to gameaccessibilityguidelines.com (Vision): "Provide a pingable sonar-style audio map".
/// </summary>
public class ProximitySonar : MonoBehaviour
{
    [Header("Beep Timing")]
    [SerializeField] private float minBeepInterval = 0.13f; // beep cadence when nearly touching the target
    [SerializeField] private float maxBeepInterval = 1.1f;  // cadence at the edge of the sonar radius
    [SerializeField] private float closeRange = 3f;         // double-beep inside this range (matches PlayerInteract's reach)
    [SerializeField] private float doubleBeepGap = 0.09f;

    [Header("Timbres (fallback tones when the CC0 files are missing)")]
    [SerializeField] private float doorBeepFrequency = 520f;
    [SerializeField] private float doorBeepDuration = 0.09f;
    [SerializeField] private float itemBeepFrequency = 1175f;
    [SerializeField] private float itemBeepDuration = 0.05f;

    [Header("Occlusion")]
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float occlusionSkin = 0.15f; // tolerance so the wall right behind a button doesn't count as blocking

    private const float PlayerSearchInterval = 2f;
    private const float IdleRescanInterval = 0.3f;

    private Player _player;
    private A11yFmodAudio.A11ySound _doorBeep;
    private A11yFmodAudio.A11ySound _itemBeep;
    private readonly Collider[] _hits = new Collider[32];
    private int _interactableLayerMask;
    private int _occlusionMask;
    private float _nextBeepTime;
    private float _nextPlayerSearchTime;
    private Collider _announcedTarget;
    private System.Func<Collider, bool> _lineOfSightPredicate;

    private void Awake()
    {
        _interactableLayerMask = LayerMask.GetMask("Interactable");
        if (_interactableLayerMask == 0)
        {
            Debug.LogWarning("[Accessibility] 'Interactable' layer not found; sonar will scan all layers.", this);
            _interactableLayerMask = ~0;
        }

        // Walls and level geometry block the sonar; these layers never do
        _occlusionMask = ~LayerMask.GetMask("Interactable", "Player", "Ignore Raycast", "TransparentFX", "UI", "Debris");
        _lineOfSightPredicate = HasLineOfSight;

        // Bundled CC0 files (StreamingAssets/A11y, see A11Y_AUDIO_CREDITS.md); generated tones as fallback
        _doorBeep = A11yFmodAudio.LoadOrGenerate("door_beep.ogg", doorBeepFrequency, doorBeepDuration);
        _itemBeep = A11yFmodAudio.LoadOrGenerate("item_beep.ogg", itemBeepFrequency, itemBeepDuration);
    }

    private void OnDestroy()
    {
        A11yFmodAudio.Release(ref _doorBeep);
        A11yFmodAudio.Release(ref _itemBeep);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => _player = null;

    private void Update()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;
        if (IsGamePaused()) return;

        if (_player == null)
        {
            if (Time.unscaledTime < _nextPlayerSearchTime) return;
            _nextPlayerSearchTime = Time.unscaledTime + PlayerSearchInterval;
            _player = Core.Player;
            if (_player == null) return; // no player in this scene (e.g. main menu)

            // These need to live on the player object (controller hits, health events, blink meter, camera yaw, navigation)
            if (_player.GetComponent<WallBumpFeedback>() == null)
            {
                _player.gameObject.AddComponent<WallBumpFeedback>();
            }
            if (_player.GetComponent<VitalStatusAnnouncer>() == null)
            {
                _player.gameObject.AddComponent<VitalStatusAnnouncer>();
            }
            if (_player.GetComponent<BlinkWarning>() == null)
            {
                _player.gameObject.AddComponent<BlinkWarning>();
            }
            if (_player.GetComponent<KeyboardLook>() == null)
            {
                _player.gameObject.AddComponent<KeyboardLook>();
            }
            if (_player.GetComponent<TargetNavigator>() == null)
            {
                _player.gameObject.AddComponent<TargetNavigator>();
            }
            if (_player.GetComponent<FallGuard>() == null)
            {
                _player.gameObject.AddComponent<FallGuard>();
            }
            if (_player.GetComponent<StanceAnnouncer>() == null)
            {
                _player.gameObject.AddComponent<StanceAnnouncer>();
            }
            if (_player.GetComponent<ScanPulse>() == null)
            {
                _player.gameObject.AddComponent<ScanPulse>();
            }
            if (_player.GetComponent<StatusReporter>() == null)
            {
                _player.gameObject.AddComponent<StatusReporter>();
            }
            if (_player.GetComponent<RoomTracker>() == null)
            {
                _player.gameObject.AddComponent<RoomTracker>();
            }
        }

        if (Time.time < _nextBeepTime) return;

        Vector3 origin = _player.transform.position;
        int count = Physics.OverlapSphereNonAlloc(origin, manager.sonarRadius, _hits, _interactableLayerMask);

        if (!SonarLogic.TryChooseTarget(origin, _hits, count, out SonarTargetKind kind, out Collider chosen, out Vector3 targetPosition, out float distance, _lineOfSightPredicate))
        {
            _nextBeepTime = Time.time + IdleRescanInterval;
            return;
        }

        // Continuous parking-sensor beeping is opt-in (Shift+F6); the in-reach
        // announcement stays on in both modes because it is event-like, not a drone
        if (manager.sonarContinuous)
        {
            EmitBeep(kind, targetPosition, distance, manager);
        }
        AnnounceIfNewCloseTarget(kind, chosen, distance);
        _nextBeepTime = Time.time + SonarLogic.IntervalForDistance(distance, manager.sonarRadius, minBeepInterval, maxBeepInterval);
    }

    private static bool IsGamePaused()
    {
        var gameManager = Core.GameManager;
        if (gameManager != null) return gameManager.gamePaused;
        return Time.timeScale == 0f;
    }

    // Shared with TargetNavigator so cycling sees exactly what the sonar sees
    public int ScanInteractables(Vector3 origin, float radius, Collider[] results)
    {
        return Physics.OverlapSphereNonAlloc(origin, radius, results, _interactableLayerMask);
    }

    public bool HasLineOfSight(Collider target)
    {
        if (_player == null) return false;
        Vector3 eye = _player.transform.position + Vector3.up * eyeHeight;
        Vector3 delta = target.bounds.center - eye;
        float length = delta.magnitude - occlusionSkin;
        if (length <= 0f) return true;

        return !Physics.Raycast(eye, delta.normalized, length, _occlusionMask, QueryTriggerInteraction.Ignore);
    }

    // First NVDA use case: announce a target once when it enters interaction range
    private void AnnounceIfNewCloseTarget(SonarTargetKind kind, Collider chosen, float distance)
    {
        if (distance > closeRange)
        {
            if (_announcedTarget != null && chosen != _announcedTarget) _announcedTarget = null;
            return;
        }

        if (chosen == _announcedTarget) return;
        _announcedTarget = chosen;

        string text = kind == SonarTargetKind.Door
            ? "Puerta al alcance."
            : $"Objeto al alcance: {A11yAltText.HumanizeName(chosen.name)}.";
        ScreenReaderOutput.Speak(text);
    }

    private void EmitBeep(SonarTargetKind kind, Vector3 position, float distance, AccessibilityManager manager)
    {
        var sound = kind == SonarTargetKind.Door ? _doorBeep : _itemBeep;
        A11yFmodAudio.PlayAt(sound, position, manager.sonarVolume);

        // Double beep = "you are close enough to interact"
        if (distance <= closeRange)
        {
            StartCoroutine(PlaySecondBeep(sound, position, manager.sonarVolume));
        }
    }

    private IEnumerator PlaySecondBeep(A11yFmodAudio.A11ySound sound, Vector3 position, float volume)
    {
        yield return new WaitForSeconds(doubleBeepGap);
        A11yFmodAudio.PlayAt(sound, position, volume);
    }
}
