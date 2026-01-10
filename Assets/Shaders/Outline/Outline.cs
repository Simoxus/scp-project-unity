using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
    private static readonly HashSet<Mesh> registeredMeshes = new();
    public static bool GlobalOutlinesEnabled { get; set; } = true;
    public static bool GlobalFadingEnabled { get; set; } = true;

    public enum Mode { OutlineAll, OutlineVisible, OutlineHidden, OutlineAndSilhouette, SilhouetteOnly }

    [Header("Appearance")]
    [SerializeField] private Mode outlineMode;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField, Range(0f, 10f)] private float outlineWidth = 2f;

    [Header("Fading")]
    [SerializeField] private bool enableFading = true;
    [SerializeField, Range(0.01f, 20f)] private float fadeInSpeed = 3.8f;
    [SerializeField, Range(0.01f, 40f)] private float fadeOutSpeed = 37f;

    [Header("Optional")]
    [SerializeField] private bool precomputeOutline = true;
    [SerializeField] private bool includeChildren = false;

    [SerializeField, HideInInspector] private List<Mesh> bakeKeys = new();
    [SerializeField, HideInInspector] private List<ListVector3> bakeValues = new();

    private Renderer[] _renderers;
    private Material _outlineFillMaterial;
    private Material _outlineMaskMaterial;
    private bool _needsUpdate;
    private float _currentAlpha;
    private float _targetAlpha;
    private bool _isEnabled;
    private bool _materialsAttached;
    private CancellationTokenSource _fadeCts;

    public Mode OutlineMode { get => outlineMode; set { outlineMode = value; _needsUpdate = true; } }
    public Color OutlineColor { get => outlineColor; set { outlineColor = value; _needsUpdate = true; } }
    public float OutlineWidth { get => outlineWidth; set { outlineWidth = value; _needsUpdate = true; } }

    [Serializable]
    private class ListVector3 { public List<Vector3> data; }

    private void Awake()
    {
        if (!GlobalOutlinesEnabled)
        {
            enabled = false;
            return;
        }

        _renderers = (includeChildren ? GetComponentsInChildren<Renderer>() : new[] { GetComponent<Renderer>() })
            .Where(r => r != null).ToArray();

        _outlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Mat_Outline_Mask"));
        _outlineFillMaterial = Instantiate(Resources.Load<Material>(@"Mat_Outline_Fill"));
        _outlineMaskMaterial.name = "OutlineMask (Instance)";
        _outlineFillMaterial.name = "OutlineFill (Instance)";

        LoadSmoothNormals();
        _needsUpdate = true;
        enabled = false;
    }

    private void OnEnable()
    {
        if (!GlobalOutlinesEnabled) return;

        _isEnabled = true;
        _targetAlpha = 1f;

        if (enableFading && GlobalFadingEnabled)
        {
            _currentAlpha = 0f;
            if (!_materialsAttached) AttachOutlineMaterials();
            UpdateMaterialAlpha();
            StartFade();
        }
        else
        {
            if (!_materialsAttached) AttachOutlineMaterials();
            _currentAlpha = 1f;
            UpdateMaterialAlpha();
        }
    }

    private void OnDisable()
    {
        _isEnabled = false;
        _targetAlpha = 0f;

        if (enableFading && GlobalFadingEnabled)
            StartFade();
        else
        {
            _currentAlpha = 0f;
            DetachOutlineMaterials();
        }
    }

    private void OnDestroy()
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        if (_materialsAttached) DetachOutlineMaterials();
        Destroy(_outlineMaskMaterial);
        Destroy(_outlineFillMaterial);
    }

    private void OnValidate()
    {
        _needsUpdate = true;

        if (!precomputeOutline && bakeKeys.Count != 0 || bakeKeys.Count != bakeValues.Count)
        {
            bakeKeys.Clear();
            bakeValues.Clear();
        }

        if (precomputeOutline && bakeKeys.Count == 0) Bake();
    }

    private void Update()
    {
        if (_needsUpdate)
        {
            _needsUpdate = false;
            UpdateMaterialProperties();
        }
    }

    private void StartFade()
    {
        _fadeCts?.Cancel();
        _fadeCts?.Dispose();
        _fadeCts = new CancellationTokenSource();
        FadeAsync(_fadeCts.Token).Forget();
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

            if (_currentAlpha <= 0.001f && !_isEnabled)
                DetachOutlineMaterials();
        }
        catch (OperationCanceledException) { }
    }

    private void AttachOutlineMaterials()
    {
        if (_materialsAttached || _renderers == null) return;

        foreach (var r in _renderers)
        {
            if (r == null) continue;
            var mats = r.sharedMaterials.ToList();
            mats.Add(_outlineMaskMaterial);
            mats.Add(_outlineFillMaterial);
            r.materials = mats.ToArray();
        }
        _materialsAttached = true;
    }

    private void DetachOutlineMaterials()
    {
        if (!_materialsAttached || _renderers == null) return;

        foreach (var r in _renderers)
        {
            if (r == null) continue;
            var mats = r.sharedMaterials.ToList();
            mats.Remove(_outlineMaskMaterial);
            mats.Remove(_outlineFillMaterial);
            r.materials = mats.ToArray();
        }
        _materialsAttached = false;
    }

    private void UpdateMaterialAlpha()
    {
        if (_outlineFillMaterial == null) return;
        var color = outlineColor;
        color.a = _currentAlpha;
        _outlineFillMaterial.SetColor("_OutlineColor", color);
    }

    private void Bake()
    {
        var bakedMeshes = new HashSet<Mesh>();
        ProcessMeshes((mesh, _) =>
        {
            if (bakedMeshes.Add(mesh))
            {
                bakeKeys.Add(mesh);
                bakeValues.Add(new ListVector3 { data = SmoothNormals(mesh) });
            }
        });
    }

    private void LoadSmoothNormals()
    {
        ProcessMeshes((mesh, mats) =>
        {
            if (!registeredMeshes.Add(mesh) || mesh.vertexCount == 0) return;

            var idx = bakeKeys.IndexOf(mesh);
            var smoothNormals = idx >= 0 && bakeValues[idx].data.Count == mesh.vertexCount
                ? bakeValues[idx].data
                : SmoothNormals(mesh);

            mesh.SetUVs(3, smoothNormals);
            if (mats != null) CombineSubmeshes(mesh, mats);
        });

        UpdateRendererBounds();
    }

    private void ProcessMeshes(Action<Mesh, Material[]> action)
    {
        if (includeChildren)
        {
            foreach (var mf in GetComponentsInChildren<MeshFilter>())
                if (mf?.sharedMesh != null) action(mf.sharedMesh, mf.GetComponent<Renderer>()?.sharedMaterials);

            foreach (var smr in GetComponentsInChildren<SkinnedMeshRenderer>())
                if (smr?.sharedMesh != null) action(smr.sharedMesh, smr.sharedMaterials);
        }
        else
        {
            var mf = GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
                action(mf.sharedMesh, GetComponent<Renderer>()?.sharedMaterials);

            if (TryGetComponent<SkinnedMeshRenderer>(out var smr) && smr.sharedMesh != null)
                action(smr.sharedMesh, smr.sharedMaterials);
        }
    }

    private List<Vector3> SmoothNormals(Mesh mesh)
    {
        var smoothNormals = new List<Vector3>(mesh.normals);
        var vertices = mesh.vertices;
        var groups = new Dictionary<Vector3, List<int>>();

        // Group vertex indices by position
        for (int i = 0; i < vertices.Length; i++)
        {
            if (!groups.TryGetValue(vertices[i], out var indices))
            {
                indices = new List<int>();
                groups[vertices[i]] = indices;
            }
            indices.Add(i);
        }

        // Average normals for each group
        foreach (var indices in groups.Values)
        {
            if (indices.Count == 1) continue;

            var smoothNormal = Vector3.zero;
            foreach (var i in indices)
                smoothNormal += smoothNormals[i];
            smoothNormal.Normalize();

            foreach (var i in indices)
                smoothNormals[i] = smoothNormal;
        }

        return smoothNormals;
    }

    private void CombineSubmeshes(Mesh mesh, Material[] materials)
    {
        if (mesh.subMeshCount > 1 && mesh.subMeshCount <= materials.Length)
        {
            mesh.subMeshCount++;
            mesh.SetTriangles(mesh.triangles, mesh.subMeshCount - 1);
        }
    }

    private void UpdateMaterialProperties()
    {
        var color = outlineColor;
        color.a = _currentAlpha;
        _outlineFillMaterial.SetColor("_OutlineColor", color);

        var width = _isEnabled ? outlineWidth : 0f;
        var (maskZTest, fillZTest, finalWidth) = outlineMode switch
        {
            Mode.OutlineAll => (Always, Always, width),
            Mode.OutlineVisible => (Always, LessEqual, width),
            Mode.OutlineHidden => (Always, Greater, width),
            Mode.OutlineAndSilhouette => (Always, Always, width),
            Mode.SilhouetteOnly => (LessEqual, Greater, 0f),
            _ => (Always, Always, width)
        };

        _outlineMaskMaterial.SetFloat("_ZTest", (float)maskZTest);
        _outlineFillMaterial.SetFloat("_ZTest", (float)fillZTest);
        _outlineFillMaterial.SetFloat("_OutlineWidth", finalWidth);

        UpdateRendererBounds();
    }

    private void UpdateRendererBounds()
    {
        if (_renderers == null) return;

        // Calculate expansion needed
        float maxExpansion = outlineWidth * 0.001f * 10f;

        foreach (var r in _renderers)
        {
            if (r == null) continue;

            if (r is SkinnedMeshRenderer smr)
            {
                var bounds = smr.localBounds;
                bounds.Expand(maxExpansion * 2f);
                smr.localBounds = bounds;
            }
            else if (r.TryGetComponent<MeshFilter>(out var mf) && mf.sharedMesh != null)
            {
                var mesh = mf.sharedMesh;
                var meshBounds = mesh.bounds;
                meshBounds.Expand(maxExpansion * 2f);
                mesh.bounds = meshBounds;
            }
        }
    }

    private const UnityEngine.Rendering.CompareFunction Always = UnityEngine.Rendering.CompareFunction.Always;
    private const UnityEngine.Rendering.CompareFunction LessEqual = UnityEngine.Rendering.CompareFunction.LessEqual;
    private const UnityEngine.Rendering.CompareFunction Greater = UnityEngine.Rendering.CompareFunction.Greater;
}