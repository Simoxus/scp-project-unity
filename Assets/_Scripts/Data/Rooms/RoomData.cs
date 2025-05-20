using UnityEngine;
using EditorAttributes;
using System.Collections.Generic;
using System;
using Events.Base;

namespace Rooms.Data
{
    [CreateAssetMenu(fileName = "NewRoomData", menuName = "Rooms/Room Data")]
    public class RoomData : ScriptableObject
    {
        [SerializeField, AssetPreview] public GameObject roomPrefab;

        [Header("Room Info")]
        public string roomID;
        public string roomDisplayID;

        [TextArea]
        public string description;

        [Header("Room Properties")]
        public ZoneLocation zoneLocation;
        public RoomType roomType;
        public bool roomRequired;
        [SerializeField, HideField(nameof(roomRequired))] public float roomRarity;

        [Space]
        public AllowedConnectDirections allowedConnectDirections;

        [Header("Environment")]
        public bool featuresEvent;
        [SerializeField, IndentProperty(15f), ShowField(nameof(featuresEvent))] public RoomEvent featuredEvent;
        public bool customAmbience;
        [SerializeField, IndentProperty(15f), ShowField(nameof(customAmbience))] public AudioClip customAmbienceAudio;
        public bool customFog;
        [SerializeField, IndentProperty(15f), ShowField(nameof(customFog))] public Color customFogColor;
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
}
