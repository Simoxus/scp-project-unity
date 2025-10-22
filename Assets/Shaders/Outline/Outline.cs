using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    public enum Mode
    {
        OutlineAll,
        OutlineVisible,
        OutlineHidden,
        OutlineAndSilhouette,
        SilhouetteOnly
    }

    public Mode OutlineMode
    {
        get { return outlineMode; }
        set
        {
            outlineMode = value;
            needsUpdate = true;
        }
    }

    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            needsUpdate = true;
        }
    }

    public float OutlineWidth
    {
        get { return outlineWidth; }
        set
        {
            outlineWidth = value;
            needsUpdate = true;
        }
    }

    public bool Enabled
    {
        get { return enabled; }
        set
        {
            enabled = value;
            needsUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [SerializeField]
    private Mode outlineMode;

    [SerializeField]
    private Color outlineColor = Color.white;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 2f;

    [Header("Optional")]

    [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
    + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline = true;

    [SerializeField, Tooltip("Apply outline to children: Outline will be applied to all child renderers. "
    + "Apply to this object only: Outline will only be applied to the renderer on this GameObject.")]
    private bool includeChildren = false;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    private Renderer[] renderers;
    private Material outlineMaskMaterial;
    private Material outlineFillMaterial;

    private bool needsUpdate;

    void Awake()
    {
        // Cache renderers
        if (includeChildren)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }
        else
        {
            renderers = new Renderer[] { GetComponent<Renderer>() };
            renderers = renderers.Where(r => r != null).ToArray();
        }

        // Instantiate outline materials
        outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Mat_Outline_Mask"));
        outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Mat_Outline_Fill"));

        outlineMaskMaterial.name = "OutlineMask (Instance)";
        outlineFillMaterial.name = "OutlineFill (Instance)";

        // Retrieve or generate smooth normals
        LoadSmoothNormals();

        needsUpdate = true;
        enabled = false;
    }

    void OnEnable()
    {
        foreach (var renderer in renderers)
        {
            // Append outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Add(outlineMaskMaterial);
            materials.Add(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnValidate()
    {
        // Update material properties
        needsUpdate = true;

        // Clear cache when baking is disabled or corrupted
        if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        // Generate smooth normals when baking is enabled
        if (precomputeOutline && bakeKeys.Count == 0)
        {
            Bake();
        }
    }

    void Update()
    {
        if (needsUpdate)
        {
            needsUpdate = false;
            UpdateMaterialProperties();
        }
    }

    void OnDisable()
    {
        foreach (var renderer in renderers)
        {
            // Remove outline shaders
            var materials = renderer.sharedMaterials.ToList();

            materials.Remove(outlineMaskMaterial);
            materials.Remove(outlineFillMaterial);

            renderer.materials = materials.ToArray();
        }
    }

    void OnDestroy()
    {
        // Destroy material instances
        Destroy(outlineMaskMaterial);
        Destroy(outlineFillMaterial);
    }

    void Bake()
    {
        // Generate smooth normals for each mesh
        var bakedMeshes = new HashSet<Mesh>();

        if (includeChildren)
        {
            // Bake all children
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.sharedMesh != null && bakedMeshes.Add(meshFilter.sharedMesh))
                {
                    var smoothNormals = SmoothNormals(meshFilter.sharedMesh);
                    bakeKeys.Add(meshFilter.sharedMesh);
                    bakeValues.Add(new ListVector3() { data = smoothNormals });
                }
            }

            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMeshRenderer.sharedMesh != null && bakedMeshes.Add(skinnedMeshRenderer.sharedMesh))
                {
                    var smoothNormals = SmoothNormals(skinnedMeshRenderer.sharedMesh);
                    bakeKeys.Add(skinnedMeshRenderer.sharedMesh);
                    bakeValues.Add(new ListVector3() { data = smoothNormals });
                }
            }
        }
        else
        {
            // Bake only this GameObject
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null && bakedMeshes.Add(meshFilter.sharedMesh))
            {
                var smoothNormals = SmoothNormals(meshFilter.sharedMesh);
                bakeKeys.Add(meshFilter.sharedMesh);
                bakeValues.Add(new ListVector3() { data = smoothNormals });
            }

            var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null && bakedMeshes.Add(skinnedMeshRenderer.sharedMesh))
            {
                var smoothNormals = SmoothNormals(skinnedMeshRenderer.sharedMesh);
                bakeKeys.Add(skinnedMeshRenderer.sharedMesh);
                bakeValues.Add(new ListVector3() { data = smoothNormals });
            }
        }
    }

    void LoadSmoothNormals()
    {
        if (includeChildren)
        {
            // Load smooth normals for all children
            foreach (var meshFilter in GetComponentsInChildren<MeshFilter>())
            {
                if (meshFilter.sharedMesh != null && registeredMeshes.Add(meshFilter.sharedMesh) && meshFilter.sharedMesh.vertexCount > 0)
                {
                    var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
                    List<Vector3> smoothNormals;

                    if (index >= 0)
                    {
                        smoothNormals = bakeValues[index].data;

                        if (smoothNormals.Count != meshFilter.sharedMesh.vertexCount)
                        {
                            Log.Editor($"Baked smooth normals count ({smoothNormals.Count}) doesn't match vertex count ({meshFilter.sharedMesh.vertexCount}) for mesh {meshFilter.sharedMesh.name}. Regenerating...");
                            smoothNormals = SmoothNormals(meshFilter.sharedMesh);
                        }
                    }
                    else
                    {
                        smoothNormals = SmoothNormals(meshFilter.sharedMesh);
                    }

                    meshFilter.sharedMesh.SetUVs(3, smoothNormals);

                    var renderer = meshFilter.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
                    }
                }
            }

            foreach (var skinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                if (skinnedMeshRenderer.sharedMesh != null && registeredMeshes.Add(skinnedMeshRenderer.sharedMesh) && skinnedMeshRenderer.sharedMesh.vertexCount > 0)
                {
                    var index = bakeKeys.IndexOf(skinnedMeshRenderer.sharedMesh);
                    List<Vector3> smoothNormals;

                    if (index >= 0)
                    {
                        smoothNormals = bakeValues[index].data;

                        if (smoothNormals.Count != skinnedMeshRenderer.sharedMesh.vertexCount)
                        {
                            Log.Editor($"Baked smooth normals count ({smoothNormals.Count}) doesn't match vertex count ({skinnedMeshRenderer.sharedMesh.vertexCount}) for mesh {skinnedMeshRenderer.sharedMesh.name}. Regenerating...");
                            smoothNormals = SmoothNormals(skinnedMeshRenderer.sharedMesh);
                        }
                    }
                    else
                    {
                        smoothNormals = SmoothNormals(skinnedMeshRenderer.sharedMesh);
                    }

                    skinnedMeshRenderer.sharedMesh.SetUVs(3, smoothNormals);
                    CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
                }
            }
        }
        else
        {
            // Load smooth normals only for this GameObject
            var meshFilter = GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null && registeredMeshes.Add(meshFilter.sharedMesh) && meshFilter.sharedMesh.vertexCount > 0)
            {
                var index = bakeKeys.IndexOf(meshFilter.sharedMesh);
                List<Vector3> smoothNormals;

                if (index >= 0)
                {
                    smoothNormals = bakeValues[index].data;

                    if (smoothNormals.Count != meshFilter.sharedMesh.vertexCount)
                    {
                        Debug.LogWarning($"Baked smooth normals count ({smoothNormals.Count}) doesn't match vertex count ({meshFilter.sharedMesh.vertexCount}) for mesh {meshFilter.sharedMesh.name}. Regenerating...");
                        smoothNormals = SmoothNormals(meshFilter.sharedMesh);
                    }
                }
                else
                {
                    smoothNormals = SmoothNormals(meshFilter.sharedMesh);
                }

                meshFilter.sharedMesh.SetUVs(3, smoothNormals);

                var renderer = meshFilter.GetComponent<Renderer>();
                if (renderer != null)
                {
                    CombineSubmeshes(meshFilter.sharedMesh, renderer.sharedMaterials);
                }
            }

            var skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null && registeredMeshes.Add(skinnedMeshRenderer.sharedMesh) && skinnedMeshRenderer.sharedMesh.vertexCount > 0)
            {
                var index = bakeKeys.IndexOf(skinnedMeshRenderer.sharedMesh);
                List<Vector3> smoothNormals;

                if (index >= 0)
                {
                    smoothNormals = bakeValues[index].data;

                    if (smoothNormals.Count != skinnedMeshRenderer.sharedMesh.vertexCount)
                    {
                        Debug.LogWarning($"Baked smooth normals count ({smoothNormals.Count}) doesn't match vertex count ({skinnedMeshRenderer.sharedMesh.vertexCount}) for mesh {skinnedMeshRenderer.sharedMesh.name}. Regenerating...");
                        smoothNormals = SmoothNormals(skinnedMeshRenderer.sharedMesh);
                    }
                }
                else
                {
                    smoothNormals = SmoothNormals(skinnedMeshRenderer.sharedMesh);
                }

                skinnedMeshRenderer.sharedMesh.SetUVs(3, smoothNormals);
                CombineSubmeshes(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials);
            }
        }
    }

    List<Vector3> SmoothNormals(Mesh mesh)
    {
        // Group vertices by location
        var groups = mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

        // Copy normals to a new list
        var smoothNormals = new List<Vector3>(mesh.normals);

        // Average normals for grouped vertices
        foreach (var group in groups)
        {
            // Skip single vertices
            if (group.Count() == 1)
            {
                continue;
            }

            // Calculate the average normal
            var smoothNormal = Vector3.zero;

            foreach (var pair in group)
            {
                smoothNormal += smoothNormals[pair.Value];
            }

            smoothNormal.Normalize();

            // Assign smooth normal to each vertex
            foreach (var pair in group)
            {
                smoothNormals[pair.Value] = smoothNormal;
            }
        }

        return smoothNormals;
    }

    void CombineSubmeshes(Mesh mesh, Material[] materials)
    {
        // Skip meshes with a single submesh
        if (mesh.subMeshCount == 1)
        {
            return;
        }

        // Skip if submesh count exceeds material count
        if (mesh.subMeshCount > materials.Length)
        {
            return;
        }

        // Append combined submesh
        mesh.subMeshCount++;
        mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
    }

    void UpdateMaterialProperties()
    {
        // Apply properties according to mode
        outlineFillMaterial.SetColor("_OutlineColor", outlineColor);

        float effectiveWidth = enabled ? outlineWidth : 0f;

        switch (outlineMode)
        {
            case Mode.OutlineAll:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", effectiveWidth);
                break;

            case Mode.OutlineVisible:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_OutlineWidth", effectiveWidth);
                break;

            case Mode.OutlineHidden:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", effectiveWidth);
                break;

            case Mode.OutlineAndSilhouette:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                outlineFillMaterial.SetFloat("_OutlineWidth", effectiveWidth);
                break;

            case Mode.SilhouetteOnly:
                outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
                break;
        }
    }
}