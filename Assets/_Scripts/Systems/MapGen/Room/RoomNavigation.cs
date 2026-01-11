using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Facility.Generation
{
    public class RoomNavigation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NavMeshSurface navMeshSurface;

        [Header("Settings")]
        [SerializeField] private bool bakeOnStart = false;
        [SerializeField] private LayerMask geometryLayerMask = -1;
        [SerializeField] private Vector3 volumePadding = new Vector3(2f, 5f, 2f);

        private RoomInstance _roomInstance;
        private bool _isBaked = false;

        private void Awake()
        {
            InitializeNavMeshSurface();
        }

        private void Start()
        {
            if (bakeOnStart)
            {
                BakeNavMesh();
            }
        }

        private void OnDestroy()
        {
            ClearNavMesh();
        }

        private void InitializeNavMeshSurface()
        {
            if (navMeshSurface == null)
            {
                navMeshSurface = GetComponent<NavMeshSurface>();
            }

            if (navMeshSurface == null)
            {
                navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            }

            _roomInstance = GetComponentInParent<RoomInstance>();
            if (_roomInstance == null)
            {
                return;
            }

            ConfigureNavMeshSurface();
        }

        private void ConfigureNavMeshSurface()
        {
            if (navMeshSurface == null || _roomInstance == null)
                return;

            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navMeshSurface.collectObjects = CollectObjects.Volume;
            navMeshSurface.layerMask = geometryLayerMask;

            Bounds roomBounds = GetRoomBounds();

            navMeshSurface.center = transform.InverseTransformPoint(roomBounds.center);

            Vector3 worldSize = roomBounds.size + volumePadding;
            navMeshSurface.size = transform.InverseTransformVector(worldSize);
            navMeshSurface.size = new Vector3(
                Mathf.Abs(navMeshSurface.size.x),
                Mathf.Abs(navMeshSurface.size.y),
                Mathf.Abs(navMeshSurface.size.z)
            );
        }

        private Bounds GetRoomBounds()
        {
            BoxCollider roomCollider = _roomInstance.GetComponent<BoxCollider>();
            if (roomCollider != null)
            {
                Bounds bounds = new Bounds(
                    _roomInstance.transform.TransformPoint(roomCollider.center),
                    Vector3.Scale(roomCollider.size, _roomInstance.transform.lossyScale)
                );
                return bounds;
            }

            Renderer[] renderers = _roomInstance.GetComponentsInChildren<Renderer>(includeInactive: false);
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                return bounds;
            }

            return new Bounds(_roomInstance.RoomCenter.position, new Vector3(20f, 5f, 20f));
        }

        public void BakeNavMesh()
        {
            if (navMeshSurface == null) return;
            if (_isBaked) return;

            ConfigureNavMeshSurface();
            navMeshSurface.BuildNavMesh();
            _isBaked = true;
        }

        public void ClearNavMesh()
        {
            if (navMeshSurface != null && _isBaked)
            {
                navMeshSurface.RemoveData();
                _isBaked = false;
            }
        }

        public NavMeshLink CreateLinkToRoom(RoomNavigation otherRoom, Vector3 worldStartPoint, Vector3 worldEndPoint)
        {
            if (otherRoom == null)
            {
                return null;
            }

            GameObject linkObj = new GameObject($"NavLink_{gameObject.name}_to_{otherRoom.gameObject.name}");
            linkObj.transform.SetParent(transform);
            linkObj.transform.position = worldStartPoint;

            NavMeshLink link = linkObj.AddComponent<NavMeshLink>();
            link.startPoint = Vector3.zero;
            link.endPoint = linkObj.transform.InverseTransformPoint(worldEndPoint);
            link.width = 2f;
            link.costModifier = -1;
            link.bidirectional = true;
            link.autoUpdate = true;
            link.area = 0;

            return link;
        }

        #region Editor
#if UNITY_EDITOR

        private void OnDrawGizmosSelected()
        {
            if (navMeshSurface == null)
                return;

            Gizmos.color = new Color(0, 1, 0, 0.15f);
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(navMeshSurface.center, navMeshSurface.size);
            Gizmos.color = new Color(0, 1, 0, 0.5f);
            Gizmos.DrawWireCube(navMeshSurface.center, navMeshSurface.size);
            Gizmos.matrix = oldMatrix;
        }
#endif
        #endregion
    }
}