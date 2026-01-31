using EditorAttributes;
using FMODUnity;
using System.Collections.Generic;
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

        [Header("Room Type")]
        [SerializeField] private RoomLayout roomLayout;
        [SerializeField] private bool isRequired;
        [SerializeField] private bool isUnique;
        [HideField(nameof(isRequired))]
        [SerializeField] private float spawnWeight = 1f;
        [SerializeField] private bool isLarge;
        [ShowField(nameof(isLarge)), Tooltip("Additional cells this room occupies relative to its origin/anchor point")]
        [SerializeField] private Vector2Int[] expandRelativeToOrigin = new Vector2Int[] { };

        [Header("Room Positioning")]
        [Tooltip("Enable custom positional offset for this room")]
        [SerializeField] private bool hasCustomOffset = false;

        [Tooltip("Positional offset to apply when instantiating this room (local space)")]
        [SerializeField, ShowField(nameof(hasCustomOffset))]
        private Vector3 roomOffset = Vector3.zero;

        [Tooltip("Additional rotation offset (in degrees, Y-axis)")]
        [SerializeField, ShowField(nameof(hasCustomOffset))]
        private float rotationOffset = 0f;

        [Header("Room Orientation")]
        [Tooltip("Define which exits the room prefab has in its default (0°) orientation. North=Forward, East=Right, South=Back, West=Left")]
        [SerializeField] private bool defaultExitNorth;
        [SerializeField] private bool defaultExitEast;
        [SerializeField] private bool defaultExitSouth;
        [SerializeField] private bool defaultExitWest;

        [Header("Environment")]
        [SerializeField] private bool hasCustomMusic;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomMusic))]
        private EventReference customMusic;

        [SerializeField] private bool hasCustomAmbientLoop;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomAmbientLoop))]
        private EventReference customAmbientLoop;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomAmbientLoop))]
        private float minPlayInterval = 30f;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomAmbientLoop))]
        private float maxPlayInterval = 90f;

        [SerializeField] private bool hasCustomFog;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomFog))]
        private Color customFogColor = Color.gray;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomFog))]
        private float customFogFadeTime = 2f;

        [SerializeField] private bool hasCustomAmbient;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomAmbient))]
        private Color customAmbientColor = Color.white;
        [SerializeField, IndentProperty(15f), ShowField(nameof(hasCustomAmbient))]
        private float customAmbientFadeTime = 2f;

        public AssetReferenceGameObject RoomPrefabReference => roomPrefabReference;
        public string RoomID => roomID;
        public string RoomName => roomName;
        public string Description => description;
        public RoomLayout Layout => roomLayout;
        public bool IsRequired => isRequired;
        public bool IsUnique => isUnique;
        public float SpawnWeight => spawnWeight;
        public bool IsLarge => isLarge;
        public Vector2Int[] ExpandRelativeToOrigin => expandRelativeToOrigin;

        public bool HasCustomOffset => hasCustomOffset;
        public Vector3 RoomOffset => roomOffset;
        public float RotationOffset => rotationOffset;

        public bool HasCustomMusic => hasCustomMusic;
        public EventReference CustomMusic => customMusic;
        public bool HasCustomAmbientLoop => hasCustomAmbientLoop;
        public EventReference CustomAmbientLoop => customAmbientLoop;
        public float MinPlayInterval => minPlayInterval;
        public float MaxPlayInterval => maxPlayInterval;
        public bool HasCustomFog => hasCustomFog;
        public Color CustomFogColor => customFogColor;
        public float CustomFogFadeTime => customFogFadeTime;
        public bool HasCustomAmbient => hasCustomAmbient;
        public Color CustomAmbientColor => customAmbientColor;
        public float CustomAmbientFadeTime => customAmbientFadeTime;

        public int ConnectionCount
        {
            get
            {
                return roomLayout switch
                {
                    RoomLayout.DeadEnd => 1,
                    RoomLayout.Hallway => 2,
                    RoomLayout.Corner => 2,
                    RoomLayout.Junction => 3,
                    RoomLayout.Crossroads => 4,
                    RoomLayout.Checkpoint => 2,
                    _ => 0
                };
            }
        }

        public bool[] GetDefaultExitPattern()
        {
            return new bool[]
            {
                defaultExitNorth,
                defaultExitEast,
                defaultExitSouth,
                defaultExitWest
            };
        }

        public Vector2Int[] GetOccupiedCells()
        {
            if (!isLarge)
                return new Vector2Int[] { Vector2Int.zero };

            List<Vector2Int> cells = new List<Vector2Int>();
            cells.Add(Vector2Int.zero); // anchor cell

            if (expandRelativeToOrigin != null)
            {
                cells.AddRange(expandRelativeToOrigin);
            }

            return cells.ToArray();
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
        Junction,
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