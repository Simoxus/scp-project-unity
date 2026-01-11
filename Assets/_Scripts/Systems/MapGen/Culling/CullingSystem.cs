using System.Collections.Generic;
using UnityEngine;

namespace Facility.Generation
{
    public class CullingSystem : Singleton<CullingSystem>
    {
        [Header("Settings")]
        [SerializeField] private int simulationDistance = 120;
        [SerializeField] private float updateInterval = 0.1f;
        [SerializeField] private float hysteresisDistance = 10f;

        [Header("References")]
        [SerializeField] private Transform cullingOrigin;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private int activeRoomCount = 0;
        [SerializeField] private int totalRoomCount = 0;

        private readonly Dictionary<RoomInstance, RoomCullingState> roomStates = new Dictionary<RoomInstance, RoomCullingState>();
        private readonly HashSet<RoomInstance> activeRooms = new HashSet<RoomInstance>();
        private readonly List<RoomInstance> roomsToRemove = new List<RoomInstance>();

        private float updateTimer = 0f;
        private float simulationDistanceSqr;
        private float activationDistanceSqr;
        private float deactivationDistanceSqr;

        public Transform CullingOrigin
        {
            get => cullingOrigin;
            set
            {
                cullingOrigin = value;
                Log.VerboseInfo($"Culling origin set to {value?.name}");
            }
        }

        public bool IsActive { get; set; } = false;
        public int SimulationDistance => simulationDistance;
        public int ActiveRoomCount => activeRoomCount;
        public int TotalRoomCount => totalRoomCount;

        protected override void Awake()
        {
            base.Awake();
            UpdateDistanceThresholds();
        }

        private void Update()
        {
            if (!IsActive || cullingOrigin == null)
                return;

            updateTimer += Time.deltaTime;

            if (updateTimer >= updateInterval)
            {
                updateTimer = 0f;
                UpdateCulling();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Clear();
        }

        public void RegisterRoom(RoomInstance room)
        {
            if (room == null)
            {
                Log.Warning("Attempted to register null room");
                return;
            }

            if (room.transform == null)
            {
                Log.Warning($"Room {room.name} has null transform, cannot register");
                return;
            }

            if (roomStates.ContainsKey(room))
            {
                Log.VerboseWarning($"Room {room.name} already registered");
                return;
            }

            var state = new RoomCullingState(room);
            if (!state.IsValid)
            {
                Log.Warning($"Room {room.name} culling state is invalid, not registering");
                return;
            }

            roomStates[room] = state;
            totalRoomCount = roomStates.Count;

            if (IsActive && cullingOrigin != null)
            {
                float distanceSqr = (room.transform.position - cullingOrigin.position).sqrMagnitude;
                bool shouldCull = distanceSqr > activationDistanceSqr;
                state.SetCulled(shouldCull);

                if (!shouldCull)
                {
                    activeRooms.Add(room);
                }
            }
            else
            {
                state.SetCulled(false);
                activeRooms.Add(room);
            }

            activeRoomCount = activeRooms.Count;

            Log.VerboseInfo($"Registered room {room.name} at {room.GridCoordinate}");
        }

        public void UnregisterRoom(RoomInstance room)
        {
            if (room == null) return;

            if (roomStates.Remove(room))
            {
                activeRooms.Remove(room);
                totalRoomCount = roomStates.Count;
                activeRoomCount = activeRooms.Count;

                Log.VerboseInfo($"Unregistered room {room.name}");
            }
        }

        public void SetSimulationDistancePreset(int preset)
        {
            simulationDistance = preset switch
            {
                0 => 60,
                1 => 120,
                2 => 200,
                3 => 350,
                4 => 1000000,
                _ => 120
            };

            UpdateDistanceThresholds();
            Log.Info($"Simulation distance set to {simulationDistance}");

            if (IsActive)
            {
                ForceUpdate();
            }
        }

        public void SetSimulationDistance(int distance)
        {
            simulationDistance = Mathf.Max(0, distance);
            UpdateDistanceThresholds();
            Log.Info($"Custom simulation distance set to {simulationDistance}");

            if (IsActive)
            {
                ForceUpdate();
            }
        }

        public IReadOnlyCollection<RoomInstance> GetActiveRooms()
        {
            return activeRooms;
        }

        public void ForceUpdate()
        {
            if (!IsActive || cullingOrigin == null)
                return;

            UpdateCulling();
        }

        public void Clear()
        {
            foreach (var state in roomStates.Values)
            {
                state.SetCulled(false);
            }

            roomStates.Clear();
            activeRooms.Clear();
            roomsToRemove.Clear();
            totalRoomCount = 0;
            activeRoomCount = 0;

            Log.VerboseInfo("Cleared all rooms");
        }

        private void UpdateCulling()
        {
            if (cullingOrigin == null) return;

            Vector3 playerPos = cullingOrigin.position;
            int activatedCount = 0;
            int deactivatedCount = 0;

            roomsToRemove.Clear();

            foreach (var kvp in roomStates)
            {
                RoomInstance room = kvp.Key;
                RoomCullingState state = kvp.Value;

                if (room == null || room.gameObject == null || room.transform == null)
                {
                    roomsToRemove.Add(room);
                    continue;
                }

                if (!state.IsValid)
                {
                    Log.Warning($"Room {room.name} state became invalid, marking for removal");
                    roomsToRemove.Add(room);
                    continue;
                }

                float distanceSqr = (room.transform.position - playerPos).sqrMagnitude;

                if (state.IsCulled)
                {
                    if (distanceSqr <= activationDistanceSqr)
                    {
                        state.SetCulled(false);
                        activeRooms.Add(room);
                        activatedCount++;

                        if (showDebugInfo)
                        {
                            Log.VerboseInfo($"Activated {room.name} at distance {Mathf.Sqrt(distanceSqr):F1}");
                        }
                    }
                }
                else
                {
                    if (distanceSqr > deactivationDistanceSqr)
                    {
                        state.SetCulled(true);
                        activeRooms.Remove(room);
                        deactivatedCount++;

                        if (showDebugInfo)
                        {
                            Log.VerboseInfo($"Deactivated {room.name} at distance {Mathf.Sqrt(distanceSqr):F1}");
                        }
                    }
                }
            }

            if (roomsToRemove.Count > 0)
            {
                Log.VerboseWarning($"Removing {roomsToRemove.Count} null/destroyed rooms from tracking");

                foreach (var room in roomsToRemove)
                {
                    roomStates.Remove(room);
                    activeRooms.Remove(room);
                }

                totalRoomCount = roomStates.Count;
            }

            activeRoomCount = activeRooms.Count;

            if (showDebugInfo && (activatedCount > 0 || deactivatedCount > 0))
            {
                Log.VerboseInfo($"Activated {activatedCount}, Deactivated {deactivatedCount}, Active: {activeRoomCount}/{totalRoomCount}");
            }
        }

        private void UpdateDistanceThresholds()
        {
            simulationDistanceSqr = simulationDistance * simulationDistance;
            activationDistanceSqr = simulationDistanceSqr;
            deactivationDistanceSqr = (simulationDistance + hysteresisDistance) * (simulationDistance + hysteresisDistance);

            Log.VerboseInfo($"Thresholds updated - Activation: {Mathf.Sqrt(activationDistanceSqr):F1}, Deactivation: {Mathf.Sqrt(deactivationDistanceSqr):F1}");
        }

        #region Gizmos
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (cullingOrigin == null) return;

            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawWireSphere(cullingOrigin.position, simulationDistance);

            Gizmos.color = new Color(1, 1, 0, 0.1f);
            Gizmos.DrawWireSphere(cullingOrigin.position, simulationDistance + hysteresisDistance);

            Gizmos.color = Color.green;
            foreach (var room in activeRooms)
            {
                if (room != null && room.transform != null)
                {
                    Gizmos.DrawLine(cullingOrigin.position, room.transform.position);
                }
            }
        }
#endif
        #endregion
    }

