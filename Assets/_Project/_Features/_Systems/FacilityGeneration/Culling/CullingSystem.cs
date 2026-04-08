using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

using ReadOnlyAttribute = TriInspector.ReadOnlyAttribute;

namespace Facility.Generation
{
    public class CullingSystem : Singleton<CullingSystem>
    {
        [Space]
        [SerializeField] private Transform cullingOrigin;
        [Space]
        [SerializeField] private int roomBatchSize = 64;
        [SerializeField] private int lightBatchSize = 128;

        [Header("Rooms")]
        [SerializeField] private float cullDistance = 30f;
        [SerializeField] private float showDistance = 25f;
        [SerializeField] private float updateInterval = 0.1f;

        [Header("Lights")]
        [SerializeField] private bool enableLightCulling = true;
        [SerializeField] private float lightCullDistance = 20f;
        [SerializeField] private float lightShowDistance = 15f;

        [Header("Debug")]
        [SerializeField, ReadOnly] private int visibleRoomCount = 0;
        [SerializeField, ReadOnly] private int totalRoomCount = 0;
        [SerializeField, ReadOnly] private int visibleLightCount = 0;
        [SerializeField, ReadOnly] private int totalLightCount = 0;

        // Room tracking
        private readonly List<RoomInstance> _rooms = new List<RoomInstance>();
        private readonly Dictionary<RoomInstance, RoomCullingState> _roomStates = new Dictionary<RoomInstance, RoomCullingState>();
        private readonly Dictionary<Vector2Int, RoomInstance> _registeredRooms = new Dictionary<Vector2Int, RoomInstance>();
        private readonly Dictionary<Vector2Int, HashSet<Vector2Int>> _largeRoomOccupiedCells = new Dictionary<Vector2Int, HashSet<Vector2Int>>();
        private readonly List<RoomInstance> _roomsToRemove = new List<RoomInstance>();

        // Light tracking
        private readonly List<LightCullingState> _allLights = new List<LightCullingState>();

        // For jobs
        private NativeArray<Vector3> _roomPositions;
        private NativeArray<bool> _roomWasCulled;
        private NativeArray<bool> _roomShouldBeCulled;
        private NativeArray<float> _roomExtents;
        private NativeArray<Vector3> _lightPositions;
        private NativeArray<bool> _lightWasCulled;
        private NativeArray<bool> _lightShouldBeCulled;

        // Track pending jobs
        private JobHandle _lastRoomJobHandle;
        private JobHandle _lastLightJobHandle;
        private bool _hasRoomJobPending;
        private bool _hasLightJobPending;

        private float _updateTimer = 0f;
        private bool _isUpdating = false;
        private bool _nativeArraysInitialized = false;

        public Transform CullingOrigin
        {
            get => cullingOrigin;
            set
            {
                cullingOrigin = value;
                Log.VerboseInfo($"Culling origin set to {value?.name}");
            }
        }

        public float CullDistance
        {
            get => cullDistance;
            set
            {
                cullDistance = value;
                showDistance = Mathf.Min(showDistance, cullDistance - 1f);
            }
        }

        public bool IsActive { get; set; } = false;
        public int VisibleRoomCount => visibleRoomCount;
        public int TotalRoomCount => totalRoomCount;

        private void Update()
        {
            if (!IsActive || cullingOrigin == null) return;

            _updateTimer += Time.deltaTime;

            if (_updateTimer >= updateInterval)
            {
                _updateTimer = 0f;
                UpdateCullingAsync().Forget();
            }
        }

        protected override void OnSingletonDestroy()
        {
            Clear(activateCulledObjects: false);
            DisposeNativeArrays();
        }

        public void RegisterRoom(RoomInstance room)
        {
            if (room == null || room.transform == null) return;

            Vector2Int roomPos = room.GridCoordinate;

            if (!_registeredRooms.ContainsKey(roomPos))
            {
                _registeredRooms[roomPos] = room;
                _rooms.Add(room);

                var state = new RoomCullingState(room);
                if (state.IsValid)
                {
                    _roomStates[room] = state;
                }

                if (room.RoomData != null && room.RoomData.IsLarge)
                {
                    RegisterLargeRoomCells(room);
                }

                if (enableLightCulling)
                {
                    RegisterRoomLights(room);
                }

                totalRoomCount = _registeredRooms.Count;
                _nativeArraysInitialized = false;
            }
        }

