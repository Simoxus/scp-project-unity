using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facility.Generation
{
    public class RoomInstance : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoxCollider roomBounds;
        [SerializeField] private Transform roomCenter;
        [SerializeField] private GameObject roomGeometry;
        [SerializeField] private GameObject roomLights;
        [SerializeField] private GameObject roomProps;
        [SerializeField] private GameObject roomDoors;
        [SerializeField] private GameObject roomSounds;
        [SerializeField] private GameObject roomPoints;
        [SerializeField] private GameObject roomNavigation;
        [SerializeField] private GameObject roomSpawns;

        [Header("Runtime Data")]
        [SerializeField, ReadOnly] private RoomData roomData;
        [SerializeField, ReadOnly] private Vector2Int gridCoordinate;
        [SerializeField, ReadOnly] private int facilityLevel;
        [SerializeField, ReadOnly] private int generationSeed;
        [SerializeField, ReadOnly] private int currentRotation;

        public event Action<Direction> OnConnectionDisabled;
        public event Action OnRoomEntered;
        public event Action OnRoomExited;

        private List<ConnectionPoint> _allConnectionPoints = new List<ConnectionPoint>();
        private readonly List<RoomInstance> _neighborRooms = new List<RoomInstance>(4);
        private CullingSystem _cullingSystem;
        private RoomNavigation _roomNavigationComponent;
        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        private Tween _fogTween;
        private Color _originalFogColor;
        private bool _hasStoredOriginalFog;

        private static readonly Vector3[] CardinalDirections = new Vector3[]
        {
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 0),
            new Vector3(0, 0, -1),
            new Vector3(-1, 0, 0)
        };

        public BoxCollider RoomBounds => roomBounds;
        public Transform RoomCenter => roomCenter;
        public RoomData RoomData => roomData;
        public Vector2Int GridCoordinate => gridCoordinate;
        public int FacilityLevel => facilityLevel;
        public int GenerationSeed => generationSeed;
        public int CurrentRotation => currentRotation;

        private void Awake()
        {
            CacheAllComponents();
        }

        private void OnDestroy()
        {
            StopFogTransition();

            if (_cullingSystem != null)
            {
                _cullingSystem.UnregisterRoom(this);
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

        public void Initialize(RoomData roomData, Vector2Int gridCoord, int level, int seed, int rotation, CullingSystem cullingSystem = null)
        {
            this.roomData = roomData;
            this.gridCoordinate = gridCoord;
            this.facilityLevel = level;
            this.generationSeed = seed;
            this.currentRotation = rotation % 4;

            _cullingSystem = cullingSystem;
            if (_cullingSystem != null)
            {
                _cullingSystem.RegisterRoom(this);
                Log.VerboseInfo($"Room {gameObject.name} registered with CullingSystem");
            }

            EnsureComponentsCached();
        }

        private void CacheAllComponents()
        {
            CacheConnectionPoints();
            CacheNavigationComponent();
            CacheSpawnPoints();
        }

        private void CacheConnectionPoints()
        {
            _allConnectionPoints.Clear();
            ConnectionPoint[] points = GetComponentsInChildren<ConnectionPoint>(includeInactive: true);
            _allConnectionPoints.AddRange(points);

            Log.VerboseInfo($"Room {gameObject.name} cached {_allConnectionPoints.Count} connection points");
        }

        private void CacheNavigationComponent()
        {
            _roomNavigationComponent = GetComponentInChildren<RoomNavigation>();
        }

        private void CacheSpawnPoints()
        {
            _spawnPoints.Clear();
            SpawnPoint[] points = GetComponentsInChildren<SpawnPoint>(includeInactive: true);
            _spawnPoints.AddRange(points);
        }

        private void EnsureComponentsCached()
        {
            if (_allConnectionPoints.Count == 0) CacheConnectionPoints();
            if (_roomNavigationComponent == null) CacheNavigationComponent();
            if (_spawnPoints.Count == 0) CacheSpawnPoints();
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
                await UniTask.SwitchToMainThread();
                _roomNavigationComponent.BakeNavMesh();
            }
        }

        public void BakeNavigation()
        {
            if (_roomNavigationComponent != null)
            {
                _roomNavigationComponent.BakeNavMesh();
            }
        }

        public void LinkNavigationToNeighbor(RoomInstance neighbor, Vector3 thisEnd, Vector3 neighborEnd)
        {
            if (_roomNavigationComponent != null && neighbor._roomNavigationComponent != null)
            {
                _roomNavigationComponent.CreateLinkToRoom(neighbor._roomNavigationComponent, thisEnd, neighborEnd);
            }
        }

        public IReadOnlyList<SpawnPoint> GetAllSpawnPoints()
        {
            return _spawnPoints.AsReadOnly();
        }

        public List<SpawnPoint> GetSpawnPoints(SpawnType type)
        {
            return _spawnPoints.FindAll(sp => sp.Type == type && sp.IsActive);
        }

        public ConnectionPoint GetConnectionPoint(Direction worldDirection)
        {
            if (_allConnectionPoints.Count == 0)
            {
                Log.Warning($"Room {gameObject.name} has no connection points!");
                return null;
            }

            Vector3 targetDirection = CardinalDirections[(int)worldDirection];
            ConnectionPoint bestMatch = null;
            float bestDot = -2f;

            foreach (var point in _allConnectionPoints)
            {
                if (point == null || !point.gameObject.activeInHierarchy) continue;

                Vector3 pointForward = point.GetForwardDirection().normalized;
                float dot = Vector3.Dot(pointForward, targetDirection);

                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestMatch = point;
                }
            }

            if (bestMatch != null && bestDot > 0.7f)
            {
                return bestMatch;
            }

            return null;
        }

        public IEnumerable<ConnectionPoint> GetAllConnectionPoints()
        {
            return _allConnectionPoints;
        }

        public bool HasConnection(Direction direction)
        {
            return GetConnectionPoint(direction) != null;
        }

        public void DisableConnection(Direction direction)
        {
            ConnectionPoint point = GetConnectionPoint(direction);
            if (point != null)
            {
                point.SetActive(false);
                OnConnectionDisabled?.Invoke(direction);
            }
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

        public Bounds GetRoomBounds()
        {
            if (roomBounds != null)
            {
                return roomBounds.bounds;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                Bounds bounds = colliders[0].bounds;
                for (int i = 1; i < colliders.Length; i++)
                {
                    bounds.Encapsulate(colliders[i].bounds);
                }
                return bounds;
            }

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }

            Vector3 center = roomCenter != null ? roomCenter.position : transform.position;
            return new Bounds(center, Vector3.one * 5f);
        }

        public void EnterRoom()
        {
            OnRoomEntered?.Invoke();
            ApplyEnvironmentSettingsAsync().Forget();
        }

        public void ExitRoom()
        {
            OnRoomExited?.Invoke();
            RevertToOriginalFogAsync().Forget();
        }

        [ContextMenu("Refresh Connection Points")]
        private void RefreshConnectionPoints()
        {
            CacheConnectionPoints();
        }

        private async UniTaskVoid ApplyEnvironmentSettingsAsync()
        {
            if (roomData == null) return;

            if (!_hasStoredOriginalFog)
            {
                _originalFogColor = RenderSettings.fogColor;
                _hasStoredOriginalFog = true;
            }

            if (roomData.HasCustomAmbience)
            {
            }

            if (roomData.HasCustomFog)
            {
                await TransitionFogColorAsync(roomData.CustomFogColor, roomData.CustomFogFadeTime);
            }
        }

        private async UniTaskVoid RevertToOriginalFogAsync()
        {
            if (roomData == null || !roomData.HasCustomFog || !_hasStoredOriginalFog) return;

            await TransitionFogColorAsync(_originalFogColor, roomData.CustomFogFadeTime);
        }

        private async UniTask TransitionFogColorAsync(Color targetColor, float duration)
        {
            StopFogTransition();

            Color startColor = RenderSettings.fogColor;
            _fogTween = Tween.Custom(
                startColor,
                targetColor,
                duration,
                onValueChange:
                newColor => RenderSettings.fogColor = newColor,
                ease: Ease.InOutSine
            );

            await _fogTween.ToYieldInstruction().ToUniTask();
        }

        private void StopFogTransition()
        {
            if (_fogTween.isAlive)
            {
                _fogTween.Stop();
            }
        }
    }
}