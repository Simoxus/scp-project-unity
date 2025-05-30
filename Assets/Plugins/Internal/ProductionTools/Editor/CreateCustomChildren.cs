using UnityEngine;
using UnityEditor;

public class CreateCustomChildren : MonoBehaviour
{
    [MenuItem("GameObject/Create Custom Children", false, 10)]
    static void CreateChildren()
    {
        Transform parent = Selection.activeTransform;

        if (parent == null)
        {
            Debug.LogWarning("You must select a GameObject in the Hierarchy first.");
            return;
        }

        CreateChild("Interacts", parent);
        CreateChild("Lights", parent);
        CreateChild("Props", parent);
    }

    static void CreateChild(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name); // Makes it undoable
        go.transform.SetParent(parent);
        go.transform.localPosition = Vector3.zero; // Optional: resets local position
    }

    // Ensures the menu item only appears if a GameObject is selected
    [MenuItem("GameObject/Create Custom Children", true)]
    static bool ValidateCreateChildren()
    {
        return Selection.activeTransform != null;
    }
}
