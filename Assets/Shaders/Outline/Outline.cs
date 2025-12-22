using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    private static HashSet<Mesh> registeredMeshes = new HashSet<Mesh>();

    private static bool _globalFadingEnabled = true;
    public static bool GlobalFadingEnabled
    {
        get => _globalFadingEnabled;
        set => _globalFadingEnabled = value;
    }

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
            _needsUpdate = true;
        }
    }

    public Color OutlineColor
    {
        get { return outlineColor; }
        set
        {
            outlineColor = value;
            _needsUpdate = true;
        }
    }

    public float OutlineWidth
    {
        get { return outlineWidth; }
        set
        {
            outlineWidth = value;
            _needsUpdate = true;
        }
    }

    [Serializable]
    private class ListVector3
    {
        public List<Vector3> data;
    }

    [Header("Appearance")]
    [SerializeField] private Mode outlineMode;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;

    [Header("Fading")]
    [SerializeField] private bool enableFading = true;
    [SerializeField, Range(0.01f, 20f)] private float fadeInSpeed = 3.8f;
    [SerializeField, Range(0.01f, 40f)] private float fadeOutSpeed = 37f;

    [Header("Optional")]
    [SerializeField, Tooltip("Precompute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
    + "Precompute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
    private bool precomputeOutline = true;

    [SerializeField, Tooltip("Apply outline to children: Outline will be applied to all child _renderers. "
    + "Apply to this object only: Outline will only be applied to the renderer on this GameObject.")]
    private bool includeChildren = false;

    [SerializeField, HideInInspector]
    private List<Mesh> bakeKeys = new List<Mesh>();

    [SerializeField, HideInInspector]
    private List<ListVector3> bakeValues = new List<ListVector3>();

    private Renderer[] _renderers;
    private Material _outlineFillMaterial;
    private Material _outlineMaskMaterial;

    private bool _needsUpdate;
    private float _currentAlpha = 0f;
    private float _targetAlpha = 0f;
    private bool _isEnabled = false;
    private bool _materialsAttached = false;
    private CancellationTokenSource _fadeCts;

    void Awake()
    {
        // Cache renderers
        if (includeChildren)
        {
            _renderers = GetComponentsInChildren<Renderer>();
        }
        else
        {
            _renderers = new Renderer[] { GetComponent<Renderer>() };
            _renderers = _renderers.Where(r => r != null).ToArray();
        }

        // Instantiate outline materials
        _outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Mat_Outline_Mask"));
        _outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Mat_Outline_Fill"));

        _outlineMaskMaterial.name = "OutlineMask (Instance)";
        _outlineFillMaterial.name = "OutlineFill (Instance)";

        // Retrieve or generate smooth normals
        LoadSmoothNormals();

        _needsUpdate = true;

        // Start disabled
        enabled = false;
    }

    void OnEnable()
    {
        _isEnabled = true;
        _targetAlpha = 1f;

        // Start fade-in
        if (enableFading && GlobalFadingEnabled)
        {
            // Reset to start values for fade-in
            _currentAlpha = 0f;

            // Attach materials AFTER setting alpha to 0
            if (!_materialsAttached)
            {
                AttachOutlineMaterials();
            }

            UpdateMaterialAlpha();

            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = new CancellationTokenSource();
            FadeAsync(_fadeCts.Token).Forget();
        }
        else
        {
            if (!_materialsAttached)
            {
                AttachOutlineMaterials();
            }

            _currentAlpha = 1f;
            UpdateMaterialAlpha();
        }
    }

    void OnDisable()
    {
        _isEnabled = false;
        _targetAlpha = 0f;

        if (enableFading && GlobalFadingEnabled)
        {
            // Start fade-out - this will detach materials when done
            _fadeCts?.Cancel();
            _fadeCts?.Dispose();
            _fadeCts = new CancellationTokenSource();
            FadeAsync(_fadeCts.Token).Forget();
        }
        else
        {
            _currentAlpha = 0f;
            DetachOutlineMaterials();
        }
    }

    void OnValidate()
    {
        // Update material properties
        _needsUpdate = true;

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
        if (_needsUpdate)
        {
            _needsUpdate = false;
            UpdateMaterialProperties();
        }
    }

    void OnDestroy()
    {
        // Cancel any running fade tasks
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = null;

        // Detach materials if still attached
        if (_materialsAttached)
        {
            DetachOutlineMaterials();
        }

        // Destroy material instances
        Destroy(_outlineMaskMaterial);
        Destroy(_outlineFillMaterial);
    }

    private async UniTaskVoid FadeAsync(CancellationToken ct)
    {
        try
        {
            while (Mathf.Abs(_currentAlpha - _targetAlpha) > 0.001f)
            {
                ct.ThrowIfCancellationRequested();

                float speed = _targetAlpha > _currentAlpha ? fadeInSpeed : fadeOutSpeed;
                _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, Time.deltaTime * speed);

                UpdateMaterialAlpha();

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            _currentAlpha = _targetAlpha;
            UpdateMaterialAlpha();

            // If we faded out completely and component is disabled, detach materials
            if (_currentAlpha <= 0.001f && !_isEnabled)
            {
                DetachOutlineMaterials();
            }
        }
        catch (OperationCanceledException)
        {
            // Task was cancelled, this is expected behavior
        }
    }

    private void AttachOutlineMaterials()
    {
        if (_materialsAttached || _renderers == null) return;

        foreach (var renderer in _renderers)
        {
            if (renderer == null) continue;

            var materials = renderer.sharedMaterials.ToList();
            materials.Add(_outlineMaskMaterial);
            materials.Add(_outlineFillMaterial);
            renderer.materials = materials.ToArray();
        }

        _materialsAttached = true;
    }

    private void DetachOutlineMaterials()
    {
        if (!_materialsAttached || _renderers == null) return;

        foreach (var renderer in _renderers)
        {
            if (renderer == null) continue;

            var materials = renderer.sharedMaterials.ToList();
            materials.Remove(_outlineMaskMaterial);
            materials.Remove(_outlineFillMaterial);
            renderer.materials = materials.ToArray();
        }

        _materialsAttached = false;
    }

    private void UpdateMaterialAlpha()
    {
        if (_outlineFillMaterial == null) return;

        Color color = outlineColor;
        color.a = _currentAlpha;
        _outlineFillMaterial.SetColor("_OutlineColor", color);
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
        // Apply color with current alpha
        Color color = outlineColor;
        color.a = _currentAlpha;
        _outlineFillMaterial.SetColor("_OutlineColor", color);

        float effectiveWidth = _isEnabled ? outlineWidth : 0f;

        switch (outlineMode)
        {
            case Mode.OutlineAll:
                _outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat("_OutlineWidth", effectiveWidth);
                break;

            case Mode.OutlineVisible:
                _outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                _outlineFillMaterial.SetFloat("_OutlineWidth", effectiveWidth);
                break;

            case Mode.OutlineHidden:
                _outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                _outlineFillMaterial.SetFloat("_OutlineWidth", effectiveWidth);
                break;

            case Mode.OutlineAndSilhouette:
                _outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                _outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Always);
                _outlineFillMaterial.SetFloat("_OutlineWidth", effectiveWidth);
                break;

            case Mode.SilhouetteOnly:
                _outlineMaskMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.LessEqual);
                _outlineFillMaterial.SetFloat("_ZTest", (float)UnityEngine.Rendering.CompareFunction.Greater);
                _outlineFillMaterial.SetFloat("_OutlineWidth", 0f);
                break;
        }
    }
}