using UnityEngine;

/// <summary>
/// Announces vital state changes through the screen reader: health level transitions
/// (from PlayerHealth's own events) and sanity dropping below / recovering above its threshold.
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab edits.
/// Maps to gameaccessibilityguidelines.com (Vision): "Ensure no essential information is
/// conveyed by visuals alone" (health and sanity are only shown as visual bars).
/// </summary>
public class VitalStatusAnnouncer : MonoBehaviour
{
    private const float SanityPollInterval = 1f;

    private PlayerHealth _health;
    private PlayerSanity _sanity;
    private float _nextSanityPoll;
    private bool _sanityWasLow;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
        _sanity = GetComponent<PlayerSanity>();
    }

    private float _lastHealth = -1f;

    private void OnEnable()
    {
        if (_health != null)
        {
            _health.OnHealthLevelChanged += OnHealthLevelChanged;
            _health.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnHealthLevelChanged -= OnHealthLevelChanged;
            _health.OnHealthChanged -= OnHealthChanged;
        }
    }

    // Damage is a tense moment: it gets the strong rumble, scaled by the size of the hit
    private void OnHealthChanged(float current, float max)
    {
        if (_lastHealth >= 0f && current < _lastHealth && max > 0f)
        {
            var manager = AccessibilityManager.Instance;
            if (manager != null && manager.sonarEnabled)
            {
                A11yHaptics.PulseDamage(this, (_lastHealth - current) / max);
            }
        }
        _lastHealth = current;
    }

    private void OnHealthLevelChanged(PlayerHealth.HealthLevel level)
    {
        // Life-threatening states interrupt whatever NVDA was reading
        bool urgent = level == PlayerHealth.HealthLevel.NearDeath || level == PlayerHealth.HealthLevel.Dead;
        ScreenReaderOutput.Speak(HealthLevelText(level), urgent);
    }

    private void Update()
    {
        // PlayerSanity has no events yet (early upstream code), so we poll its threshold
        if (_sanity == null || Time.time < _nextSanityPoll) return;
        _nextSanityPoll = Time.time + SanityPollInterval;

        bool isLow = _sanity.currentSanity <= _sanity.sanityThreshold;
        if (isLow == _sanityWasLow) return;
        _sanityWasLow = isLow;

        ScreenReaderOutput.Speak(isLow ? "Cordura baja." : "Cordura recuperada.", isLow);
    }

    public static string HealthLevelText(PlayerHealth.HealthLevel level)
    {
        switch (level)
        {
            case PlayerHealth.HealthLevel.Healthy: return "Salud recuperada.";
            case PlayerHealth.HealthLevel.Injured: return "Estás herido.";
            case PlayerHealth.HealthLevel.Critical: return "Salud crítica.";
            case PlayerHealth.HealthLevel.NearDeath: return "Al borde de la muerte.";
            case PlayerHealth.HealthLevel.Dead: return "Has muerto.";
            default: return level.ToString();
        }
    }
}
