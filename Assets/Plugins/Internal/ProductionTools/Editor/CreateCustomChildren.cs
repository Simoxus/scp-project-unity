using UnityEditor;
using UnityEngine;

public class CreateCustomChildren : EditorWindow
{
    private string childNamesInput = "Child1\nChild2\nChild3";

    [MenuItem("Tools/Production Tools/Create Named Children")]
    public static void ShowWindow()
    {
        GetWindow<CreateCustomChildren>("Create Named Children");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Create Named Empty Children", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Select GameObjects in the hierarchy and input child names (one per line).", MessageType.Info);

        EditorGUILayout.LabelField("Child Names (one per line):");
        childNamesInput = EditorGUILayout.TextArea(childNamesInput, GUILayout.Height(100));

        if (GUILayout.Button("Create Children"))
        {
            CreateChildren();
        }
    }

    private void CreateChildren()
    {
        string[] names = childNamesInput.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects in the Hierarchy.", "OK");
            return;
        }

        if (names.Length == 0)
        {
            EditorUtility.DisplayDialog("No Names", "Please enter at least one name for the children.", "OK");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();

        foreach (GameObject parent in selectedObjects)
        {
            foreach (string childName in names)
            {
                GameObject child = new GameObject(childName.Trim());
                Undo.RegisterCreatedObjectUndo(child, "Create Named Child");
                child.transform.SetParent(parent.transform);
                child.transform.localPosition = Vector3.zero;
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
    }
}