        private void RegisterLargeRoomCells(RoomInstance room)
        {
            Vector2Int anchorPos = room.GridCoordinate;
            Vector2Int[] occupiedCells = room.RoomData.GetOccupiedCells();

            if (!_largeRoomOccupiedCells.ContainsKey(anchorPos))
            {
                _largeRoomOccupiedCells[anchorPos] = new HashSet<Vector2Int>();
            }

            foreach (var offset in occupiedCells)
            {
                Vector2Int cellPos = anchorPos + offset;
                _largeRoomOccupiedCells[anchorPos].Add(cellPos);
            }
        }

        private void RegisterRoomLights(RoomInstance room)
        {
            Light[] lights = room.GetComponentsInChildren<Light>(true);

            foreach (var light in lights)
            {
                var lightState = new LightCullingState(light);
                _allLights.Add(lightState);
            }

            totalLightCount = _allLights.Count;
        }

        public void UnregisterRoom(RoomInstance room)
        {
            if (room == null) return;

            Vector2Int roomPos = room.GridCoordinate;

            if (_registeredRooms.Remove(roomPos))
            {
                _rooms.Remove(room);
                _roomStates.Remove(room);
                _largeRoomOccupiedCells.Remove(roomPos);

                _allLights.RemoveAll(l => l.Light != null && l.Light.transform.IsChildOf(room.transform));

                totalRoomCount = _registeredRooms.Count;
                totalLightCount = _allLights.Count;
                _nativeArraysInitialized = false;
            }
        }

        private void RebuildNativeArrays()
        {
            DisposeNativeArrays();

            // Rebuild room arrays
            int roomCount = _rooms.Count;
            if (roomCount > 0)
            {
                _roomPositions = new NativeArray<Vector3>(roomCount, Allocator.Persistent);
                _roomWasCulled = new NativeArray<bool>(roomCount, Allocator.Persistent);
                _roomShouldBeCulled = new NativeArray<bool>(roomCount, Allocator.Persistent);
                _roomExtents = new NativeArray<float>(roomCount, Allocator.Persistent);

                for (int i = 0; i < roomCount; i++)
                {
                    if (_rooms[i] != null && _rooms[i].transform != null)
                    {
                        Bounds bounds = _rooms[i].GetRoomBounds();
                        _roomPositions[i] = bounds.center;
                        _roomWasCulled[i] = _roomStates.TryGetValue(_rooms[i], out var state) && state.IsCulled;
                    }
                }
            }

            // Rebuild light arrays
            int lightCount = _allLights.Count;
            if (lightCount > 0 && enableLightCulling)
            {
                _lightPositions = new NativeArray<Vector3>(lightCount, Allocator.Persistent);
                _lightWasCulled = new NativeArray<bool>(lightCount, Allocator.Persistent);
                _lightShouldBeCulled = new NativeArray<bool>(lightCount, Allocator.Persistent);

                for (int i = 0; i < lightCount; i++)
                {
                    if (_allLights[i].Light != null)
                    {
                        _lightPositions[i] = _allLights[i].Light.transform.position;
                        _lightWasCulled[i] = _allLights[i].IsCulled;
                    }
                }
            }

            _nativeArraysInitialized = true;
        }

