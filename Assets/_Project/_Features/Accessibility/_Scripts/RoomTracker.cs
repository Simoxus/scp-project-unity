using Facility.Generation;
using UnityEngine;

/// <summary>
/// Announces where you are when you enter a new area — the feature our blind QA
/// missed most in The Last of Us ("it never tells you which room you walked into").
/// In generated facilities this uses the game's own RoomInstance bounds (room name +
/// zone from RoomData); in hand-built test scenes it falls back to raycasting the
/// floor and climbing to the zone container. Descriptions come from the alt-text
/// registry ("room:{name}" keys) with a humanized name as fallback.
/// Attached to the player GameObject at runtime by ProximitySonar — no prefab edits.
/// Maps to gameaccessibilityguidelines.com (Vision): "Ensure no essential information
/// is conveyed by visuals alone".
/// </summary>
public class RoomTracker : MonoBehaviour
{
    private const float PollInterval = 1f;
    private const float RoomListRefreshInterval = 10f;

    private int _floorMask;
    private float _nextPoll;
    private float _nextRoomListRefresh;
    private RoomInstance[] _rooms = new RoomInstance[0];
    private string _currentArea;

    private void Awake()
    {
        _floorMask = ~LayerMask.GetMask("Interactable", "Player", "Ignore Raycast", "TransparentFX", "UI", "Debris");
    }

    private void Update()
    {
        var manager = AccessibilityManager.Instance;
        if (manager == null || !manager.sonarEnabled) return;
        var gameManager = Core.GameManager;
        if (gameManager != null && gameManager.gamePaused) return;
        if (Time.unscaledTime < _nextPoll) return;
        _nextPoll = Time.unscaledTime + PollInterval;

        string area = ResolveFromRoomInstances() ?? ResolveFromHierarchy();
        if (string.IsNullOrEmpty(area) || area == _currentArea) return;
        _currentArea = area;

        string description = A11yAltText.TryGet("room:" + area, out string alt)
            ? alt
            : A11yAltText.HumanizeName(area);
        ScreenReaderOutput.Speak($"Entraste a: {description}.");
    }

    // Generated facility: the game's own room system knows exactly where we are
    private string ResolveFromRoomInstances()
    {
        if (Time.unscaledTime >= _nextRoomListRefresh)
        {
            _nextRoomListRefresh = Time.unscaledTime + RoomListRefreshInterval;
            _rooms = FindObjectsByType<RoomInstance>(FindObjectsSortMode.None);
        }

        for (int i = 0; i < _rooms.Length; i++)
        {
            var room = _rooms[i];
            if (room == null || room.Bounds == null) continue;
            if (!room.Bounds.bounds.Contains(transform.position)) continue;

            string roomName = room.RoomData != null ? room.RoomData.RoomName : room.gameObject.name;
            return $"{room.Zone}/{roomName}";
        }

        return null;
    }

    // Hand-built test scenes: climb from the floor we stand on to the zone container
    private string ResolveFromHierarchy()
    {
        if (!Physics.Raycast(transform.position + Vector3.up * 0.3f, Vector3.down, out RaycastHit hit, 5f, _floorMask, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        Transform node = hit.transform;
        Transform room = node;
        while (node != null)
        {
            if (node.name.EndsWith("Zone"))
            {
                return room != node ? $"{node.name}/{room.name}" : node.name;
            }
            room = node;
            node = node.parent;
        }

        return null;
    }
}
