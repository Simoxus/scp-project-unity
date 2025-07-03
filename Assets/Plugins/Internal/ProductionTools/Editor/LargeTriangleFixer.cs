using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class LargeTriangleFixer : EditorWindow
{
    private const float Threshold = 500f;

    private class ProblemMesh
    {
        public Mesh mesh;
        public string assetPath;
        public List<int> largeTriangleIndices = new();
    }

    private List<ProblemMesh> problemMeshes = new();

    [MenuItem("Tools/Production Tools/Large Triangle Scanner")]
    public static void ShowWindow()
    {
        GetWindow<LargeTriangleFixer>("Large Triangle Scanner");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Scan All Meshes in Project"))
        {
            ScanMeshes();
        }

        if (problemMeshes.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Found {problemMeshes.Count} mesh(es) with large triangles", EditorStyles.boldLabel);

            foreach (var pm in problemMeshes)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"Mesh: {pm.mesh.name} | Triangles: {pm.largeTriangleIndices.Count} | Path: {pm.assetPath}");
                GUILayout.EndVertical();
            }

            if (GUILayout.Button("Fix Selected Meshes (Save as New Asset)"))
            {
                FixMeshes();
            }
        }
    }

    private void ScanMeshes()
    {
        problemMeshes.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Mesh");
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null) continue;

            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;
            var largeTris = new List<int>();

            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 v0 = verts[tris[i]];
                Vector3 v1 = verts[tris[i + 1]];
                Vector3 v2 = verts[tris[i + 2]];

                if (Vector3.Distance(v0, v1) > Threshold ||
                    Vector3.Distance(v1, v2) > Threshold ||
                    Vector3.Distance(v2, v0) > Threshold)
                {
                    largeTris.Add(i);
                }
            }

            if (largeTris.Count > 0)
            {
                problemMeshes.Add(new ProblemMesh
                {
                    mesh = mesh,
                    assetPath = path,
                    largeTriangleIndices = largeTris
                });
            }
        }

        Debug.Log($"Scan complete. {problemMeshes.Count} mesh(es) with large triangles.");
    }

    private void FixMeshes()
    {
        foreach (var pm in problemMeshes)
        {
            Mesh original = pm.mesh;
            string path = pm.assetPath;
            Vector3[] verts = original.vertices;
            int[] tris = original.triangles;

            List<Vector3> newVerts = new(verts);
            List<int> newTris = new();

            for (int i = 0; i < tris.Length; i += 3)
            {
                Vector3 v0 = verts[tris[i]];
                Vector3 v1 = verts[tris[i + 1]];
                Vector3 v2 = verts[tris[i + 2]];

                bool isLarge = Vector3.Distance(v0, v1) > Threshold ||
                               Vector3.Distance(v1, v2) > Threshold ||
                               Vector3.Distance(v2, v0) > Threshold;

                if (!isLarge)
                {
                    newTris.Add(tris[i]);
                    newTris.Add(tris[i + 1]);
                    newTris.Add(tris[i + 2]);
                }
                else
                {
                    // Midpoints
                    Vector3 m01 = (v0 + v1) * 0.5f;
                    Vector3 m12 = (v1 + v2) * 0.5f;
                    Vector3 m20 = (v2 + v0) * 0.5f;

                    int i0 = AddVertex(newVerts, v0);
                    int i1 = AddVertex(newVerts, v1);
                    int i2 = AddVertex(newVerts, v2);

                    int im01 = AddVertex(newVerts, m01);
                    int im12 = AddVertex(newVerts, m12);
                    int im20 = AddVertex(newVerts, m20);

                    // Subdivide into 4 triangles
                    newTris.Add(i0); newTris.Add(im01); newTris.Add(im20);
                    newTris.Add(im01); newTris.Add(i1); newTris.Add(im12);
                    newTris.Add(im12); newTris.Add(i2); newTris.Add(im20);
                    newTris.Add(im01); newTris.Add(im12); newTris.Add(im20);
                }
            }

            Mesh newMesh = new Mesh();
            newMesh.name = original.name + "_Fixed";
            newMesh.vertices = newVerts.ToArray();
            newMesh.triangles = newTris.ToArray();
            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

            string newPath = Path.GetDirectoryName(path) + "/" + original.name + "_Fixed.asset";
            AssetDatabase.CreateAsset(newMesh, newPath);
            AssetDatabase.SaveAssets();

            Debug.Log($"Fixed and saved mesh: {newPath}");
        }

        Debug.Log("Fixing complete.");
    }

    private int AddVertex(List<Vector3> verts, Vector3 v)
    {
        verts.Add(v);
        return verts.Count - 1;
    }
}
