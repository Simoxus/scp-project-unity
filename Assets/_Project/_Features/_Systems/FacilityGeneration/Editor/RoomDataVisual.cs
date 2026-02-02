using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Facility.Generation.EditorTools
{
    public class RoomDataVisualizer : EditorWindow
    {
        private RoomData selectedRoomData;
        private GameObject previewInstance;
        private Vector2 scrollPosition;

        private bool showGridOverlay = true;
        private bool showExitGizmos = true;
        private bool showAnchorPoint = true;
        private bool showOccupiedCells = true;
        private bool showRotationPreview = false;

        private int previewRotation = 0;
        private float cellSize = 12.603f;
        private float gizmoAlpha = 0.7f;

        private Camera sceneCamera;
        private Vector3 lastCameraPosition;
        private Quaternion lastCameraRotation;

        [MenuItem("Simoxus/Rooms/Room Data Visualizer", false, 2)]
        public static void ShowWindow()
        {
            var window = GetWindow<RoomDataVisualizer>("Room Visualizer");
            window.minSize = new Vector2(400, 600);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.update -= OnEditorUpdate;
            CleanupPreview();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawRoomDataSelection();

            if (selectedRoomData != null)
            {
                EditorGUILayout.Space(10);
                DrawRoomInfo();

                EditorGUILayout.Space(10);
                DrawVisualizationOptions();

                EditorGUILayout.Space(10);
                DrawPreviewControls();

                EditorGUILayout.Space(10);
                DrawRoomConfiguration();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Room Data Visualizer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select a RoomData asset to visualize its configuration, exits, anchor offset, and occupied cells. " +
                "The preview updates in the Scene view with visual gizmos.",
                MessageType.Info
            );
        }

        private void DrawRoomDataSelection()
        {
            EditorGUI.BeginChangeCheck();

            RoomData newRoomData = (RoomData)EditorGUILayout.ObjectField(
                "Room Data",
                selectedRoomData,
                typeof(RoomData),
                false
            );

            if (EditorGUI.EndChangeCheck() && newRoomData != selectedRoomData)
            {
                selectedRoomData = newRoomData;
                CleanupPreview();
                if (selectedRoomData != null)
                {
                    LoadPreview();
                }
            }
        }

        private void DrawRoomInfo()
        {
            EditorGUILayout.LabelField("Room Information", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("Room Name", selectedRoomData.RoomName);
                EditorGUILayout.TextField("Room ID", selectedRoomData.RoomID);
                EditorGUILayout.EnumPopup("Layout", selectedRoomData.Layout);
                EditorGUILayout.IntField("Connection Count", selectedRoomData.ConnectionCount);
            }

            EditorGUILayout.Space(5);

            if (selectedRoomData.IsLarge)
            {
                EditorGUILayout.LabelField("Large Room Configuration", EditorStyles.boldLabel);

                Vector2Int[] occupiedCells = selectedRoomData.GetOccupiedCells();
                int totalCells = occupiedCells.Length;

                EditorGUILayout.HelpBox($"This room occupies {totalCells} grid cells", MessageType.Info);
            }
        }

        private void DrawVisualizationOptions()
        {
            EditorGUILayout.LabelField("Visualization Options", EditorStyles.boldLabel);

            cellSize = EditorGUILayout.Slider("Cell Size", cellSize, 5f, 20f);
            gizmoAlpha = EditorGUILayout.Slider("Gizmo Alpha", gizmoAlpha, 0.1f, 1f);

            EditorGUILayout.Space(5);

            showGridOverlay = EditorGUILayout.Toggle("Show Grid Overlay", showGridOverlay);
            showExitGizmos = EditorGUILayout.Toggle("Show Exit Gizmos", showExitGizmos);
            showAnchorPoint = EditorGUILayout.Toggle("Show Anchor Point", showAnchorPoint);
            showOccupiedCells = EditorGUILayout.Toggle("Show Occupied Cells", showOccupiedCells);
            showRotationPreview = EditorGUILayout.Toggle("Show Rotation Preview", showRotationPreview);

            SceneView.RepaintAll();
        }

        private void DrawPreviewControls()
        {
            EditorGUILayout.LabelField("Preview Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Rotate -90°"))
            {
                previewRotation = (previewRotation - 1 + 4) % 4;
                UpdatePreviewRotation();
            }
            if (GUILayout.Button("Reset Rotation"))
            {
                previewRotation = 0;
                UpdatePreviewRotation();
            }
            if (GUILayout.Button("Rotate +90°"))
            {
                previewRotation = (previewRotation + 1) % 4;
                UpdatePreviewRotation();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"Current Rotation: {previewRotation * 90}°");

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload Preview"))
            {
                CleanupPreview();
                LoadPreview();
            }
            if (GUILayout.Button("Focus in Scene"))
            {
                FocusPreviewInScene();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRoomConfiguration()
        {
            EditorGUILayout.LabelField("Exit Configuration", EditorStyles.boldLabel);

            bool[] exits = selectedRoomData.GetDefaultExitPattern();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Toggle("North Exit", exits[0]);
                EditorGUILayout.Toggle("East Exit", exits[1]);
                EditorGUILayout.Toggle("South Exit", exits[2]);
                EditorGUILayout.Toggle("West Exit", exits[3]);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Exit pattern is based on default (0°) orientation. " +
                "Rotate the preview to see how exits align at different angles.",
                MessageType.Info
            );

            if (selectedRoomData.HasCustomOffset)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Custom Offset", EditorStyles.boldLabel);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Vector3Field("Position Offset", selectedRoomData.RoomOffset);
                    EditorGUILayout.FloatField("Rotation Offset", selectedRoomData.RotationOffset);
                }
            }
        }

        private void LoadPreview()
        {
            if (selectedRoomData?.RoomPrefabReference == null) return;

            // Load the prefab synchronously for editor preview
            var prefabAsset = selectedRoomData.RoomPrefabReference.Asset;
            if (prefabAsset == null)
            {
                selectedRoomData.RoomPrefabReference.LoadAssetAsync();
                return;
            }

            Vector3 previewPosition = Vector3.zero;
            previewInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset);
            previewInstance.transform.position = previewPosition;
            previewInstance.transform.rotation = Quaternion.identity;
            previewInstance.hideFlags = HideFlags.DontSave;

            UpdatePreviewRotation();
            FocusPreviewInScene();
        }

        private void CleanupPreview()
        {
            if (previewInstance != null)
            {
                DestroyImmediate(previewInstance);
                previewInstance = null;
            }
        }

        private void UpdatePreviewRotation()
        {
            if (previewInstance == null) return;

            float angle = previewRotation * 90f;
            if (selectedRoomData.HasCustomOffset)
            {
                angle += selectedRoomData.RotationOffset;
            }

            previewInstance.transform.rotation = Quaternion.Euler(0, angle, 0);
            SceneView.RepaintAll();
        }

        private void FocusPreviewInScene()
        {
            if (previewInstance == null) return;

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                Bounds bounds = CalculatePreviewBounds();
                sceneView.Frame(bounds, false);
            }
        }

        private Bounds CalculatePreviewBounds()
        {
            if (previewInstance == null)
            {
                return new Bounds(Vector3.zero, Vector3.one * cellSize);
            }

            Renderer[] renderers = previewInstance.GetComponentsInChildren<Renderer>();
            Bounds bounds;

            if (renderers.Length > 0)
            {
                bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }
            else
            {
                bounds = new Bounds(previewInstance.transform.position, Vector3.one * cellSize);
            }

            // Expand bounds to include all occupied grid cells
            Vector2Int[] occupiedCells = selectedRoomData.GetOccupiedCells();
            foreach (var cellOffset in occupiedCells)
            {
                Vector3 rotatedOffset = Quaternion.Euler(0, previewRotation * 90f, 0) * new Vector3(cellOffset.x * cellSize, 0, cellOffset.y * cellSize);

                Vector3 cellWorldPos = previewInstance.transform.position + rotatedOffset;
                bounds.Encapsulate(new Bounds(cellWorldPos, Vector3.one * cellSize));
            }

            return bounds;
        }

        private void OnEditorUpdate()
        {
            if (selectedRoomData?.RoomPrefabReference != null && previewInstance == null)
            {
                LoadPreview();
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (selectedRoomData == null || previewInstance == null) return;

            DrawVisualizationGizmos();

            Handles.BeginGUI();
            DrawSceneOverlay();
            Handles.EndGUI();
        }

        private void DrawVisualizationGizmos()
        {
            Vector3 basePosition = previewInstance.transform.position;

            if (showGridOverlay)
            {
                DrawGridOverlay(basePosition);
            }

            if (showOccupiedCells && selectedRoomData.IsLarge)
            {
                DrawOccupiedCells(basePosition);
            }

            if (showAnchorPoint)
            {
                DrawAnchorPointGizmo(basePosition);
            }

            if (showExitGizmos)
            {
                DrawExitGizmos(basePosition);
            }

            if (showRotationPreview)
            {
                DrawRotationIndicator(basePosition);
            }
        }

        private void DrawGridOverlay(Vector3 basePosition)
        {
            // Determine grid radius based on occupied cells or default for small rooms
            int gridRadius = 3;
            if (selectedRoomData.IsLarge)
            {
                Vector2Int[] occupiedCells = selectedRoomData.GetOccupiedCells();
                foreach (var cell in occupiedCells)
                {
                    gridRadius = Mathf.Max(gridRadius, Mathf.Abs(cell.x) + 1, Mathf.Abs(cell.y) + 1);
                }
            }

            Color gridColor = new Color(0.5f, 0.5f, 0.5f, gizmoAlpha * 0.3f);
            Handles.color = gridColor;

            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int z = -gridRadius; z <= gridRadius; z++)
                {
                    Vector3 cellPos = basePosition + new Vector3(x * cellSize, 0, z * cellSize);
                    DrawWireCube(cellPos, new Vector3(cellSize, 0.1f, cellSize));
                }
            }
        }

        private void DrawOccupiedCells(Vector3 basePosition)
        {
            Vector2Int[] occupiedCells = selectedRoomData.GetOccupiedCells();

            Color occupiedColor = new Color(0, 1, 0, gizmoAlpha * 0.4f);
            Handles.color = occupiedColor;

            foreach (var cellOffset in occupiedCells)
            {
                // Rotate the cell offset based on current rotation
                Vector3 rotatedOffset = new Vector3(cellOffset.x * cellSize, 0, cellOffset.y * cellSize);
                rotatedOffset = Quaternion.Euler(0, previewRotation * 90f, 0) * rotatedOffset;

                // Add the rotated offset directly (cells are already relative to anchor)
                Vector3 cellPos = basePosition + rotatedOffset;
                DrawFilledCube(cellPos, new Vector3(cellSize * 0.95f, 0.1f, cellSize * 0.95f));

                // Draw cell coordinate label
                Handles.Label(cellPos + Vector3.up * 0.2f, $"({cellOffset.x}, {cellOffset.y})",
                    new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = Color.white },
                        alignment = TextAnchor.MiddleCenter
                    });
            }

            // Draw the boundary
            Color boundaryColor = new Color(1, 0, 1, gizmoAlpha);
            Handles.color = boundaryColor;

            // Calculate bounds from occupied cells
            int minX = occupiedCells.Min(c => c.x);
            int maxX = occupiedCells.Max(c => c.x);
            int minZ = occupiedCells.Min(c => c.y);
            int maxZ = occupiedCells.Max(c => c.y);

            Vector3 size = new Vector3(
                (maxX - minX + 1) * cellSize,
                0.2f,
                (maxZ - minZ + 1) * cellSize
            );

            // Rotate the center offset
            Vector3 centerOffset = new Vector3(
                (minX + maxX) * cellSize * 0.5f,
                0,
                (minZ + maxZ) * cellSize * 0.5f
            );
            centerOffset = Quaternion.Euler(0, previewRotation * 90f, 0) * centerOffset;

            Vector3 boundaryCenter = basePosition + centerOffset;
            DrawWireCube(boundaryCenter, size);
        }


        private void DrawAnchorPointGizmo(Vector3 basePosition)
        {
            Color anchorColor = new Color(1, 1, 0, gizmoAlpha);
            Handles.color = anchorColor;

            float anchorSize = cellSize * 0.2f;
            Handles.SphereHandleCap(0, basePosition + Vector3.up * 0.1f, Quaternion.identity, anchorSize, EventType.Repaint);

            // Draw anchor label
            Handles.Label(basePosition + Vector3.up * (anchorSize + 0.3f), "ANCHOR",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.yellow },
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                });

            // Draw coordinate axes from anchor
            Handles.color = Color.red;
            Handles.DrawLine(basePosition, basePosition + Vector3.right * cellSize * 0.5f);
            Handles.color = Color.blue;
            Handles.DrawLine(basePosition, basePosition + Vector3.forward * cellSize * 0.5f);
        }

        private void DrawExitGizmos(Vector3 basePosition)
        {
            bool[] defaultExits = selectedRoomData.GetDefaultExitPattern();
            Color exitColor = new Color(0, 1, 0, gizmoAlpha);

            float exitSize = cellSize * 0.2f;
            float offset = cellSize * 0.45f;

            for (int i = 0; i < 4; i++)
            {
                // Rotate the exit direction based on preview rotation
                int rotatedDirection = (i + previewRotation) % 4;

                if (!defaultExits[i]) continue;

                Vector3 exitOffset = rotatedDirection switch
                {
                    0 => Vector3.forward * offset,   // North
                    1 => Vector3.right * offset,     // East
                    2 => Vector3.back * offset,      // South
                    3 => Vector3.left * offset,      // West
                    _ => Vector3.zero
                };

                Vector3 exitPos = basePosition + exitOffset + Vector3.up * 0.15f;

                Handles.color = exitColor;
                Handles.CubeHandleCap(0, exitPos, Quaternion.identity, exitSize, EventType.Repaint);

                // Draw direction arrow
                Handles.color = new Color(0, 1, 0, gizmoAlpha * 0.7f);
                Vector3 arrowStart = exitPos;
                Vector3 arrowEnd = exitPos + exitOffset.normalized * (exitSize * 1.5f);
                Handles.DrawLine(arrowStart, arrowEnd);
                Handles.ConeHandleCap(0, arrowEnd, Quaternion.LookRotation(exitOffset), exitSize * 0.5f, EventType.Repaint);

                // Label the direction
                string directionName = ((Direction)rotatedDirection).ToString();
                Handles.Label(exitPos + Vector3.up * 0.3f, directionName,
                    new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = Color.green },
                        alignment = TextAnchor.MiddleCenter
                    });
            }
        }

        private void DrawRotationIndicator(Vector3 basePosition)
        {
            Color rotationColor = new Color(1, 1, 1, gizmoAlpha * 0.5f);
            Handles.color = rotationColor;

            float radius = cellSize * 0.8f;
            Handles.DrawWireDisc(basePosition + Vector3.up * 0.05f, Vector3.up, radius);

            // Draw rotation angle arc
            float angle = previewRotation * 90f;
            Handles.color = new Color(0, 1, 1, gizmoAlpha);
            Handles.DrawWireArc(basePosition + Vector3.up * 0.05f, Vector3.up, Vector3.forward, angle, radius);

            // Draw forward direction indicator
            Vector3 forwardDir = previewInstance.transform.forward;
            Handles.color = Color.cyan;
            Handles.ArrowHandleCap(0, basePosition + Vector3.up * 0.05f,
                Quaternion.LookRotation(forwardDir), radius * 0.8f, EventType.Repaint);
        }

        private void DrawSceneOverlay()
        {
            GUILayout.BeginArea(new Rect(10, 10, 250, 200));
            GUILayout.BeginVertical(EditorStyles.helpBox);

            GUILayout.Label("Room Data Visualizer", EditorStyles.boldLabel);
            GUILayout.Label($"Room: {selectedRoomData.RoomName}");
            GUILayout.Label($"Rotation: {previewRotation * 90}°");

            if (selectedRoomData.IsLarge)
            {
                Vector2Int[] cells = selectedRoomData.GetOccupiedCells();
                GUILayout.Label($"Cells: {cells.Length}");
            }

            GUILayout.Space(5);
            GUILayout.Label("Controls:", EditorStyles.miniBoldLabel);
            GUILayout.Label("Q/E - Rotate");
            GUILayout.Label("F - Focus");

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawWireCube(Vector3 center, Vector3 size)
        {
            Vector3 halfSize = size / 2f;

            Vector3[] corners = new Vector3[8];
            corners[0] = center + new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
            corners[1] = center + new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
            corners[2] = center + new Vector3(halfSize.x, -halfSize.y, halfSize.z);
            corners[3] = center + new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
            corners[4] = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            corners[5] = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            corners[6] = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            corners[7] = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            // Bottom face
            Handles.DrawLine(corners[0], corners[1]);
            Handles.DrawLine(corners[1], corners[2]);
            Handles.DrawLine(corners[2], corners[3]);
            Handles.DrawLine(corners[3], corners[0]);

            // Top face
            Handles.DrawLine(corners[4], corners[5]);
            Handles.DrawLine(corners[5], corners[6]);
            Handles.DrawLine(corners[6], corners[7]);
            Handles.DrawLine(corners[7], corners[4]);

            // Vertical edges
            Handles.DrawLine(corners[0], corners[4]);
            Handles.DrawLine(corners[1], corners[5]);
            Handles.DrawLine(corners[2], corners[6]);
            Handles.DrawLine(corners[3], corners[7]);
        }

        private void DrawFilledCube(Vector3 center, Vector3 size)
        {
            DrawWireCube(center, size);

            Vector3 halfSize = size / 2f;

            // Draw filled top face
            Vector3[] topVerts = new Vector3[4];
            topVerts[0] = center + new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
            topVerts[1] = center + new Vector3(halfSize.x, halfSize.y, -halfSize.z);
            topVerts[2] = center + new Vector3(halfSize.x, halfSize.y, halfSize.z);
            topVerts[3] = center + new Vector3(-halfSize.x, halfSize.y, halfSize.z);

            Handles.DrawSolidRectangleWithOutline(topVerts, Handles.color, Color.clear);
        }
    }
}
#endif