        private async UniTaskVoid UpdateCullingAsync()
        {
            if (_isUpdating) return;

            _isUpdating = true;

            try
            {
                // Rebuild native arrays if needed (room added/removed)
                if (!_nativeArraysInitialized)
                {
                    RebuildNativeArrays();
                }

                if (_rooms.Count == 0)
                {
                    visibleRoomCount = 0;
                    visibleLightCount = 0;
                    _isUpdating = false;
                    return;
                }

                Vector3 originPos = cullingOrigin.position;

                // Update room positions and culled states
                for (int i = 0; i < _rooms.Count; i++)
                {
                    if (_rooms[i] != null && _rooms[i].transform != null)
                    {
                        Bounds bounds = _rooms[i].GetRoomBounds();
                        _roomPositions[i] = bounds.center;
                        _roomWasCulled[i] = _roomStates.TryGetValue(_rooms[i], out var state) && state.IsCulled;
                        _roomExtents[i] = bounds.extents.magnitude;
                    }
                }

                // Schedule room culling job
                var roomJob = new RoomDistanceCullingJob
                {
                    RoomPositions = _roomPositions,
                    WasCulled = _roomWasCulled,
                    RoomExtents = _roomExtents,
                    OriginPosition = originPos,
                    CullDistanceSqr = cullDistance * cullDistance,
                    ShowDistanceSqr = showDistance * showDistance,
                    ShouldBeCulled = _roomShouldBeCulled
                };

                _lastRoomJobHandle = roomJob.Schedule(_rooms.Count, roomBatchSize);
                _hasRoomJobPending = true;

                // Schedule light culling job if enabled
                if (enableLightCulling && _allLights.Count > 0)
                {
                    // Update light positions
                    for (int i = 0; i < _allLights.Count; i++)
                    {
                        if (_allLights[i].Light != null)
                        {
                            _lightPositions[i] = _allLights[i].Light.transform.position;
                            _lightWasCulled[i] = _allLights[i].IsCulled;
                        }
                    }

                    var lightJob = new LightDistanceCullingJob
                    {
                        LightPositions = _lightPositions,
                        WasCulled = _lightWasCulled,
                        OriginPosition = originPos,
                        CullDistanceSqr = lightCullDistance * lightCullDistance,
                        ShowDistanceSqr = lightShowDistance * lightShowDistance,
                        ShouldBeCulled = _lightShouldBeCulled
                    };

                    _lastLightJobHandle = lightJob.Schedule(_allLights.Count, lightBatchSize);
                    _hasLightJobPending = true;
                }

                // Wait for both jobs to complete
                await UniTask.WaitWhile(() => !_lastRoomJobHandle.IsCompleted);
                _lastRoomJobHandle.Complete();
                _hasRoomJobPending = false;

                if (enableLightCulling && _allLights.Count > 0)
                {
                    await UniTask.WaitWhile(() => !_lastLightJobHandle.IsCompleted);
                    _lastLightJobHandle.Complete();
                    _hasLightJobPending = false;
                }

                // Apply results on main thread
                int roomsVisible = 0;
                _roomsToRemove.Clear();

                for (int i = 0; i < _rooms.Count; i++)
                {
                    RoomInstance room = _rooms[i];

                    if (room == null || room.gameObject == null)
                    {
                        _roomsToRemove.Add(room);
                        continue;
                    }

                    if (_roomStates.TryGetValue(room, out var state) && state.IsValid)
                    {
                        bool shouldBeCulled = _roomShouldBeCulled[i];
                        state.SetCulled(shouldBeCulled);

                        if (!shouldBeCulled)
                        {
                            roomsVisible++;
                        }
                    }
                }

                // Clean up invalid rooms
                if (_roomsToRemove.Count > 0)
                {
                    foreach (var room in _roomsToRemove)
                    {
                        UnregisterRoom(room);
                    }
                }

                // Apply light culling results
                int lightsVisible = 0;
                if (enableLightCulling)
                {
                    for (int i = _allLights.Count - 1; i >= 0; i--)
                    {
                        LightCullingState lightState = _allLights[i];

                        if (lightState.Light == null)
                        {
                            _allLights.RemoveAt(i);
                            _nativeArraysInitialized = false; // Need to rebuild arrays
                            continue;
                        }

                        if (i < _lightShouldBeCulled.Length)
                        {
                            bool shouldBeCulled = _lightShouldBeCulled[i];
                            lightState.SetCulled(shouldBeCulled);

                            if (!shouldBeCulled)
                            {
                                lightsVisible++;
                            }
                        }
                    }

                    totalLightCount = _allLights.Count;
                }

                visibleRoomCount = roomsVisible;
                visibleLightCount = lightsVisible;
            }
            finally
            {
                _isUpdating = false;
            }

            await UniTask.Yield();
        }

        private void CompleteAllPendingJobs()
        {
            if (_hasRoomJobPending)
            {
                _lastRoomJobHandle.Complete();
                _hasRoomJobPending = false;
            }

            if (_hasLightJobPending)
            {
                _lastLightJobHandle.Complete();
                _hasLightJobPending = false;
            }
        }

        private void DisposeNativeArrays()
        {
            CompleteAllPendingJobs();

            if (_roomPositions.IsCreated) _roomPositions.Dispose();
            if (_roomWasCulled.IsCreated) _roomWasCulled.Dispose();
            if (_roomShouldBeCulled.IsCreated) _roomShouldBeCulled.Dispose();
            if (_roomExtents.IsCreated) _roomExtents.Dispose();
            if (_lightPositions.IsCreated) _lightPositions.Dispose();
            if (_lightWasCulled.IsCreated) _lightWasCulled.Dispose();
            if (_lightShouldBeCulled.IsCreated) _lightShouldBeCulled.Dispose();

            _nativeArraysInitialized = false;
        }

        public void ShowAllRooms()
        {
            foreach (var state in _roomStates.Values) state.SetCulled(false);
            foreach (var lightState in _allLights) lightState.SetCulled(false);

            visibleRoomCount = totalRoomCount;
            visibleLightCount = totalLightCount;
        }

        public void ForceUpdate()
        {
            UpdateCullingAsync().Forget();
        }

