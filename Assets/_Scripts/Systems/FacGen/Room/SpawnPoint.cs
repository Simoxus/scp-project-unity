using UnityEngine;

namespace Facility.Generation
{
    public enum SpawnType
    {
        Player,
        Entity,
        Vent
    }

    public class SpawnPoint : MonoBehaviour
    {
        [Space]
        [SerializeField] private SpawnType type;
        [SerializeField] private bool isActive = true;
        [SerializeField] private float spawnRadius = 0.25f;

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;
        public SpawnType Type => type;
        public bool IsActive => isActive;
        public float SpawnRadius => spawnRadius;

        public void SetActive(bool active) => isActive = active;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!isActive) return;

            Color gizmoColor = type switch
            {
                SpawnType.Player => Color.green,
                SpawnType.Entity => Color.red,
                SpawnType.Vent => Color.cyan,
                _ => Color.white
            };

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            UnityEditor.Handles.Label(transform.position + Vector3.up, type.ToString());
        }

        private void OnDrawGizmosSelected()
        {
            Color gizmoColor = type switch
            {
                SpawnType.Player => Color.green,
                SpawnType.Entity => Color.red,
                SpawnType.Vent => Color.cyan,
                _ => Color.white
            };

            gizmoColor.a = 0.3f;
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, spawnRadius);
        }
#endif
    }
}