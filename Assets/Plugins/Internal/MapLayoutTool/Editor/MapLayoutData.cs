using UnityEngine;
using EditorAttributes;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CustomMapLayout", menuName = "Custom Data/Map Layout Data")]
public class MapLayoutData : ScriptableObject
{
    public float gridWidth = 20.5f;
    public float gridHeight = 20.5f;
    public Dictionary<Vector2Int, Placement> placements = new Dictionary<Vector2Int, Placement>();
}

[System.Serializable]
public class Placement
{
    public ZoneLocation zoneLocation;
    public RoomType roomType;
    public bool isRequired;

    public AllowedConnections allowedConnections;
    public List<RoomType> allowedRoomTypes = new List<RoomType>();
}

[System.Serializable]
public struct AllowedConnections
{
    public bool north, east, south, west;
}

// Different types of zones
public enum ZoneLocation
{
    SurfaceZone,
    EntranceZone,
    HeavyZone,
    LightZone,
    Custom
}

// Different types of rooms
public enum RoomType
{
    Containment,
    Checkpoint,
    DeadEnd,
    TwoWay,
    ThreeWay,
    FourWay,
    Corner,
    Custom
}