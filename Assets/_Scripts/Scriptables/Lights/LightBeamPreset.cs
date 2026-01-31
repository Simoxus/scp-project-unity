using UnityEngine;

[CreateAssetMenu(fileName = "Preset_Beam_", menuName = "Custom/Light Beam Preset")]
public class LightBeamPreset : ScriptableObject
{
    [Header("Base Settings")]
    public Color baseColor = Color.white;
    [Range(0, 5)] public float intensity = 1f;
    [Range(0, 1)] public float beamDensity = 0.3f;

    [Header("Texture")]
    public bool useTexture = false;
    public Texture2D mainTexture;

    [Header("Fading")]
    [Range(0, 50)] public float fadeDist = 12f;
    [Range(0.1f, 10f)] public float fadePower = 2f;
    [Range(0.1f, 10f)] public float viewPower = 1f;
    [Range(-2, 2)] public float viewMin = -0.5f;
    [Range(-2, 5)] public float viewMax = 2.5f;

    [Header("Volumetric Noise")]
    public bool useNoise = true;
    [Range(0.1f, 10f)] public float noiseScale = 2f;
    [Range(0, 1)] public float noiseIntensity = 0.3f;
    [Range(0, 2)] public float noiseSpeed = 0.1f;
    [Range(0.1f, 5f)] public float noiseContrast = 1.5f;

    [Header("Dust Particles")]
    public bool useDust = false;
    [Range(1, 50)] public float dustDensity = 15f;
    [Range(0.001f, 0.1f)] public float dustSize = 0.01f;
    [Range(0, 5)] public float dustIntensity = 2f;
    [Range(0, 3)] public float dustSpeed = 0.3f;
    [Range(0, 2)] public float dustDrift = 0.3f;
    [Range(0.1f, 20f)] public float dustHeightRange = 5f;
    [Range(0.1f, 5f)] public float dustRadialSpread = 1f;
    public Vector3 dustOffset = Vector3.zero;

#if UNITY_EDITOR
    private void OnValidate()
    {
        var allBeamSettings = FindObjectsByType<LightBeamSettings>(FindObjectsSortMode.None);
        foreach (var beamSetting in allBeamSettings)
        {
            if (beamSetting.GetPreset() == this)
            {
                beamSetting.ApplySettings();
            }
        }
    }
#endif
}