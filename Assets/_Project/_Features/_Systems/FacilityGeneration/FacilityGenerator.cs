using Cysharp.Threading.Tasks;
using Facility.Persistence.Types;
using System.Collections.Generic;
using System.Diagnostics;
using TriInspector;
using UnityEngine;

namespace Facility.Generation
{
    public class FacilityGenerator : Singleton<FacilityGenerator>
    {
        [Space]
        [SerializeField] private bool generateOnStart;
        [Space]
        [SerializeField] private FacilityGeneratorSettings settings;
        [SerializeField] private CullingSystem cullingSystem;

        [Header("Seed Configuration")]
        [SerializeField] private bool checkForExistingSeed = true;
        [SerializeField] private bool useRandomSeed = false;
        [SerializeField, HideIf(nameof(useRandomSeed))] private bool useStringSeed = false;
        [SerializeField, HideIf(nameof(useRandomSeed)), ShowIf(nameof(useStringSeed))] private string seedString = "";
        [SerializeField, HideIf(nameof(useRandomSeed))] private int numericSeed;

        [Header("Anchors")]
        [SerializeField] private Transform facilityRoot;
        [SerializeField] private Transform roomAnchor;
        [SerializeField] private Transform doorAnchor;

        [Header("Navigation")]
        [SerializeField] private bool bakeNavigationOnGenerate = true;
        [SerializeField] private bool createNavigationLinks = true;
        [SerializeField] private float doorwayLinkOffset = 1.5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        [SerializeField] private bool showZoneBoundaries = true;

        private FG_LayoutBuilder _layoutBuilder;
        private FG_PathBuilder _pathBuilder;
        private FG_RoomAssigner _roomAssigner;
        private FG_GizmoDrawer _gizmoDrawer;
        private FG_Instantiator _instantiator;
        private FG_NavMeshLinker _navMeshLinker;
        private FG_Persistence _persistence;

        public bool GenerateOnStart => generateOnStart;

        private GridCell[,] _grid;
        private List<GridCell> _occupiedCells = new List<GridCell>();
        private Dictionary<Vector2Int, RoomInstance> _roomInstances = new Dictionary<Vector2Int, RoomInstance>();
        private List<GameObject> _doorInstances = new List<GameObject>();
        private System.Random _random;
        private Stopwatch _totalTimer;
        private GridCell _startRoomCell;

        public GridCell[,] Grid => _grid;
        public int CurrentSeed => numericSeed;
        public string CurrentSeedString => seedString;
        public bool IsGenerated { get; private set; }
        public GridCell StartRoomCell => _startRoomCell;
        public FacilityGeneratorSettings Settings => settings;

        protected override void OnSingletonAwake()
        {
            InitializeModules();
        }

        private void InitializeModules()
        {
            if (cullingSystem == null)
            {
                cullingSystem = Core.CullingSystem;
            }

            if (settings != null)
            {
                _gizmoDrawer = new FG_GizmoDrawer(settings);
                _persistence = new FG_Persistence(settings);
                _navMeshLinker = new FG_NavMeshLinker(settings, doorwayLinkOffset);
            }
        }

        public void Start()
        {
            if (GenerateOnStart)
            {
                GenerateFacility();
            }
        }

        public void SetSeedFromString(string seed)
        {
            seedString = seed;
            numericSeed = FG_SeedUtility.ConvertToNumericSeed(seed);
            useStringSeed = true;
            useRandomSeed = false;
            Log.Info($"String seed '{seedString}' converted to numeric seed '{numericSeed}'");
        }

        public void SetNumericSeed(int seed)
        {
            numericSeed = seed;
            seedString = seed.ToString();
            useStringSeed = false;
            useRandomSeed = false;
        }

        [ContextMenu("Generate Facility")]
        public async void GenerateFacility()
        {
            await GenerateFacilityAsync();
        }

        public async UniTask GenerateFacilityAsync()
        {
            if (settings == null)
            {
                Log.Error("No setting provided");
                return;
            }

            ClearFacility();
            DetermineSeed();

            if (checkForExistingSeed)
            {
                var (facilityData, navLinksData, doorStatesData) = await _persistence.TryLoadFromExistingSeed(seedString);

                if (facilityData != null)
                {
                    await LoadFromFacilityData(facilityData, navLinksData, doorStatesData);
                    Log.Success($"Facility loaded from existing save (seed: {numericSeed})");
                    return;
                }
            }

            await GenerateNewFacility();

            IsGenerated = true;
        }

