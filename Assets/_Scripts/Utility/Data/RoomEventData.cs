using UnityEngine;

[CreateAssetMenu(fileName = "RoomEvent", menuName = "Custom/Room Event Data")]
public class RoomEventData : ScriptableObject
{
    [Header("Event Settings")]
    public string eventID;
    public string eventName;
    [TextArea] public string eventDescription;

    [Header("Trigger Settings")]
    public bool triggerOnce = true;
    public float initialDelay = 0f;
    public TriggerType triggerType = TriggerType.OnEnter;
    public string requiredTag = "Player";
}

public enum TriggerType
{
    OnEnter,        // Trigger when entering the collider
    OnExit,         // Trigger when exiting the collider
    OnStay,         // Trigger while staying in collider (continuous)
    OnEnterOnce     // Trigger once on first enter, then disable
}