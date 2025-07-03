using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class AssignSelectedToArray : EditorWindow
{
    private UnityEngine.Object targetObject;
    private List<FieldInfo> arrayFields = new List<FieldInfo>();
    private string[] fieldNames;
    private int selectedFieldIndex = -1;

    [MenuItem("Tools/Production Tools/Assign Selected to Array (Advanced)")]
    private static void Init()
    {
        var window = GetWindow<AssignSelectedToArray>("Assign To Array");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Target Component or ScriptableObject", EditorStyles.boldLabel);
        targetObject = EditorGUILayout.ObjectField("Target", targetObject, typeof(UnityEngine.Object), true);

        if (targetObject != null && GUI.changed)
        {
            CacheArrayFields();
        }

        if (arrayFields.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Select Array Field to Assign", EditorStyles.boldLabel);
            selectedFieldIndex = EditorGUILayout.Popup("Field", selectedFieldIndex, fieldNames);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Assign Selected GameObjects"))
        {
            if (targetObject == null || selectedFieldIndex < 0)
            {
                Debug.LogError("Target or field not set.");
                return;
            }

            AssignToSelectedField();
        }
    }

    private void CacheArrayFields()
    {
        arrayFields.Clear();

        var targetType = targetObject.GetType();
        var fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            if (!field.FieldType.IsArray) continue;

            Type elementType = field.FieldType.GetElementType();
            if (typeof(UnityEngine.Object).IsAssignableFrom(elementType))
            {
                arrayFields.Add(field);
            }
        }

        fieldNames = arrayFields.Select(f => $"{f.Name} ({f.FieldType.GetElementType().Name}[])").ToArray();
        selectedFieldIndex = arrayFields.Count > 0 ? 0 : -1;
    }

    private void AssignToSelectedField()
    {
        var field = arrayFields[selectedFieldIndex];
        var elementType = field.FieldType.GetElementType();
        var list = new List<UnityEngine.Object>();

        foreach (var go in Selection.gameObjects)
        {
            UnityEngine.Object match = GetAssignableObject(go, elementType);
            if (match != null) list.Add(match);
        }

        Array finalArray = Array.CreateInstance(elementType, list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            finalArray.SetValue(list[i], i);
        }

        Undo.RecordObject(targetObject, "Assign Selected to Array");
        field.SetValue(targetObject, finalArray);
        EditorUtility.SetDirty(targetObject);

        Debug.Log($"Assigned {list.Count} object(s) to '{field.Name}' in '{targetObject.name}'.");
    }

    private UnityEngine.Object GetAssignableObject(GameObject go, Type targetType)
    {
        if (targetType == typeof(GameObject) || targetType.IsAssignableFrom(typeof(GameObject)))
            return go;

        return go.GetComponent(targetType);
    }
}
