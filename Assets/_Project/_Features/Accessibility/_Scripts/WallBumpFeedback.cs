using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays a soft thud when the player deliberately walks into a wall (not floors, not interactables).
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab or scene edits.
/// Audio goes through FMOD Core (the project disables Unity's audio engine).
/// Maps to gameaccessibilityguidelines.com (Vision): "Ensure no essential information is conveyed by visuals alone".
/// </summary>
public class WallBumpFeedback : MonoBehaviour
{
    [SerializeField] private float bumpCooldown = 0.5f;
    [SerializeField] private float bumpVolumeScale = 0.9f;

    private Player _player;
    private readonly List<A11yFmodAudio.A11ySound> _bumpSounds = new List<A11yFmodAudio.A11ySound>();
    private float _lastBumpTime;
    private int _interactableLayer;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _interactableLayer = LayerMask.NameToLayer("Interactable");

        // Bundled CC0 thuds (Kenney "Impact Sounds", 5 variants to avoid repetition fatigue)
        for (int i = 0; i < 5; i++)
        {
            var sound = A11yFmodAudio.LoadOrGenerate($"wall_bump_00{i}.ogg", 90f, 0.06f);
            if (sound.valid) _bumpSounds.Add(sound);
            if (!sound.valid && i == 0) break; // fallback tone already covers it; don't add 5 identical tones
        }
        if (_bumpSounds.Count == 0)
        {
            var fallback = A11yFmodAudio.Generate(90f, 0.06f);
            if (fallback.valid) _bumpSounds.Add(fallback);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < _bumpSounds.Count; i++)
        {
            var sound = _bumpSounds[i];
            A11yFmodAudio.Release(ref sound);
        }
        _bumpSounds.Clear();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;
        if (Time.time - _lastBumpTime < bumpCooldown) return;
        if (hit.gameObject.layer == _interactableLayer) return; // interactables already announce themselves via the sonar
        if (_player == null || _player.Inputs == null) return;
        if (_bumpSounds.Count == 0) return;

        Vector2 moveInput = _player.Inputs.MoveInput;
        if (moveInput.sqrMagnitude < 0.04f) return; // not deliberately walking

        Vector3 worldMoveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
        if (!SonarLogic.ShouldBumpFeedback(worldMoveDirection, hit.normal)) return;

        _lastBumpTime = Time.time;
        var chosen = _bumpSounds[Random.Range(0, _bumpSounds.Count)];
        A11yFmodAudio.PlayAt(chosen, hit.point, manager.sonarVolume * bumpVolumeScale);
        A11yHaptics.Pulse(this, 0.05f, 0.25f, 0.05f); // subtle high-frequency tick: strong rumble is reserved for damage (QA design rule)
    }
}
