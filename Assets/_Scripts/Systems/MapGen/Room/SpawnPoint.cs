using UnityEngine;

namespace Facility.Generation
{
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Spawn Info")]
        [SerializeField] private SpawnType spawnType = SpawnType.Entity;
        [SerializeField] private bool isActive = true;

        [Header("Enemy Settings")]
        [SerializeField] private string enemyID = "";
        [SerializeField] private int minDifficulty = 0;
        [SerializeField] private int maxDifficulty = 10;

        [Header("Item Settings")]
        [SerializeField] private string itemID = "";
        [SerializeField] private float spawnChance = 1.0f;

        [Header("Visual")]
        [SerializeField] private Color gizmoColor = Color.red;

        public SpawnType Type => spawnType;
        public bool IsActive => isActive;
        public string EnemyID => enemyID;
        public string ItemID => itemID;
        public float SpawnChance => spawnChance;
        public int MinDifficulty => minDifficulty;
        public int MaxDifficulty => maxDifficulty;

        public bool IsValidForDifficulty(int difficulty)
        {
            return difficulty >= minDifficulty && difficulty <= maxDifficulty;
        }

        public Vector3 GetSpawnPosition()
        {
            return transform.position;
        }

        public Quaternion GetSpawnRotation()
        {
            return transform.rotation;
        }

        public void MarkAsUsed()
        {
            isActive = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = isActive ? gizmoColor : Color.gray;
            switch (spawnType)
            {
                case SpawnType.Entity:
                    DrawEnemyGizmo();
                    break;
                case SpawnType.Item:
                    DrawItemGizmo();
                    break;
                case SpawnType.Player:
                    DrawPlayerGizmo();
                    break;
            }

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 1.5f,
                $"{spawnType}\n{(spawnType == SpawnType.Entity ? enemyID : itemID)}"
            );
        }

        private void DrawEnemyGizmo()
        {
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one);
            Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * 1.5f);
        }

        private void DrawItemGizmo()
        {
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.3f, 0.3f);
        }

        private void DrawPlayerGizmo()
        {
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
            Gizmos.DrawLine(transform.position + Vector3.right * 0.5f, transform.position + Vector3.right * 0.5f + Vector3.up * 2f);
            Gizmos.DrawLine(transform.position + Vector3.left * 0.5f, transform.position + Vector3.left * 0.5f + Vector3.up * 2f);
        }
#endif
    }

    public enum SpawnType
    {
        Entity,
        Item,
        Player
    }
}