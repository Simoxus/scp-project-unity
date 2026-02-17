using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Facility.Generation
{
    public class RoomInstance : MonoBehaviour
    {
        [Space]
        [SerializeField] private BoxCollider roomBounds;
        [SerializeField] private GameObject roomGeometry;
        [SerializeField] private GameObject roomLights;
        [SerializeField] private GameObject roomProps;
        [SerializeField] private GameObject roomDoors;
        [SerializeField] private GameObject roomSounds;
        [SerializeField] private GameObject roomPoints;
        [SerializeField] private GameObject roomNavigation;
        [SerializeField] private GameObject roomSpawns;
        [SerializeField] private GameObject roomEvents;
        [SerializeField] private GameObject roomExtra;

        [Header("Runtime")]
        [SerializeField, ReadOnly] private RoomData roomData;
        [SerializeField, ReadOnly] private Vector2Int gridCoordinate;
        [SerializeField, ReadOnly] private int facilityLevel;
        [SerializeField, ReadOnly] private int generationSeed;
        [SerializeField, ReadOnly] private int currentRotation;
        [SerializeField, ReadOnly] private ZoneLocation zone;

        public event Action OnRoomEntered;
        public event Action OnRoomExited;

        public BoxCollider Bounds => roomBounds;
        public GameObject Geometry => roomGeometry;
        public GameObject Lights => roomLights;
        public GameObject Props => roomProps;
        public GameObject Doors => roomDoors;
        public GameObject Sounds => roomSounds;
        public GameObject Triggers => roomPoints;
        public GameObject Navigation => roomNavigation;
        public GameObject Spawns => roomSpawns;
        public GameObject Events => roomEvents;
        public GameObject Extra => roomExtra;

        private readonly List<RoomInstance> _neighborRooms = new List<RoomInstance>(4);
        private CullingSystem _cullingSystem;
        private RoomNavigation _roomNavigationComponent;
        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        private bool _hasAppliedEnvironment;

        public RoomData RoomData => roomData;
        public Vector2Int GridCoordinate => gridCoordinate;
        public int FacilityLevel => facilityLevel;
        public int GenerationSeed => generationSeed;
        public int CurrentRotation => currentRotation;
        public ZoneLocation Zone => zone;

        private void Awake()
        {
            CacheNavigation();
            CacheSpawnPoints();
        }

        private void OnDestroy()
        {
            if (_cullingSystem != null)
            {
                _cullingSystem.UnregisterRoom(this);
            }

            if (Core.FacilityManager != null)
            {
                Core.FacilityManager.UnregisterRoom(this);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                EnterRoom();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                ExitRoom();
            }
        }

        public void Initialize(RoomData roomData, Vector2Int gridCoord, int level, int seed, int rotation, ZoneLocation zone, CullingSystem cullingSystem = null)
        {
            this.roomData = roomData;
            this.gridCoordinate = gridCoord;
            this.facilityLevel = level;
            this.generationSeed = seed;
            this.currentRotation = rotation % 4;
            this.zone = zone;

            _cullingSystem = cullingSystem;

            if (_cullingSystem != null)
            {
                _cullingSystem.RegisterRoom(this);
            }

            if (Core.FacilityManager != null)
            {
                Core.FacilityManager.RegisterRoom(this);
            }

            EnsureComponentsCached();
        }

        private void CacheNavigation()
        {
            _roomNavigationComponent = GetComponentInChildren<RoomNavigation>();
        }

        private void CacheSpawnPoints()
        {
            _spawnPoints.Clear();

            SpawnPoint[] points;

            // Check roomSpawns first, then roomPoints
            if (roomSpawns)
            {
                points = roomSpawns.GetComponentsInChildren<SpawnPoint>(includeInactive: true);
            }
            else if (roomPoints)
            {
                points = roomPoints.GetComponentsInChildren<SpawnPoint>(includeInactive: true);
            }
            else
            {
                points = GetComponentsInChildren<SpawnPoint>(includeInactive: true);
            }

            _spawnPoints.AddRange(points);
        }

        private void EnsureComponentsCached()
        {
            if (_roomNavigationComponent == null) CacheNavigation();
            if (_spawnPoints.Count == 0) CacheSpawnPoints();
        }

        public Bounds GetRoomBounds()
        {
            if (roomBounds != null)
            {
                return roomBounds.bounds;
            }

            return new Bounds(transform.position, Vector3.one * 10f);
        }

        public GameObject[] GetCullableObjects()
        {
            List<GameObject> cullable = new List<GameObject>();

            if (roomLights != null) cullable.Add(roomLights);
            if (roomProps != null) cullable.Add(roomProps);
            if (roomSounds != null) cullable.Add(roomSounds);

            return cullable.ToArray();
        }

        public Renderer[] GetCullableRenderers()
        {
            if (roomGeometry == null) return new Renderer[0];
            return roomGeometry.GetComponentsInChildren<Renderer>();
        }

        public GameObject[] GetEssentialObjects()
        {
            List<GameObject> essential = new List<GameObject>();

            if (roomDoors != null) essential.Add(roomDoors);
            if (roomPoints != null) essential.Add(roomPoints);
            if (roomNavigation != null) essential.Add(roomNavigation);
            if (roomSpawns != null) essential.Add(roomSpawns);

            return essential.ToArray();
        }

        public async UniTask BakeNavigationAsync()
        {
            if (_roomNavigationComponent != null)
            {
                await _roomNavigationComponent.BakeNavMeshAsync();
            }
        }

        public void LinkNavigationToNeighbor(RoomInstance neighbor, Vector3 thisEnd, Vector3 neighborEnd)
        {
            if (_roomNavigationComponent != null && neighbor._roomNavigationComponent != null)
            {
                _roomNavigationComponent.CreateLinkToRoom(neighbor._roomNavigationComponent, thisEnd, neighborEnd);
            }
        }

        public SpawnPoint GetSpawnPoint(SpawnType type)
        {
            return _spawnPoints.Find(sp => sp.Type == type && sp.IsActive);
        }

        public SpawnPoint GetRandomSpawnPoint(SpawnType type)
        {
            List<SpawnPoint> points = GetSpawnPoints(type);
            return points.Count > 0 ? points[UnityEngine.Random.Range(0, points.Count)] : null;
        }

        public List<SpawnPoint> GetSpawnPoints(SpawnType type)
        {
            return _spawnPoints.FindAll(sp => sp.Type == type && sp.IsActive);
        }

        public IReadOnlyList<SpawnPoint> GetAllSpawnPoints()
        {
            return _spawnPoints.AsReadOnly();
        }

        public void AddNeighbor(RoomInstance neighbor)
        {
            if (neighbor != null && !_neighborRooms.Contains(neighbor))
            {
                _neighborRooms.Add(neighbor);
            }
        }

        public IReadOnlyList<RoomInstance> GetNeighbors()
        {
            return _neighborRooms.AsReadOnly();
        }

        public void EnterRoom()
        {
            OnRoomEntered?.Invoke();
            ApplyEnvironmentSettings();
        }

        public void ExitRoom()
        {
            OnRoomExited?.Invoke();
            RevertToOriginalEnvironment();
        }

        private void ApplyEnvironmentSettings()
        {
            if (roomData == null || _hasAppliedEnvironment) return;

            if (Core.MusicManager != null)
            {
                if (roomData.HasCustomMusic)
                {
                    Core.MusicManager.PlayMusicWithGracePeriod(roomData.CustomMusic);
                }
                else
                {
                    if (Core.FacilityGenerator != null && Core.FacilityGenerator.Settings != null)
                    {
                        var zoneSettings = Core.FacilityGenerator.Settings.GetZoneSettings(zone);
                        if (zoneSettings != null && !zoneSettings.zoneMusic.IsNull)
                        {
                            Core.MusicManager.SetZoneMusic(zoneSettings.zoneMusic);
                        }
                    }
                    Core.MusicManager.PlayZoneMusic();
                }

                if (roomData.HasCustomAmbientLoop)
                {
                    Core.MusicManager.SetAmbientLoop(
                        roomData.CustomAmbientLoop,
                        roomData.MinPlayInterval,
                        roomData.MaxPlayInterval
                    );
                }
                else
                {
                    if (Core.FacilityGenerator != null && Core.FacilityGenerator.Settings != null)
                    {
                        var zoneSettings = Core.FacilityGenerator.Settings.GetZoneSettings(zone);
                        if (zoneSettings != null && !zoneSettings.zoneAmbientLoop.IsNull)
                        {
                            Core.MusicManager.SetAmbientLoop(
                                zoneSettings.zoneAmbientLoop,
                                zoneSettings.minAmbientInterval,
                                zoneSettings.maxAmbientInterval
                            );
                        }
                    }
                }
            }

            if (roomData.HasCustomFog)
            {
                Core.FacilityManager.SetFogColor(roomData.CustomFogColor, roomData.CustomFogFadeTime);
            }

            if (roomData.HasCustomAmbient)
            {
                Core.FacilityManager.SetAmbientColor(roomData.CustomAmbientColor, roomData.CustomAmbientFadeTime);
            }

            _hasAppliedEnvironment = true;
        }

        private void RevertToOriginalEnvironment()
        {
            if (roomData == null || !_hasAppliedEnvironment) return;

            if (roomData.HasCustomFog)
            {
                Core.FacilityManager.ResetFog(roomData.CustomFogFadeTime);
            }

            if (roomData.HasCustomAmbient)
            {
                Core.FacilityManager.ResetAmbient(roomData.CustomAmbientFadeTime);
            }

            _hasAppliedEnvironment = false;
        }
    }
}