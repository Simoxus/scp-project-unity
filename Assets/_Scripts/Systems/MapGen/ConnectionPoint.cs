using UnityEngine;

namespace Facility.Generation
{
    public class ConnectionPoint : MonoBehaviour
    {
        [Header("Connection Info")]
        [SerializeField] private Direction direction = Direction.North;
        [SerializeField] private bool isActive = true;

        public Direction Direction => direction;
        public bool IsActive => isActive;

        public Vector3 GetWorldPosition() => transform.position;
        public Quaternion GetWorldRotation() => transform.rotation;
        public Vector3 GetForwardDirection() => transform.forward;

        public void SetActive(bool active) => isActive = active;

        public void CalculateConnectionTransform(ConnectionPoint targetPoint, out Vector3 offset, out Quaternion rotation)
        {
            Vector3 oppositeForward = -GetForwardDirection();
            Vector3 targetForward = targetPoint.GetForwardDirection();

            rotation = Quaternion.FromToRotation(targetForward, oppositeForward);

            Vector3 rotatedTargetPos = rotation * (targetPoint.transform.localPosition);
            offset = GetWorldPosition() - rotatedTargetPos;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = isActive ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);

            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, direction.ToString());
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f * 1.5f);
        }
#endif
    }
}