    public class RoomCullingState
    {
        private readonly RoomInstance room;
        private readonly GameObject[] cullableObjects;
        private readonly Renderer[] cullableRenderers;
        private bool isCulled;

        public bool IsCulled => isCulled;
        public bool IsValid { get; private set; }

        public RoomCullingState(RoomInstance room)
        {
            this.room = room;
            this.isCulled = false;
            this.IsValid = false;

            if (room == null)
            {
                Log.Warning("Cannot create state for null room");
                cullableObjects = new GameObject[0];
                cullableRenderers = new Renderer[0];
                return;
            }

            cullableObjects = room.GetCullableObjects() ?? new GameObject[0];
            cullableRenderers = room.GetCullableRenderers() ?? new Renderer[0];

            IsValid = true;
        }

        public void SetCulled(bool culled)
        {
            if (isCulled == culled) return;

            if (!IsValid || room == null || room.gameObject == null)
            {
                IsValid = false;
                return;
            }

            isCulled = culled;
            bool targetActive = !culled;

            foreach (var obj in cullableObjects)
            {
                if (obj != null)
                {
                    try
                    {
                        obj.SetActive(targetActive);
                    }
                    catch (System.Exception e)
                    {
                        Log.Warning($"Failed to set active state on cullable object in {room.name}: {e.Message}");
                    }
                }
            }

            foreach (var renderer in cullableRenderers)
            {
                if (renderer != null)
                {
                    try
                    {
                        renderer.enabled = targetActive;
                    }
                    catch (System.Exception e)
                    {
                        Log.Warning($"Failed to set renderer state in {room.name}: {e.Message}");
                    }
                }
            }

            if (cullableObjects.Length > 0 || cullableRenderers.Length > 0)
            {
                Log.VerboseInfo($"Set {cullableObjects.Length} objects and {cullableRenderers.Length} renderers to {targetActive} for room {room.name}");
            }
        }
    }
}