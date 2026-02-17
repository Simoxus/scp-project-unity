using Cysharp.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Facility.Generation
{
    public class RoomNavigation : MonoBehaviour
    {
        [SerializeField] private NavMeshSurface navMeshSurface;

        [Header("Settings")]
        [SerializeField] private LayerMask geometryLayerMask = -1;
        [SerializeField] private Vector3 volumePadding = new Vector3(2f, 5f, 2f);

        private RoomInstance _roomInstance;
        private BoxCollider _roomBounds;
        private bool _isBaked = false;

        private void Awake()
        {
            InitializeNavMeshSurface();
            ConfigureNavMeshSurface();
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

            _roomBounds = _roomInstance.GetComponent<BoxCollider>();
        }

        private void ConfigureNavMeshSurface()
        {
            if (navMeshSurface == null || _roomInstance == null || _roomBounds == null)
                return;

            navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navMeshSurface.collectObjects = CollectObjects.Volume;
            navMeshSurface.layerMask = geometryLayerMask;

            Vector3 worldCenter = _roomInstance.transform.TransformPoint(_roomBounds.center);
            Vector3 worldSize = Vector3.Scale(_roomBounds.size, _roomInstance.transform.lossyScale);

            navMeshSurface.center = transform.InverseTransformPoint(worldCenter);
            navMeshSurface.size = worldSize + volumePadding;
        }

        public async UniTask BakeNavMeshAsync()
        {
            if (navMeshSurface == null) return;
            if (_isBaked) return;

            var asyncOp = navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);

            while (!asyncOp.isDone)
            {
                await UniTask.Yield();
            }

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
    }
}