        public IReadOnlyCollection<RoomInstance> GetVisibleRooms()
        {
            List<RoomInstance> visible = new List<RoomInstance>();

            foreach (var kvp in _roomStates)
            {
                if (!kvp.Value.IsCulled && kvp.Key != null)
                {
                    visible.Add(kvp.Key);
                }
            }

            return visible;
        }

        public void Clear(bool activateCulledObjects = true)
        {
            CompleteAllPendingJobs();

            if (activateCulledObjects)
            {
                foreach (var state in _roomStates.Values)
                {
                    state.SetCulled(false);
                }

                foreach (var lightState in _allLights)
                {
                    lightState.SetCulled(false);
                }
            }

            _rooms.Clear();
            _registeredRooms.Clear();
            _roomStates.Clear();
            _largeRoomOccupiedCells.Clear();
            _roomsToRemove.Clear();
            _allLights.Clear();

            visibleRoomCount = 0;
            totalRoomCount = 0;
            visibleLightCount = 0;
            totalLightCount = 0;

            DisposeNativeArrays();
        }
    }

    public class RoomCullingState
    {
        private readonly RoomInstance _room;
        private readonly GameObject[] _cullableObjects;
        private readonly Renderer[] _cullableRenderers;
        private bool _isCulled;

        public bool IsCulled => _isCulled;
        public bool IsValid { get; private set; }

        public RoomCullingState(RoomInstance room)
        {
            _room = room;
            _isCulled = false;
            IsValid = false;

            if (room == null)
            {
                _cullableObjects = new GameObject[0];
                _cullableRenderers = new Renderer[0];
                return;
            }

            _cullableObjects = room.GetCullableObjects() ?? new GameObject[0];
            _cullableRenderers = room.GetCullableRenderers() ?? new Renderer[0];

            IsValid = true;
        }

        public void SetCulled(bool culled)
        {
            if (_isCulled == culled) return;

            if (!IsValid || _room == null || _room.gameObject == null)
            {
                IsValid = false;
                return;
            }

            _isCulled = culled;
            bool targetActive = !culled;

            foreach (var obj in _cullableObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(targetActive);
                }
            }

            foreach (var renderer in _cullableRenderers)
            {
                if (renderer != null)
                {
                    renderer.enabled = targetActive;
                }
            }
        }
    }

    public class LightCullingState
    {
        private readonly Light _light;
        private readonly float _originalIntensity;
        private bool _isCulled;

        public Light Light => _light;
        public bool IsCulled => _isCulled;

        public LightCullingState(Light light)
        {
            _light = light;
            _originalIntensity = light != null ? light.intensity : 0f;
            _isCulled = false;
        }

        public void SetCulled(bool culled)
        {
            if (_isCulled == culled || _light == null) return;

            _isCulled = culled;

            if (culled)
            {
                _light.enabled = false;
            }
            else
            {
                _light.enabled = true;
                _light.intensity = _originalIntensity;
            }
        }
    }

    [BurstCompile]
    public struct RoomDistanceCullingJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> RoomPositions;
        [ReadOnly] public NativeArray<bool> WasCulled;
        [ReadOnly] public NativeArray<float> RoomExtents;
        [ReadOnly] public Vector3 OriginPosition;
        [ReadOnly] public float CullDistanceSqr;
        [ReadOnly] public float ShowDistanceSqr;

        [WriteOnly] public NativeArray<bool> ShouldBeCulled;

        public void Execute(int index)
        {
            Vector3 diff = RoomPositions[index] - OriginPosition;
            float distanceSqr = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;

            float extentBonus = RoomExtents[index];
            float extentBonusSqr = extentBonus * extentBonus;

            // Hysteresis logic
            bool shouldCull = WasCulled[index]
                ? distanceSqr > (ShowDistanceSqr + extentBonusSqr)
                : distanceSqr > (CullDistanceSqr + extentBonusSqr);

            ShouldBeCulled[index] = shouldCull;
        }
    }

    [BurstCompile]
    public struct LightDistanceCullingJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> LightPositions;
        [ReadOnly] public NativeArray<bool> WasCulled;
        [ReadOnly] public Vector3 OriginPosition;
        [ReadOnly] public float CullDistanceSqr;
        [ReadOnly] public float ShowDistanceSqr;

        [WriteOnly] public NativeArray<bool> ShouldBeCulled;

        public void Execute(int index)
        {
            Vector3 diff = LightPositions[index] - OriginPosition;
            float distanceSqr = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;

            bool shouldCull = WasCulled[index]
                ? distanceSqr > ShowDistanceSqr
                : distanceSqr > CullDistanceSqr;

            ShouldBeCulled[index] = shouldCull;
        }
    }
}