using System.IO;
using UnityEditor;
using UnityEngine;

namespace Facility.Generation.Editor
{
    public class RoomDataBatchCreator : EditorWindow
    {
        private string outputPath = "Assets/ScriptableObjects/Rooms";
        private Vector2 scrollPosition;

        [MenuItem("Simoxus/Rooms/Batch Create RoomData", false, 2)]
        private static void ShowWindow()
        {
            RoomDataBatchCreator window = GetWindow<RoomDataBatchCreator>("RoomData Batch Creator");
            window.minSize = new Vector2(400, 200);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("RoomData Batch Creator", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // Output Path Selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output Path:", GUILayout.Width(80));
            outputPath = EditorGUILayout.TextField(outputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("Select Output Folder", "Assets", "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.StartsWith(Application.dataPath))
                    {
                        outputPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Path", "Please select a folder within the Assets directory.", "OK");
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Selection Info
            GameObject[] selectedObjects = Selection.gameObjects;
            EditorGUILayout.LabelField($"Selected GameObjects: {selectedObjects.Length}", EditorStyles.helpBox);

            EditorGUILayout.Space(10);

            // Preview of what will be created
            if (selectedObjects.Length > 0)
            {
                EditorGUILayout.LabelField("Preview:", EditorStyles.boldLabel);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                foreach (GameObject obj in selectedObjects)
                {
                    string roomID = obj.name;
                    string roomName = FormatRoomName(roomID);
                    RoomLayout layout = DetermineRoomLayout(roomID);

                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField($"GameObject: {obj.name}");
                    EditorGUILayout.LabelField($"  → Room ID: {roomID}");
                    EditorGUILayout.LabelField($"  → Room Name: {roomName}");
                    EditorGUILayout.LabelField($"  → Layout: {layout}");
                    EditorGUILayout.LabelField($"  → File: RoomData_{roomID}.asset");
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.Space(10);

            // Create Button
            GUI.enabled = selectedObjects.Length > 0;
            if (GUILayout.Button("Create RoomData ScriptableObjects", GUILayout.Height(30)))
            {
                CreateRoomDataAssets(selectedObjects);
            }
            GUI.enabled = true;

            EditorGUILayout.Space(5);
        }

        private void CreateRoomDataAssets(GameObject[] selectedObjects)
        {
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            int successCount = 0;
            int failCount = 0;

            foreach (GameObject obj in selectedObjects)
            {
                try
                {
                    string roomID = obj.name;
                    string fileName = $"RoomData_{roomID}.asset";
                    string fullPath = Path.Combine(outputPath, fileName);

                    // Check if asset already exists
                    if (File.Exists(fullPath))
                    {
                        bool overwrite = EditorUtility.DisplayDialog(
                            "File Exists",
                            $"RoomData for '{roomID}' already exists. Overwrite?",
                            "Overwrite",
                            "Skip"
                        );

                        if (!overwrite)
                        {
                            Debug.Log($"<color=yellow>[RoomData Creator]</color> Skipped existing: {fileName}");
                            continue;
                        }
                    }

                    // Create the RoomData ScriptableObject
                    RoomData roomData = ScriptableObject.CreateInstance<RoomData>();

                    // Set basic properties using SerializedObject to access private fields
                    SerializedObject so = new SerializedObject(roomData);

                    so.FindProperty("roomID").stringValue = roomID;
                    so.FindProperty("roomName").stringValue = FormatRoomName(roomID);
                    so.FindProperty("description").stringValue = $"Auto-generated room data for {roomID}";

                    // Determine and set room layout
                    RoomLayout layout = DetermineRoomLayout(roomID);
                    so.FindProperty("roomLayout").enumValueIndex = (int)layout;

                    // Set required/unique flags for containment rooms
                    bool isContainment = roomID.Contains("cont-");
                    so.FindProperty("isRequired").boolValue = isContainment;
                    so.FindProperty("isUnique").boolValue = isContainment;

                    // Set default spawn weight
                    if (!isContainment)
                    {
                        so.FindProperty("spawnWeight").floatValue = 1f;
                    }

                    // Set default exit orientations based on layout
                    SetDefaultExits(so, layout);

                    // Set up Addressable reference for the room prefab
                    SetPrefabReference(so, obj.name);

                    so.ApplyModifiedProperties();

                    // Save the asset
                    AssetDatabase.CreateAsset(roomData, fullPath);
                    successCount++;

                    Debug.Log($"<color=green>[RoomData Creator]</color> Created: {fileName}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"<color=red>[RoomData Creator]</color> Failed to create RoomData for '{obj.name}': {e.Message}");
                    failCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[RoomData Creator]</color> Batch creation complete: {successCount} created, {failCount} failed");
        }

        private string FormatRoomName(string roomID)
        {
            // Split by hyphens
            string[] parts = roomID.Split('-');

            if (parts.Length < 2) return roomID; // Fallback if format is unexpected

            // Special handling for containment rooms
            // Format: cont-lcz-012 → SCP-012 Containment
            if (roomID.Contains("cont-"))
            {
                // Try to find the SCP number (usually the last part)
                for (int i = parts.Length - 1; i >= 0; i--)
                {
                    if (int.TryParse(parts[i], out int scpNumber))
                    {
                        return $"SCP-{parts[i]} Containment";
                    }
                }
                // Fallback if no number found
                return "SCP Containment";
            }

            // Get zone prefix (lcz, hcz, ez, etc.)
            string zone = parts[0].ToUpper();

            // Determine room type
            string roomType = "";
            string number = "";

            // Check for common patterns
            if (roomID.Contains("hall") && !roomID.Contains("corner") && !roomID.Contains("cross") && !roomID.Contains("junction"))
            {
                roomType = "Hallway";
            }
            else if (roomID.Contains("corner"))
            {
                roomType = "Corner";
            }
            else if (roomID.Contains("deadend"))
            {
                roomType = "Dead End";
            }
            else if (roomID.Contains("cross"))
            {
                roomType = "Crossroads";
            }
            else if (roomID.Contains("junction"))
            {
                roomType = "Junction";
            }
            else if (roomID.Contains("cp-"))
            {
                roomType = "Checkpoint";
            }
            else
            {
                // Generic room type from second part
                roomType = CapitalizeFirst(parts[1]);
            }

            // Try to extract number from last part
            if (parts.Length > 0)
            {
                string lastPart = parts[parts.Length - 1];
                if (int.TryParse(lastPart, out int num))
                {
                    number = num.ToString();
                }
            }

            // Build final name
            string result = $"{zone} {roomType}";
            if (!string.IsNullOrEmpty(number))
            {
                result += $" {number}";
            }

            return result;
        }

        private string CapitalizeFirst(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        private RoomLayout DetermineRoomLayout(string roomID)
        {
            string lower = roomID.ToLower();

            // Check for specific keywords
            if (lower.Contains("deadend"))
                return RoomLayout.DeadEnd;

            if (lower.Contains("corner"))
                return RoomLayout.Corner;

            if (lower.Contains("cross"))
                return RoomLayout.Crossroads;

            if (lower.Contains("junction"))
                return RoomLayout.Junction;

            if (lower.Contains("cp-"))
                return RoomLayout.Checkpoint;

            // Check for hallway (only if it contains "hall" and no other keywords)
            if (lower.Contains("hall"))
                return RoomLayout.Hallway;

            // Default fallback
            return RoomLayout.DeadEnd;
        }

        private void SetPrefabReference(SerializedObject so, string prefabName)
        {
            // Get the Addressables settings
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning($"<color=yellow>[RoomData Creator]</color> Addressables settings not found. Is Addressables configured?");
                return;
            }

            // Search through all addressable entries
            string foundGuid = null;
            string foundAddress = null;

            foreach (var group in settings.groups)
            {
                if (group == null) continue;

                foreach (var entry in group.entries)
                {
                    // Check if the entry's address or asset name matches
                    if (entry.address.Equals(prefabName, System.StringComparison.OrdinalIgnoreCase) ||
                        System.IO.Path.GetFileNameWithoutExtension(entry.AssetPath).Equals(prefabName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        foundGuid = entry.guid;
                        foundAddress = entry.address;
                        break;
                    }
                }

                if (foundGuid != null) break;
            }

            if (foundGuid == null)
            {
                Debug.LogWarning($"<color=yellow>[RoomData Creator]</color> No addressable asset found with name '{prefabName}'");
                return;
            }

            // Set the AssetReferenceGameObject
            SerializedProperty refProp = so.FindProperty("roomPrefabReference");
            if (refProp != null)
            {
                // Set the GUID on the AssetReference
                SerializedProperty guidProp = refProp.FindPropertyRelative("m_AssetGUID");
                if (guidProp != null)
                {
                    guidProp.stringValue = foundGuid;
                    Debug.Log($"<color=green>[RoomData Creator]</color> Set addressable reference for '{prefabName}' (Address: {foundAddress})");
                }
            }
        }

        private void SetDefaultExits(SerializedObject so, RoomLayout layout)
        {
            // Set default exit orientations based on layout type
            // These are common patterns - adjust as needed for your specific rooms

            switch (layout)
            {
                case RoomLayout.DeadEnd:
                    // Typically one exit (North)
                    so.FindProperty("defaultExitNorth").boolValue = true;
                    so.FindProperty("defaultExitEast").boolValue = false;
                    so.FindProperty("defaultExitSouth").boolValue = false;
                    so.FindProperty("defaultExitWest").boolValue = false;
                    break;

                case RoomLayout.Hallway:
                    // Two opposite exits (North and South)
                    so.FindProperty("defaultExitNorth").boolValue = true;
                    so.FindProperty("defaultExitEast").boolValue = false;
                    so.FindProperty("defaultExitSouth").boolValue = true;
                    so.FindProperty("defaultExitWest").boolValue = false;
                    break;

                case RoomLayout.Corner:
                    // Two adjacent exits (North and East)
                    so.FindProperty("defaultExitNorth").boolValue = true;
                    so.FindProperty("defaultExitEast").boolValue = true;
                    so.FindProperty("defaultExitSouth").boolValue = false;
                    so.FindProperty("defaultExitWest").boolValue = false;
                    break;

                case RoomLayout.Junction:
                    // Three exits (North, East, South)
                    so.FindProperty("defaultExitNorth").boolValue = true;
                    so.FindProperty("defaultExitEast").boolValue = true;
                    so.FindProperty("defaultExitSouth").boolValue = true;
                    so.FindProperty("defaultExitWest").boolValue = false;
                    break;

                case RoomLayout.Crossroads:
                    // Four exits (all directions)
                    so.FindProperty("defaultExitNorth").boolValue = true;
                    so.FindProperty("defaultExitEast").boolValue = true;
                    so.FindProperty("defaultExitSouth").boolValue = true;
                    so.FindProperty("defaultExitWest").boolValue = true;
                    break;

                case RoomLayout.Checkpoint:
                    // Two opposite exits (North and South, like a hallway)
                    so.FindProperty("defaultExitNorth").boolValue = true;
                    so.FindProperty("defaultExitEast").boolValue = false;
                    so.FindProperty("defaultExitSouth").boolValue = true;
                    so.FindProperty("defaultExitWest").boolValue = false;
                    break;
            }
        }
    }
}