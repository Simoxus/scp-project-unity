using UnityEditor;
using UnityEditorInternal; // Required for ComponentUtility
using UnityEngine;

namespace Facility.Generation.Editor
{
    public static class RoomInstanceQuickSetup
    {
        [MenuItem("Simoxus/Rooms/Quick Setup", false, 1)]
        private static void QuickSetupSelectedRooms()
        {
            GameObject[] selected = Selection.gameObjects;
            if (selected.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects to setup as rooms.", "OK");
                return;
            }

            int setupCount = 0;
            Undo.RecordObjects(selected, "Quick Setup Rooms");

            foreach (GameObject obj in selected)
            {
                if (SetupRoomInstance(obj))
                {
                    setupCount++;
                }
            }

            Debug.Log($"<color=green>[Room Quick Setup]</color> Successfully setup {setupCount}/{selected.Length} room(s)");
        }

        [MenuItem("Simoxus/Rooms/Quick Setup", true)]
        private static bool ValidateQuickSetup()
        {
            return Selection.gameObjects.Length > 0;
        }

        private static bool SetupRoomInstance(GameObject roomObject)
        {
            if (roomObject == null) return false;

            // 1. Ensure RoomInstance exists
            RoomInstance roomInstance = roomObject.GetComponent<RoomInstance>();
            if (roomInstance == null)
            {
                roomInstance = Undo.AddComponent<RoomInstance>(roomObject);
            }

            SerializedObject so = new SerializedObject(roomInstance);
            so.Update();

            // 2. Setup BoxCollider and Bounds
            BoxCollider boundsCollider = SetupRoomBounds(roomObject, roomInstance);
            SerializedProperty boundsProp = so.FindProperty("roomBounds");
            if (boundsProp != null) boundsProp.objectReferenceValue = boundsCollider;

            // 3. Match child folders
            AssignChildObject(so, roomObject.transform, "roomGeometry", "Geometry");
            AssignChildObject(so, roomObject.transform, "roomLights", "Lights");
            AssignChildObject(so, roomObject.transform, "roomProps", "Props");
            AssignChildObject(so, roomObject.transform, "roomDoors", "Doors");
            AssignChildObject(so, roomObject.transform, "roomSounds", "Sounds");
            AssignChildObject(so, roomObject.transform, "roomPoints", "Triggers");
            AssignChildObject(so, roomObject.transform, "roomNavigation", "Navigation");
            AssignChildObject(so, roomObject.transform, "roomSpawns", "Spawns");
            AssignChildObject(so, roomObject.transform, "roomEvents", "Events");
            AssignChildObject(so, roomObject.transform, "roomExtra", "Extra");

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(roomInstance);
            return true;
        }

        private static BoxCollider SetupRoomBounds(GameObject roomObject, RoomInstance instance)
        {
            // Calculate total bounds of all renderers
            Renderer[] renderers = roomObject.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0) return null;

            Bounds combinedBounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }

            // Get or Add BoxCollider
            BoxCollider box = roomObject.GetComponent<BoxCollider>();
            if (box == null)
            {
                box = Undo.AddComponent<BoxCollider>(roomObject);
            }

            // Set Collider Properties
            box.isTrigger = true;
            box.center = roomObject.transform.InverseTransformPoint(combinedBounds.center);
            box.size = combinedBounds.size;

            // Move component in hierarchy to be directly under RoomInstance
            MoveComponentUnderTarget(roomObject, box, instance);

            return box;
        }

        private static void MoveComponentUnderTarget(GameObject obj, Component componentToMove, Component target)
        {
            Component[] components = obj.GetComponents<Component>();
            int targetIndex = -1;
            int componentIndex = -1;

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == target) targetIndex = i;
                if (components[i] == componentToMove) componentIndex = i;
            }

            if (targetIndex == -1 || componentIndex == -1) return;

            // Calculate how many times we need to move up or down
            int desiredIndex = targetIndex + 1;
            while (componentIndex > desiredIndex)
            {
                ComponentUtility.MoveComponentUp(componentToMove);
                componentIndex--;
            }
            while (componentIndex < desiredIndex)
            {
                ComponentUtility.MoveComponentDown(componentToMove);
                componentIndex++;
            }
        }

        private static void AssignChildObject(SerializedObject so, Transform parent, string propertyName, string childName)
        {
            Transform child = parent.Find(childName);
            SerializedProperty prop = so.FindProperty(propertyName);
            if (prop != null)
            {
                prop.objectReferenceValue = child != null ? child.gameObject : null;
            }
        }
    }
}