using EditorAttributes;
using FMODUnity;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Facility.Generation
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "Custom/Map Gen/Room Data")]
    public class RoomData : ScriptableObject
    {
        [SerializeField] private AssetReferenceGameObject roomPrefabReference;

        [Header("Room Info")]
        [SerializeField] private string roomID;
        [SerializeField] private string roomName;
        [TextArea][SerializeField] private string description;
        [SerializeField] private ZoneLocation zoneLocation;

        [Header("Room Type")]
        [SerializeField] private RoomLayout roomLayout;
        [SerializeField] private bool isRequired;
        [SerializeField] private bool isUnique;
        [SerializeField, HideField(nameof(isRequired))]
        private float spawnWeight = 1f;

        [Header("Environment")]
        [SerializeField] private bool hasCustomAmbience;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomAmbience))]
        private EventReference customAmbienceAudio;
        [SerializeField] private bool hasCustomFog;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomFog))]
        private Color customFogColor = Color.gray;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomFog))]
        private float customFogFadeTime = 2f;

        public AssetReferenceGameObject RoomPrefabReference => roomPrefabReference;
        public string RoomID => roomID;
        public string RoomName => roomName;
        public string Description => description;
        public ZoneLocation ZoneLocation => zoneLocation;
        public RoomLayout Layout => roomLayout;
        public bool IsRequired => isRequired;
        public bool IsUnique => isUnique;
        public float SpawnWeight => spawnWeight;
        public bool HasCustomAmbience => hasCustomAmbience;
        public EventReference CustomAmbienceAudio => customAmbienceAudio;
        public bool HasCustomFog => hasCustomFog;
        public Color CustomFogColor => customFogColor;
        public float CustomFogFadeTime => customFogFadeTime;

        public int ConnectionCount
        {
            get
            {
                return roomLayout switch
                {
                    RoomLayout.DeadEnd => 1,
                    RoomLayout.Hallway => 2,
                    RoomLayout.Corner => 2,
                    RoomLayout.Intersection => 3,
                    RoomLayout.Crossroads => 4,
                    RoomLayout.Checkpoint => 2,
                    _ => 0
                };
            }
        }
    }

    public enum ZoneLocation
    {
        SurfaceZone,
        EntranceZone,
        HeavyContainmentZone,
        LightContainmentZone,
        CustomZone
    }

    public enum RoomLayout
    {
        DeadEnd,
        Hallway,
        Corner,
        Intersection,
        Crossroads,
        Checkpoint
    }

    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }
}