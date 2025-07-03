using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class GridAlignmentTool : EditorWindow
{
    private enum Axis { X, Y, Z }

    private Axis moveAxis = Axis.X;
    private float spacing = 1.0f;
    private bool sortByName = false;

    private Dictionary<GameObject, Vector3> originalPositions = new();

    [MenuItem("Tools/Production Tools/Grid Align Tool")]
    public static void ShowWindow()
    {
        GetWindow<GridAlignmentTool>("Grid Align Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("Align Selected Objects", EditorStyles.boldLabel);

        moveAxis = (Axis)EditorGUILayout.EnumPopup("Axis", moveAxis);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);
        sortByName = EditorGUILayout.Toggle("Sort by Name", sortByName);

        GUILayout.Space(10);

        if (GUILayout.Button("Align"))
        {
            AlignSelectedObjects();
        }

        if (originalPositions.Count > 0 && GUILayout.Button("Restore Original Positions"))
        {
            RestoreOriginalPositions();
        }
    }

    void AlignSelectedObjects()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("No GameObjects selected.");
            return;
        }

        // Optionally sort
        if (sortByName)
        {
            System.Array.Sort(selected, (a, b) => a.name.CompareTo(b.name));
        }

        Undo.RecordObjects(selected, "Align Objects in Grid");
        originalPositions.Clear();

        Vector3 startPos = selected[0].transform.position;

        for (int i = 0; i < selected.Length; i++)
        {
            GameObject obj = selected[i];
            originalPositions[obj] = obj.transform.position;

            Vector3 newPos = startPos;
            switch (moveAxis)
            {
                case Axis.X: newPos.x += i * spacing; break;
                case Axis.Y: newPos.y += i * spacing; break;
                case Axis.Z: newPos.z += i * spacing; break;
            }

            obj.transform.position = newPos;
        }

        Debug.Log("Aligned " + selected.Length + " objects.");
    }

    void RestoreOriginalPositions()
    {
        foreach (var pair in originalPositions)
        {
            if (pair.Key != null)
            {
                Undo.RecordObject(pair.Key.transform, "Restore Position");
                pair.Key.transform.position = pair.Value;
            }
        }

        Debug.Log("Restored original positions.");
        originalPositions.Clear();
    }
}
