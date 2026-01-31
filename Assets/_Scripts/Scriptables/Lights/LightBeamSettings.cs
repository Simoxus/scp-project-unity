using EditorAttributes;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class LightBeamSettings : MonoBehaviour
{
    [Space]
    [SerializeField] private LightBeamPreset preset;

    [Header("Override Settings")]
    [SerializeField] private bool overrideColor = false;
    [SerializeField] private bool overrideIntensity = false;
    [SerializeField] private bool overrideBeamDensity = false;
    [SerializeField] private bool overrideFadeDistance = false;
    [SerializeField] private bool overrideFadePower = false;
    [SerializeField] private bool overrideNoiseIntensity = false;

    [Header("Override Values")]
    [ShowField(nameof(overrideColor))] public Color baseColor = Color.white;
    [ShowField(nameof(overrideIntensity)), Range(0, 5)] public float intensity = 1f;
    [ShowField(nameof(overrideBeamDensity)), Range(0, 1)] public float beamDensity = 0.3f;
    [ShowField(nameof(overrideFadeDistance)), Range(0, 50)] public float fadeDistance = 12f;
    [ShowField(nameof(overrideFadePower)), Range(0.1f, 10f)] public float fadePower = 2f;
    [ShowField(nameof(overrideNoiseIntensity)), Range(0, 1)] public float noiseIntensity = 0.3f;

    private MaterialPropertyBlock _propBlock;
    private Renderer _beamMeshRenderer;

    private Color _runtimeColor;
    private float _runtimeIntensity;
    private bool _hasRuntimeColorOverride = false;
    private bool _hasRuntimeIntensityOverride = false;

    public bool OverrideColor => overrideColor;
    public bool OverrideIntensity => overrideIntensity;
    public bool OverrideBeamDensity => overrideBeamDensity;
    public bool OverrideFadeDistance => overrideFadeDistance;
    public bool OverrideFadePower => overrideFadePower;
    public bool OverrideNoiseIntensity => overrideNoiseIntensity;

    private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
    private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
    private static readonly int OverallAlphaID = Shader.PropertyToID("_OverallAlpha");
    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");
    private static readonly int FadeDistID = Shader.PropertyToID("_FadeDist");
    private static readonly int FadePowerID = Shader.PropertyToID("_FadePower");
    private static readonly int ViewPowerID = Shader.PropertyToID("_ViewPower");
    private static readonly int ViewMinID = Shader.PropertyToID("_ViewMin");
    private static readonly int ViewMaxID = Shader.PropertyToID("_ViewMax");
    private static readonly int NoiseScaleID = Shader.PropertyToID("_NoiseScale");
    private static readonly int NoiseIntensityID = Shader.PropertyToID("_NoiseIntensity");
    private static readonly int NoiseSpeedID = Shader.PropertyToID("_NoiseSpeed");
    private static readonly int NoiseContrastID = Shader.PropertyToID("_NoiseContrast");
    private static readonly int DustDensityID = Shader.PropertyToID("_DustDensity");
    private static readonly int DustSizeID = Shader.PropertyToID("_DustSize");
    private static readonly int DustIntensityID = Shader.PropertyToID("_DustIntensity");
    private static readonly int DustSpeedID = Shader.PropertyToID("_DustSpeed");
    private static readonly int DustDriftID = Shader.PropertyToID("_DustDrift");
    private static readonly int DustHeightRangeID = Shader.PropertyToID("_DustHeightRange");
    private static readonly int DustRadialSpreadID = Shader.PropertyToID("_DustRadialSpread");
    private static readonly int DustOffsetID = Shader.PropertyToID("_DustOffset");

    private void OnEnable()
    {
        Initialize();
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_beamMeshRenderer == null)
            _beamMeshRenderer = GetComponent<Renderer>();

        if (_propBlock == null)
            _propBlock = new MaterialPropertyBlock();

        ApplySettings();
    }

    public LightBeamPreset GetPreset() => preset;

    public void ApplySettings()
    {
        if (_beamMeshRenderer == null || preset == null) return;

        if (_propBlock == null)
            _propBlock = new MaterialPropertyBlock();

        Color finalColor = _hasRuntimeColorOverride ? _runtimeColor :
                          (overrideColor ? baseColor : preset.baseColor);

        float finalIntensity = _hasRuntimeIntensityOverride ? _runtimeIntensity :
                              (overrideIntensity ? intensity : preset.intensity);

        float finalBeamDensity = overrideBeamDensity ? beamDensity : preset.beamDensity;
        float finalFadeDist = overrideFadeDistance ? fadeDistance : preset.fadeDist;
        float finalFadePower = overrideFadePower ? fadePower : preset.fadePower;
        float finalNoiseIntensity = overrideNoiseIntensity ? noiseIntensity : preset.noiseIntensity;

        // Apply all properties
        _propBlock.SetColor(BaseColorID, finalColor);
        _propBlock.SetFloat(IntensityID, finalIntensity);
        _propBlock.SetFloat(OverallAlphaID, finalBeamDensity);
        _propBlock.SetFloat(FadeDistID, finalFadeDist);
        _propBlock.SetFloat(FadePowerID, finalFadePower);
        _propBlock.SetFloat(NoiseIntensityID, finalNoiseIntensity);

        if (preset.useTexture && preset.mainTexture != null)
            _propBlock.SetTexture(MainTexID, preset.mainTexture);

        _propBlock.SetFloat(ViewPowerID, preset.viewPower);
        _propBlock.SetFloat(ViewMinID, preset.viewMin);
        _propBlock.SetFloat(ViewMaxID, preset.viewMax);

        _propBlock.SetFloat(NoiseScaleID, preset.noiseScale);
        _propBlock.SetFloat(NoiseSpeedID, preset.noiseSpeed);
        _propBlock.SetFloat(NoiseContrastID, preset.noiseContrast);

        _propBlock.SetFloat(DustDensityID, preset.dustDensity);
        _propBlock.SetFloat(DustSizeID, preset.dustSize);
        _propBlock.SetFloat(DustIntensityID, preset.dustIntensity);
        _propBlock.SetFloat(DustSpeedID, preset.dustSpeed);
        _propBlock.SetFloat(DustDriftID, preset.dustDrift);
        _propBlock.SetFloat(DustHeightRangeID, preset.dustHeightRange);
        _propBlock.SetFloat(DustRadialSpreadID, preset.dustRadialSpread);
        _propBlock.SetVector(DustOffsetID, preset.dustOffset);

        SetKeywords(preset.useTexture, preset.useNoise, preset.useDust);

        _beamMeshRenderer.SetPropertyBlock(_propBlock);

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(gameObject);
#endif
    }

    public void SetRuntimeColor(Color color)
    {
        _runtimeColor = color;
        _hasRuntimeColorOverride = true;
        ApplySettings();
    }

    public void SetRuntimeIntensity(float newIntensity)
    {
        _runtimeIntensity = newIntensity;
        _hasRuntimeIntensityOverride = true;
        ApplySettings();
    }

    public void SetRuntimeColorAndIntensity(Color color, float newIntensity)
    {
        _runtimeColor = color;
        _runtimeIntensity = newIntensity;
        _hasRuntimeColorOverride = true;
        _hasRuntimeIntensityOverride = true;
        ApplySettings();
    }

    public void ClearRuntimeOverrides()
    {
        _hasRuntimeColorOverride = false;
        _hasRuntimeIntensityOverride = false;
        ApplySettings();
    }

    private void SetKeywords(bool texture, bool noise, bool dust)
    {
        Material mat = _beamMeshRenderer.sharedMaterial;
        if (mat == null) return;

        if (texture) mat.EnableKeyword("USE_TEXTURE");
        else mat.DisableKeyword("USE_TEXTURE");

        if (noise) mat.EnableKeyword("USE_NOISE");
        else mat.DisableKeyword("USE_NOISE");

        if (dust) mat.EnableKeyword("USE_DUST");
        else mat.DisableKeyword("USE_DUST");
    }

    private void OnValidate()
    {
        if (_beamMeshRenderer == null)
        {
            _beamMeshRenderer = GetComponent<Renderer>();
        }

        ApplySettings();
    }

    private void OnDisable()
    {
        if (_beamMeshRenderer != null)
        {
            _beamMeshRenderer.SetPropertyBlock(null);
        }
    }
}