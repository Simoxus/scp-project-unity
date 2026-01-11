using UnityEngine;

namespace Facility.Generation
{
    [CreateAssetMenu(fileName = "_FacilitySettings", menuName = "Custom/Map Gen/Generation Settings")]
    public class FacilityGeneratorSettings : ScriptableObject
    {
        [SerializeField] private RoomPool roomPool;
        [SerializeField] private DoorPool doorPool;

        [Header("Map Settings")]
        [SerializeField] private Vector2Int mapSize = new Vector2Int(21, 21);
        [SerializeField] private int generationSeed = 67;
        [SerializeField] private ZoneLocation zoneType = ZoneLocation.LightContainmentZone;

        [Header("Generation Settings")]
        [SerializeField] private int minPathLength = 10;
        [SerializeField] private int maxPathLength = 20;
        [SerializeField] private int branchingFactor = 3;
        [SerializeField] private float branchChance = 0.4f;

        [Header("Placement Settings")]
        [SerializeField] private float cellSize = 10f;
        [SerializeField] private float maxOverlapPercentage = 0.1f;
        [SerializeField] private int maxPlacementAttempts = 50;
        [SerializeField] private int maxGenerationRetries = 5;

        [Header("Path Settinsg")]
        [SerializeField] private int checkpointInterval = 5;
        [SerializeField] private bool guaranteeCheckpoints = true;

        public RoomPool RoomPool => roomPool;
        public DoorPool DoorPool => doorPool;
        public Vector2Int MapSize => mapSize;
        public int GenerationSeed => generationSeed;
        public ZoneLocation ZoneType => zoneType;
        public int MinPathLength => minPathLength;
        public int MaxPathLength => maxPathLength;
        public int BranchingFactor => branchingFactor;
        public float BranchChance => branchChance;
        public float CellSize => cellSize;
        public float MaxOverlapPercentage => maxOverlapPercentage;
        public int MaxPlacementAttempts => maxPlacementAttempts;
        public int MaxGenerationRetries => maxGenerationRetries;
        public int CheckpointInterval => checkpointInterval;
        public bool GuaranteeCheckpoints => guaranteeCheckpoints;

        public void Initialize()
        {
            if (roomPool != null)
            {
                roomPool.Initialize();
            }
        }

        public void SetSeed(int seed) => generationSeed = seed;
        public void SetMapSize(Vector2Int size) => mapSize = new Vector2Int(Mathf.Max(5, size.x), Mathf.Max(5, size.y));
    }
}