        private void DetermineSeed()
        {
            if (useRandomSeed)
            {
                numericSeed = FG_SeedUtility.GenerateRandomNumericSeed();
                seedString = numericSeed.ToString();
                Log.Info($"Using random seed '{numericSeed}'");
            }
            else if (useStringSeed && !string.IsNullOrEmpty(seedString))
            {
                numericSeed = FG_SeedUtility.ConvertToNumericSeed(seedString);
                Log.Info($"Using string seed '{seedString}' (numeric: {numericSeed})");
            }
            else
            {
                if (string.IsNullOrEmpty(seedString))
                {
                    seedString = numericSeed.ToString();
                }
                Log.Info($"Using numeric seed '{numericSeed}'");
            }

            _random = new System.Random(numericSeed);
        }

        private async UniTask GenerateNewFacility()
        {
            Log.Header($"Generating new facility with seed '{numericSeed}' (string: '{seedString}')");

            _totalTimer = Stopwatch.StartNew();

            _layoutBuilder = new FG_LayoutBuilder(settings, _random);
            _pathBuilder = new FG_PathBuilder(settings);
            _roomAssigner = new FG_RoomAssigner(settings, numericSeed, _random);
            _instantiator = new FG_Instantiator(settings, numericSeed, roomAnchor, doorAnchor, cullingSystem, _roomInstances, _doorInstances);

            foreach (var zoneSettings in settings.Zones)
            {
                if (zoneSettings.roomPool != null)
                {
                    zoneSettings.roomPool.Initialize();
                }
            }

            var stepTimer = Stopwatch.StartNew();

            Log.Status("Step 1: Generating grid structure");
            var gridResult = _layoutBuilder.GenerateGrid();
            _grid = gridResult.grid;
            _occupiedCells = gridResult.occupiedCells;
            _startRoomCell = gridResult.startCell;

            Log.Duration($"Step 1 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, settings.GridStructureRecommendedTime);
            stepTimer.Restart();

            Log.Status("Step 2: Connecting cells and determining room types");
            _pathBuilder.ConnectCells(_grid, _occupiedCells);

            Log.Duration($"Step 2 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, settings.CellConnectionRecommendedTime);
            stepTimer.Restart();

            Log.Status("Step 3: Validating minimum requirements");
            _pathBuilder.EnsureMinimumRequirements();

            Log.Duration($"Step 3 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, settings.ValidationRecommendedTime);
            stepTimer.Restart();

            Log.Status("Step 4: Assigning rooms to cells");
            await _roomAssigner.AssignRooms(_grid, _occupiedCells, _startRoomCell);

            Log.Duration($"Step 4 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, settings.RoomAssignmentRecommendedTime);
            stepTimer.Restart();

            await FinalizeGeneration(null, null);
        }

        private async UniTask LoadFromFacilityData(
            FacilityPersistData persistData,
            NavLinksPersistData navLinksData,
            DoorStatesPersistData doorStatesData)
        {
            _pathBuilder = new FG_PathBuilder(settings);
            _roomAssigner = new FG_RoomAssigner(settings, numericSeed, _random);
            _instantiator = new FG_Instantiator(settings, numericSeed, roomAnchor, doorAnchor, cullingSystem, _roomInstances, _doorInstances);

            var (grid, occupiedCells, startCell) = _persistence.DeserializeFacility(persistData);

            if (grid == null || occupiedCells == null || startCell == null)
            {
                Log.Error("Failed to deserialize facility data");
                return;
            }

            _grid = grid;
            _occupiedCells = occupiedCells;
            _startRoomCell = startCell;

            await FinalizeGeneration(navLinksData, doorStatesData);
            IsGenerated = true;
        }

        private async UniTask FinalizeGeneration(NavLinksPersistData navLinksData, DoorStatesPersistData doorStatesData = null)
        {
            var stepTimer = Stopwatch.StartNew();

            Log.Status("Step 5: Instantiating rooms");
            await _instantiator.InstantiateRoomsAsync(_occupiedCells);

            Log.Duration($"Step 5 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, settings.InstantiationRecommendedTime);
            stepTimer.Restart();

            if (bakeNavigationOnGenerate)
            {
                Log.Status("Step 6: Baking navigation meshes");
                await _instantiator.BakeAllNavigationAsync(_roomInstances);

                Log.Duration($"Step 6 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, settings.NavigationRecommendedTime);
                stepTimer.Restart();
            }

            if (createNavigationLinks)
            {
                if (navLinksData != null && navLinksData.links != null && navLinksData.links.Count > 0)
                {
                    Log.Status("Step 7: Loading navigation links from save data");
                    _navMeshLinker.LoadNavigationLinksFromData(navLinksData.links, _roomInstances);
                }
                else
                {
                    Log.Status("Step 7: Creating navigation links");
                    _navMeshLinker.CreateNavigationLinks(_grid, _occupiedCells);
                }

                Log.Duration($"Step 7 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, settings.NavigationLinksRecommendedTime);
                stepTimer.Restart();
            }

            Log.Status("Step 8: Creating doors");
            await _instantiator.CreateDoorsAsync(_grid, _occupiedCells);

            Log.Duration($"Step 8 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, settings.DoorCreationRecommendedTime);
            stepTimer.Restart();

            if (doorStatesData != null)
            {
                Log.Status("Step 8.5: Loading door states");
                _persistence.LoadDoorStates(doorStatesData, _doorInstances);

                Log.Duration($"Step 8.5 took {stepTimer.ElapsedMilliseconds}ms", stepTimer.ElapsedMilliseconds, 2);
                stepTimer.Restart();
            }

            if (cullingSystem != null)
            {
                Log.Status("Step 9: Setting up culling system");
                _navMeshLinker.SetupCullingSystem(cullingSystem, _roomInstances.Count);

                Log.Duration($"Step 9 took {stepTimer.ElapsedMilliseconds}ms");
                stepTimer.Restart();
            }

            if (_gizmoDrawer != null)
            {
                _gizmoDrawer.SetData(_grid, _occupiedCells, _startRoomCell);
            }

            stepTimer.Stop();
            Log.Status("Facility generation complete!");
            Log.Duration($"Generation took {_totalTimer.Elapsed.TotalSeconds} seconds", (float)_totalTimer.Elapsed.TotalMilliseconds, settings.GenerationRecommendedTime);
            _totalTimer.Stop();
        }

        [ContextMenu("Clear Facility")]
        public void ClearFacility()
        {
            if (roomAnchor != null)
            {
                for (int i = roomAnchor.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(roomAnchor.GetChild(i).gameObject);
                }
            }
            _roomInstances.Clear();

            if (doorAnchor != null)
            {
                for (int i = doorAnchor.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(doorAnchor.GetChild(i).gameObject);
                }
            }
            _doorInstances.Clear();

            _grid = null;
            _occupiedCells.Clear();
            _startRoomCell = null;

            if (_navMeshLinker != null)
            {
                _navMeshLinker.ClearNavLinkData();
            }

            if (cullingSystem != null)
            {
                cullingSystem.Clear();
            }

            if (Core.FacilityManager != null)
            {
                Core.FacilityManager.ClearRooms();
            }

            IsGenerated = false;
            Log.Info("Facility cleared");
        }

        public FacilityPersistData SaveToData()
        {
            if (!IsGenerated) return null;
            return _persistence.SaveFacilityToData(_grid, _occupiedCells, _startRoomCell, numericSeed, seedString);
        }

        public NavLinksPersistData SaveNavLinksToData()
        {
            if (!IsGenerated) return null;
            return _persistence.SaveNavLinksToData(_navMeshLinker.GetNavLinkData());
        }

        public DoorStatesPersistData SaveDoorStatesToData()
        {
            if (!IsGenerated) return null;
            return _persistence.SaveDoorStatesToData(_doorInstances);
        }

        public bool LoadFromData(
            FacilityPersistData persistData,
            NavLinksPersistData navLinksData = null,
            DoorStatesPersistData doorStatesData = null)
        {
            if (settings == null || persistData == null)
            {
                return false;
            }

            LoadFromPersistDataAsync(persistData, navLinksData, doorStatesData).Forget();
            return true;
        }

        private async UniTask LoadFromPersistDataAsync(
            FacilityPersistData persistData,
            NavLinksPersistData navLinksData,
            DoorStatesPersistData doorStatesData)
        {
            ClearFacility();

            seedString = persistData.seedString;
            numericSeed = persistData.seed;
            _random = new System.Random(numericSeed);

            Log.Info($"Loading facility from persist data (seed: {numericSeed})");

            await LoadFromFacilityData(persistData, navLinksData, doorStatesData);

            IsGenerated = true;
            Log.Success("Facility loaded successfully!");
        }

        [ContextMenu("Quick Save")]
        public async void QuickSave()
        {
            await Core.PersistenceManager.QuickSave();
        }

        [ContextMenu("Quick Load")]
        public async void QuickLoad()
        {
            await Core.PersistenceManager.QuickLoad();
        }

        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            if (_gizmoDrawer == null && settings != null) return;

            if (_gizmoDrawer != null)
            {
                _gizmoDrawer.DrawGizmos();

                if (showZoneBoundaries)
                {
                    _gizmoDrawer.DrawZoneBoundaries();
                }
            }
        }
    }
}