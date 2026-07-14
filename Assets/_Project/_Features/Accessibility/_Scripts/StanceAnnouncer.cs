using UnityEngine;

/// <summary>
/// Announces crouch state changes through the screen reader. Born from QA: the player got
/// crouched without knowing it (crouch is silent, only shown visually) and then "sprint
/// didn't work" — crouching caps speed and blocks sprinting in this game.
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab edits.
/// Maps to gameaccessibilityguidelines.com (Vision): "Ensure no essential information is
/// conveyed by visuals alone".
/// </summary>
public class StanceAnnouncer : MonoBehaviour
{
    private Player _player;
    private bool _wasCrouching;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _wasCrouching = _player != null && _player.IsInState(PlayerState.Crouching);
    }

    private void Update()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled || _player == null) return;

        bool crouching = _player.IsInState(PlayerState.Crouching);
        if (crouching != _wasCrouching)
        {
            _wasCrouching = crouching;
            ScreenReaderOutput.Speak(_wasCrouching ? "Agachado. Control vuelve a ponerte de pie." : "De pie.");
        }
    }
}
