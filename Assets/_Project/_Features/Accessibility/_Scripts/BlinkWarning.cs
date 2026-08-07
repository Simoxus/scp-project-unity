using UnityEngine;

/// <summary>
/// Short non-spatial audio cue when the blink meter is about to force a blink (~15% left),
/// so blind players can time their surroundings check before their eyes close — essential
/// once SCP-173-style enemies land. Fires once per blink cycle; re-arms when the blink ends.
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab edits.
/// Audio goes through FMOD Core (the project disables Unity's audio engine).
/// Maps to gameaccessibilityguidelines.com (Vision): "Ensure no essential information is
/// conveyed by visuals alone" (the blink meter is only a visual bar).
/// A sound (not speech) on purpose: blinks recur every ~15 seconds and TTS would fatigue.
/// </summary>
public class BlinkWarning : MonoBehaviour
{
    [SerializeField, Range(0.05f, 0.5f)] private float warnAtBlinkFraction = 0.15f;
    [SerializeField] private float cueFrequency = 880f;
    [SerializeField] private float cueDuration = 0.045f;

    private PlayerBlink _blink;
    private A11yFmodAudio.A11ySound _cue;
    private bool _armed = true;

    private void Awake()
    {
        _blink = GetComponent<PlayerBlink>();
        _cue = A11yFmodAudio.LoadOrGenerate("blink_warning.ogg", cueFrequency, cueDuration);
    }

    private void OnDestroy()
    {
        A11yFmodAudio.Release(ref _cue);
    }

    private void OnEnable()
    {
        if (_blink != null) _blink.OnBlinkEnded += Rearm;
    }

    private void OnDisable()
    {
        if (_blink != null) _blink.OnBlinkEnded -= Rearm;
    }

    private void Update()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled || _blink == null) return;

        var gameManager = Core.GameManager;
        if (gameManager != null && gameManager.gamePaused) return;

        if (SonarLogic.ShouldWarnBlink(_armed, _blink.IsBlinking, _blink.currentBlink, warnAtBlinkFraction))
        {
            _armed = false;
            A11yFmodAudio.Play2D(_cue, manager.sonarVolume);
        }
    }

    private void Rearm() => _armed = true;
}
