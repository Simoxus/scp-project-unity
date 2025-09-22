// File: MapLayoutEditor.cs
// This file defines the custom Unity Editor window for creating and editing map layouts.
// This script must be placed in a folder named "Editor" for Unity to recognize it.

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class MapLayoutEditor : EditorWindow
{
    // The currently selected MapLayoutData asset.
    private MapLayoutData mapLayoutAsset;
    // The currently selected grid position.
    private Vector2Int selectedGridPosition = new Vector2Int(-1, -1);
    // The scroll position for the editor window.
    private Vector2 scrollPosition;

    // A reference to the currently selected placement.
    private Placement selectedPlacement;

    // Add a menu item to open the editor window.
    [MenuItem("Window/Map Layout Editor")]
    public static void ShowWindow()
    {
        // Get or create the window. Explicitly referencing the static method.
        EditorWindow.GetWindow<MapLayoutEditor>("Map Layout Editor");
    }

    private void OnGUI()
    {
        // Add a scroll view for the entire window to handle large layouts.
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Map Layout Asset Field
        EditorGUILayout.LabelField("Map Layout Asset", EditorStyles.boldLabel);
        mapLayoutAsset = (MapLayoutData)EditorGUILayout.ObjectField(
            "Map Layout",
            mapLayoutAsset,
            typeof(MapLayoutData),
            false
        );

        // If no asset is selected, disable the rest of the UI and show a message.
        if (mapLayoutAsset == null)
        {
            EditorGUILayout.HelpBox("Please select or create a Map Layout Data asset.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        // Display the name of the asset.
        EditorGUILayout.LabelField($"Editing: {mapLayoutAsset.name}");

        // Horizontal separator line.
        EditorGUILayout.Space();

        // Grid Size Configuration
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        float newWidth = EditorGUILayout.FloatField("Grid Width", mapLayoutAsset.gridWidth);
        float newHeight = EditorGUILayout.FloatField("Grid Height", mapLayoutAsset.gridHeight);
        if (EditorGUI.EndChangeCheck())
        {
            // Update the dimensions and mark the asset as dirty.
            mapLayoutAsset.gridWidth = newWidth;
            mapLayoutAsset.gridHeight = newHeight;
            EditorUtility.SetDirty(mapLayoutAsset);
        }

        // Clear All Placements button.
        EditorGUILayout.Space();
        if (GUILayout.Button("Clear All Placements"))
        {
            // Display a confirmation dialog before clearing all data.
            if (EditorUtility.DisplayDialog("Clear Placements", "Are you sure you want to clear all placements?", "Yes", "No"))
            {
                mapLayoutAsset.placements.Clear();
                selectedGridPosition = new Vector2Int(-1, -1);
                selectedPlacement = null;
                EditorUtility.SetDirty(mapLayoutAsset);
            }
        }

        // Horizontal separator line.
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Begin a horizontal layout to place the grid and inspector side-by-side.
        EditorGUILayout.BeginHorizontal();

        // The main grid drawing logic on the left side.
        DrawGrid();

        // Add a vertical layout group for the placement inspector on the right.
        EditorGUILayout.BeginVertical(GUILayout.Width(300));

        // "Selected Placement" section.
        DrawSelectedPlacementInspector();

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();

        // Ensure the editor window repaints when changes are made.
        if (GUI.changed)
        {
            // Explicitly call Repaint on this instance of the EditorWindow.
            this.Repaint();
        }
    }

    private void DrawGrid()
    {
        float cellSize = 40f;
        float padding = 2f;
        float gridTotalWidth = mapLayoutAsset.gridWidth * (cellSize + padding) + padding;
        float gridTotalHeight = mapLayoutAsset.gridHeight * (cellSize + padding) + padding;

        Rect gridArea = GUILayoutUtility.GetRect(gridTotalWidth, gridTotalHeight, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

        GUI.BeginGroup(gridArea);

        // Style for the main cell buttons
        GUIStyle cellStyle = new GUIStyle(GUI.skin.box);
        cellStyle.alignment = TextAnchor.MiddleCenter;
        cellStyle.normal.textColor = Color.white;
        cellStyle.active.textColor = Color.white;

        // Style for the top-left indicator square
        GUIStyle indicatorStyle = new GUIStyle(GUI.skin.box);
        indicatorStyle.fixedWidth = 8f;
        indicatorStyle.fixedHeight = 8f;

        for (int y = (int)mapLayoutAsset.gridHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < (int)mapLayoutAsset.gridWidth; x++)
            {
                Rect cellRect = new Rect(x * (cellSize + padding), ((mapLayoutAsset.gridHeight - 1) - y) * (cellSize + padding), cellSize, cellSize);

                Vector2Int cellPosition = new Vector2Int(x, y);
                Placement placement = null;

                if (mapLayoutAsset.placements.ContainsKey(cellPosition))
                {
                    placement = mapLayoutAsset.placements[cellPosition];
                }

                Color cellColor;
                string cellText = "";

                if (cellPosition == selectedGridPosition)
                {
                    cellColor = new Color(0.2f, 0.4f, 0.8f);
                }
                else if (placement != null)
                {
                    cellColor = new Color(0.4f, 0.4f, 0.4f);
                    cellText = GetRoomTypeAbbreviation(placement.roomType);
                }
                else
                {
                    cellColor = new Color(0.6f, 0.6f, 0.6f);
                }

                GUI.backgroundColor = cellColor;

                if (GUI.Button(cellRect, cellText, cellStyle))
                {
                    // Right-click to remove placement.
                    if (Event.current.button == 1 && placement != null)
                    {
                        mapLayoutAsset.placements.Remove(cellPosition);
                        selectedGridPosition = new Vector2Int(-1, -1);
                        selectedPlacement = null;
                        EditorUtility.SetDirty(mapLayoutAsset);
                        Event.current.Use();
                    }
                    // Left-click to select or create a new placement.
                    else if (Event.current.button == 0)
                    {
                        selectedGridPosition = cellPosition;
                        if (!mapLayoutAsset.placements.ContainsKey(selectedGridPosition))
                        {
                            mapLayoutAsset.placements.Add(selectedGridPosition, new Placement());
                        }
                        selectedPlacement = mapLayoutAsset.placements[selectedGridPosition];
                        EditorUtility.SetDirty(mapLayoutAsset);
                    }
                }

                // Draw the top-left indicator box if the cell has a placement.
                if (placement != null)
                {
                    GUI.backgroundColor = new Color(0.6f, 0.6f, 0.6f); // Lighter gray for the indicator
                    Rect indicatorRect = new Rect(cellRect.x + 2, cellRect.y + 2, indicatorStyle.fixedWidth, indicatorStyle.fixedHeight);
                    GUI.Box(indicatorRect, "", indicatorStyle);
                }
            }
        }

        GUI.EndGroup();
    }

    private string GetRoomTypeAbbreviation(RoomType type)
    {
        // Simple switch to get the abbreviation for each room type.
        switch (type)
        {
            case RoomType.Containment: return "CN";
            case RoomType.Checkpoint: return "CP";
            case RoomType.DeadEnd: return "DE";
            case RoomType.TwoWay: return "2W";
            case RoomType.ThreeWay: return "3W";
            case RoomType.FourWay: return "4W";
            case RoomType.Corner: return "C";
            case RoomType.Custom: return "CS";
        }
        return "";
    }

    private string GetZoneLocationAbbreviation(ZoneLocation location)
    {
        // Simple switch to get the abbreviation for each zone location.
        switch (location)
        {
            case ZoneLocation.SurfaceZone: return "S";
            case ZoneLocation.EntranceZone: return "E";
            case ZoneLocation.HeavyZone: return "H";
            case ZoneLocation.LightZone: return "L";
            case ZoneLocation.Custom: return "CS";
        }
        return "";
    }

    private void DrawSelectedPlacementInspector()
    {
        EditorGUILayout.LabelField("Selected Placement", EditorStyles.boldLabel);

        // Check if a placement is selected.
        if (selectedPlacement != null)
        {
            // Use EditorGUI.BeginChangeCheck to detect changes to properties.
            EditorGUI.BeginChangeCheck();

            // Zone Location dropdown.
            selectedPlacement.zoneLocation = (ZoneLocation)EditorGUILayout.EnumPopup("Zone Location", selectedPlacement.zoneLocation);

            // Room Type dropdown.
            selectedPlacement.roomType = (RoomType)EditorGUILayout.EnumPopup("Room Type", selectedPlacement.roomType);

            // Required Checkbox
            selectedPlacement.isRequired = EditorGUILayout.Toggle("Required", selectedPlacement.isRequired);

            // Allowed Connections section.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Allowed Connections", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            selectedPlacement.allowedConnections.north = GUILayout.Toggle(selectedPlacement.allowedConnections.north, "North", "Button");
            selectedPlacement.allowedConnections.east = GUILayout.Toggle(selectedPlacement.allowedConnections.east, "East", "Button");
            selectedPlacement.allowedConnections.south = GUILayout.Toggle(selectedPlacement.allowedConnections.south, "South", "Button");
            selectedPlacement.allowedConnections.west = GUILayout.Toggle(selectedPlacement.allowedConnections.west, "West", "Button");
            EditorGUILayout.EndHorizontal();

            // Allowed Room Kinds checkboxes.
            EditorGUILayout.LabelField("Allowed Room Types:", EditorStyles.boldLabel);
            // Iterate through all possible room kinds to create a toggle for each.
            foreach (RoomType type in System.Enum.GetValues(typeof(RoomType)))
            {
                bool isAllowed = selectedPlacement.allowedRoomTypes.Contains(type);
                bool newIsAllowed = EditorGUILayout.Toggle($"  {type}", isAllowed);
                if (newIsAllowed != isAllowed)
                {
                    if (newIsAllowed)
                    {
                        selectedPlacement.allowedRoomTypes.Add(type);
                    }
                    else
                    {
                        selectedPlacement.allowedRoomTypes.Remove(type);
                    }
                }
            }

            // Remove This Placement button.
            EditorGUILayout.Space();
            if (GUILayout.Button("Remove This Placement"))
            {
                // Confirmation dialog before removal.
                if (EditorUtility.DisplayDialog("Remove Placement", "Are you sure you want to remove this placement?", "Yes", "No"))
                {
                    mapLayoutAsset.placements.Remove(selectedGridPosition);
                    selectedGridPosition = new Vector2Int(-1, -1);
                    selectedPlacement = null;
                    EditorUtility.SetDirty(mapLayoutAsset);
                }
            }

            // If any of the placement properties changed, mark the asset as dirty.
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(mapLayoutAsset);
            }
        }
        else
        {
            // Show a message if no cell is selected.
            EditorGUILayout.HelpBox("Select a cell on the grid to edit its placement properties.", MessageType.Info);
        }
    }
}
