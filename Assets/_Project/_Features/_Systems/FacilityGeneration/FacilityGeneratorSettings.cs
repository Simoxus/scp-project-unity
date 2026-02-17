using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facility.Generation
{
    [CreateAssetMenu(fileName = "FacilityGeneratorSettings", menuName = "Custom/Map Gen/Facility Generator Settings")]
    public class FacilityGeneratorSettings : ScriptableObject
    {
        [Space]
        [SerializeField] private List<ZoneSettings> zones = new List<ZoneSettings>();

        [Header("Grid Settings")]
        [SerializeField] private int gridWidth = 19;
        [SerializeField] private int gridHeight = 19;
        [SerializeField] private float cellSize = 20f;

        [Header("Start Room Settings")]
        [SerializeField] private RoomData startingRoom;
        [SerializeField] private int startRoomIsolationRows = 0;

        [Header("Hallway Generation")]
        [Tooltip("Min/max length for the initial hallway after isolation")]
        [SerializeField] private int initialHallwayLengthMin = 10;
        [SerializeField] private int initialHallwayLengthMax = 15;

        [Tooltip("Min/max length for horizontal hallways")]
        [SerializeField] private int horizontalHallwayLengthMin = 10;
        [SerializeField] private int horizontalHallwayLengthMax = 15;

        [Tooltip("Number of vertical hallway connections to spawn per horizontal hallway")]
        [SerializeField] private int verticalConnectionsMin = 4;
        [SerializeField] private int verticalConnectionsMax = 5;

        [Tooltip("Min/max length for vertical hallways")]
        [SerializeField] private int verticalHallwayLengthMin = 3;
        [SerializeField] private int verticalHallwayLengthMax = 4;

        [Header("Room Requirements")]
        [SerializeField] private int minDeadEnds = 5;
        [SerializeField] private int minCorners = 1;
        [SerializeField] private int minCrossroads = 1;

        [Header("Generation Settings")]
        [SerializeField] private int maxGenerationAttempts = 10;
        [SerializeField] private bool allowOverlaps = true;

        [Header("Stopwatch Settings (in milliseconds)")]
        [SerializeField] private float gridStructureRecommendedTime = 20;
        [SerializeField] private float cellConnectionRecommendedTime = 10;
        [SerializeField] private float validationRecommendedTime = 6;
        [SerializeField] private float roomAssignmentRecommendedTime = 2500;
        [SerializeField] private float instantiationRecommendedTime = 4000;
        [SerializeField] private float navigationRecommendedTime = 2000;
        [SerializeField] private float navigationLinksRecommendedTime = 35;
        [SerializeField] private float doorCreationRecommendedTime = 500;
        [SerializeField] private float generationRecommendedTime = 10000;

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellSize => cellSize;

        public RoomData StartingRoom => startingRoom;
        public int StartRoomIsolationRows => startRoomIsolationRows;

        public int InitialHallwayLengthMin => initialHallwayLengthMin;
        public int InitialHallwayLengthMax => initialHallwayLengthMax;
        public int HorizontalHallwayLengthMin => horizontalHallwayLengthMin;
        public int HorizontalHallwayLengthMax => horizontalHallwayLengthMax;
        public int VerticalHallwayLengthMin => verticalHallwayLengthMin;
        public int VerticalHallwayLengthMax => verticalHallwayLengthMax;
        public int VerticalConnectionsMin => verticalConnectionsMin;
        public int VerticalConnectionsMax => verticalConnectionsMax;

        public IReadOnlyList<ZoneSettings> Zones => zones;

        public int MinDeadEnds => minDeadEnds;
        public int MinCorners => minCorners;
        public int MinCrossroads => minCrossroads;

        public int MaxGenerationAttempts => maxGenerationAttempts;
        public bool AllowOverlaps => allowOverlaps;

        public float GridStructureRecommendedTime => gridStructureRecommendedTime;
        public float CellConnectionRecommendedTime => cellConnectionRecommendedTime;
        public float ValidationRecommendedTime => validationRecommendedTime;
        public float RoomAssignmentRecommendedTime => roomAssignmentRecommendedTime;
        public float InstantiationRecommendedTime => instantiationRecommendedTime;
        public float NavigationRecommendedTime => navigationRecommendedTime;
        public float NavigationLinksRecommendedTime => navigationLinksRecommendedTime;
        public float DoorCreationRecommendedTime => doorCreationRecommendedTime;
        public float GenerationRecommendedTime => generationRecommendedTime;

        public ZoneSettings GetZoneSettings(ZoneLocation location)
        {
            return zones.Find(z => z.zoneLocation == location);
        }
    }

    [Serializable]
    public class ZoneSettings
    {
        [SerializeField] public ZoneLocation zoneLocation;
        [SerializeField] public string zoneName;
        [SerializeField] public int startRow;
        [SerializeField] public int endRow;
        [SerializeField] public RoomPool roomPool;
        [SerializeField] public DoorPool doorPool;
        [SerializeField] public EventReference zoneMusic;
        [SerializeField] public EventReference zoneAmbientLoop;
        [SerializeField] public float minAmbientInterval = 30f;
        [SerializeField] public float maxAmbientInterval = 90f;
        [SerializeField] public Color debugColor = Color.white;

        public bool ContainsRow(int row)
        {
            return row >= startRow && row <= endRow;
        }

        public int GetRowCount()
        {
            return endRow - startRow + 1;
        }
    }
}