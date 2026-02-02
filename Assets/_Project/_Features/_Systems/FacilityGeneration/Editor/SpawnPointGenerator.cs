using System;
using UnityEditor;
using UnityEngine;

namespace Facility.Generation.EditorTools
{
    public static class SpawnPointGenerator
    {
        [MenuItem("Simoxus/Rooms/Generate Spawn Points", false, 1)]
        public static void GenerateSpawnsForSelection()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select the Room GameObjects first.", "OK");
                return;
            }

            int createdCount = 0;
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Generate Spawn Points");

            foreach (GameObject root in selectedObjects)
            {
                // Find the specific folder for spawns
                Transform spawnsParent = root.transform.Find("Spawns");
                if (spawnsParent == null) continue;

                foreach (SpawnType type in Enum.GetValues(typeof(SpawnType)))
                {
                    if (type == SpawnType.Vent)
                    {
                        // Searches for objects named "vent" and places "VentSpawnPoint_#" there
                        createdCount += HandleVentSpawns(root, spawnsParent);
                    }
                    else
                    {
                        // Creates "PlayerSpawnPoint" or "EntitySpawnPoint" at center
                        string formalName = $"{type}SpawnPoint";
                        createdCount += CreateSpawnPoint(spawnsParent, type, formalName, Vector3.zero);
                    }
                }
            }

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
            Debug.Log($"<color=cyan><b>Spawn Generator:</b></color> Generated {createdCount} points using formal naming.");
        }

        private static int HandleVentSpawns(GameObject root, Transform spawnsParent)
        {
            int count = 0;
            Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);
            int ventIndex = 1;

            foreach (var child in allChildren)
            {
                // Check if the child is named "vent" (ignoring case)
                if (child.name.Equals("vent", StringComparison.OrdinalIgnoreCase))
                {
                    string name = $"VentSpawnPoint ({ventIndex})";

                    if (spawnsParent.Find(name))
                    {
                        ventIndex++;
                        continue;
                    }

                    // Convert the vent's world position to a local position relative to the "Spawns" folder
                    Vector3 localPos = spawnsParent.InverseTransformPoint(child.position);

                    count += CreateSpawnPoint(spawnsParent, SpawnType.Vent, name, localPos);
                    ventIndex++;
                }
            }
            return count;
        }

        private static int CreateSpawnPoint(Transform parent, SpawnType type, string name, Vector3 localPos)
        {
            if (parent.Find(name)) return 0;

            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Create Spawn Point");

            go.transform.SetParent(parent);
            go.transform.localPosition = localPos;
            go.transform.localRotation = Quaternion.identity;

            SpawnPoint component = go.AddComponent<SpawnPoint>();

            // Set the private 'type' field via SerializedObject
            SerializedObject so = new SerializedObject(component);
            so.FindProperty("type").enumValueIndex = (int)type;
            so.ApplyModifiedProperties();

            return 1;
        }

        [MenuItem("Simoxus/Rooms/Generate Spawn Points", true)]
        private static bool ValidateGenerateSpawns() => Selection.activeGameObject != null;
    }
}