using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Facility.Generation.EditorTools
{
    public static class RoomNavigationGenerator
    {
        [MenuItem("Simoxus/Rooms/Generate Navigation Setup", false, 2)]
        public static void GenerateNavigationForSelection()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select the Room GameObjects first.", "OK");
                return;
            }

            int updatedCount = 0;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Refresh Navigation Components");

            foreach (GameObject roomRoot in selectedObjects)
            {
                // 1. Get the BoxCollider from the root (The Room Bounds)
                BoxCollider rootBounds = roomRoot.GetComponent<BoxCollider>();
                if (rootBounds == null)
                {
                    Debug.LogWarning($"<color=orange><b>Skipped:</b></color> {roomRoot.name} has no BoxCollider on the root.");
                    continue;
                }

                // 2. Find or Create the "Navigation" GameObject
                Transform navTransform = roomRoot.transform.Find("Navigation");
                GameObject navGo;

                if (navTransform == null)
                {
                    navGo = new GameObject("Navigation");
                    Undo.RegisterCreatedObjectUndo(navGo, "Create Navigation Object");
                    navGo.transform.SetParent(roomRoot.transform);
                    navGo.transform.localPosition = Vector3.zero;
                    navGo.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    navGo = navTransform.gameObject;
                    Undo.RegisterFullObjectHierarchyUndo(navGo, "Replace Nav Components");

                    // Scrub existing components
                    var existingRoomNavs = navGo.GetComponents<RoomNavigation>();
                    foreach (var comp in existingRoomNavs) Undo.DestroyObjectImmediate(comp);

                    var existingSurfaces = navGo.GetComponents<NavMeshSurface>();
                    foreach (var comp in existingSurfaces) Undo.DestroyObjectImmediate(comp);
                }

                // 3. Add Fresh Components
                RoomNavigation roomNav = Undo.AddComponent<RoomNavigation>(navGo);
                NavMeshSurface surface = Undo.AddComponent<NavMeshSurface>(navGo);

                // 4. Configure Surface to match BoxCollider Bounds
                surface.collectObjects = CollectObjects.Volume;
                surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;

                // Center and Size must be local to the Navigation object
                // Since Navigation is at (0,0,0) of the room, we can copy the BoxCollider values directly
                surface.center = rootBounds.center;
                surface.size = rootBounds.size;

                // 5. Link the NavMeshSurface via SerializedObject
                SerializedObject so = new SerializedObject(roomNav);
                SerializedProperty surfaceProp = so.FindProperty("navMeshSurface");
                if (surfaceProp != null)
                {
                    surfaceProp.objectReferenceValue = surface;
                    so.ApplyModifiedProperties();
                }

                // Force save the changes
                EditorUtility.SetDirty(navGo);
                EditorUtility.SetDirty(roomNav);
                EditorUtility.SetDirty(surface);

                updatedCount++;
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Debug.Log($"<color=green><b>Navigation Generator:</b></color> Re-synced {updatedCount} rooms with Volume matching BoxColliders.");
        }

        [MenuItem("Simoxus/Rooms/Generate Navigation Setup", true)]
        private static bool ValidateGenerateNavigation() => Selection.activeGameObject != null;
    }
}