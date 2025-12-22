using EditorAttributes;
using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomData", menuName = "Custom/Room Data")]
public class RoomData : ScriptableObject
{
    [SerializeField, AssetPreview] public GameObject roomPrefab;

    [Header("Room Info")]
    public string roomID;
    public string roomName;
    [TextArea]
    public string description;

    [Header("Room Properties")]
    public ZoneLocation zoneLocation;
    public RoomType roomType;
    public bool roomIsRequired;
    [SerializeField, HideField(nameof(roomIsRequired))] public float roomRarity;

    [Space]
    public AllowedConnectDirections allowedConnectDirections;

    [Header("Environment")]
    public bool customAmbience;
    [SerializeField, IndentProperty(15f), ShowField(nameof(customAmbience))]
    public EventReference customAmbienceAudio;

    public bool customFog;
    [SerializeField, IndentProperty(15f), ShowField(nameof(customFog))]
    public Color customFogColor;
}

public enum ZoneLocation
{
    SurfaceZone,
    EntranceZone,
    HeavyZone,
    LightZone,
    Custom
}

public enum RoomType
{
    Containment,
    Checkpoint,
    DeadEnd,
    Hallway,
    Custom
}

[System.Serializable]
public struct AllowedConnectDirections
{
    public bool north, east, south, west;
}
