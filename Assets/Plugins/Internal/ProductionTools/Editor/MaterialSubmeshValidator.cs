using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;

public class MaterialSubmeshValidator : EditorWindow
{
    private bool scanScene = true;
    private bool scanPrefabs = true;
    private bool autoFix = false;
    private List<string> issues = new();

    [MenuItem("Tools/Validation/Material vs SubMesh Checker")]
    public static void ShowWindow()
    {
        GetWindow<MaterialSubmeshValidator>("Material/SubMesh Validator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Material/SubMesh Validator", EditorStyles.boldLabel);
        scanScene = EditorGUILayout.Toggle("Scan Active Scene", scanScene);
        scanPrefabs = EditorGUILayout.Toggle("Scan Prefabs in Project", scanPrefabs);
        autoFix = EditorGUILayout.Toggle("Auto Fix Issues", autoFix);

        if (GUILayout.Button("Run Validation"))
        {
            issues.Clear();
            if (scanScene)
                ScanScene();

            if (scanPrefabs)
                ScanPrefabs();

            Debug.Log($"Material/SubMesh validation completed. Issues found: {issues.Count}");

            foreach (var issue in issues)
                Debug.LogWarning(issue);
        }

        if (issues.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label("Issues Found:");
            foreach (var issue in issues)
                GUILayout.Label("- " + issue);
        }
    }

    private void ScanScene()
    {
        foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            CheckRenderer(renderer, renderer.gameObject.name);
        }
    }

    private void ScanPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                CheckRenderer(renderer, $"Prefab: {path}/{renderer.gameObject.name}");
            }
        }
    }

    private void CheckRenderer(Renderer renderer, string context)
    {
        Mesh mesh = null;
        if (renderer is MeshRenderer meshRenderer)
        {
            var filter = renderer.GetComponent<MeshFilter>();
            if (filter != null) mesh = filter.sharedMesh;
        }
        else if (renderer is SkinnedMeshRenderer skinned)
        {
            mesh = skinned.sharedMesh;
        }

        if (mesh == null) return;

        int subMeshCount = mesh.subMeshCount;
        var materials = renderer.sharedMaterials;

        if (materials.Length > subMeshCount)
        {
            issues.Add($"[{context}] Material count ({materials.Length}) > subMesh count ({subMeshCount})");

            if (autoFix)
            {
                var trimmed = new Material[subMeshCount];
                for (int i = 0; i < subMeshCount; i++)
                    trimmed[i] = materials[i];
                renderer.sharedMaterials = trimmed;

                EditorUtility.SetDirty(renderer);
            }
        }
    }
}
