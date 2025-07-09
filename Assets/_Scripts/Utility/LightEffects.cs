using UnityEngine;
public class LightEffects : MonoBehaviour
{
    public enum LightEffectType
    {
        None,
        Pulse,
        Flicker,
        Dim,
        Erratic
    }

    public Light[] targetLights;

    [Header("Light Settings")]
    public LightEffectType effectType = LightEffectType.Pulse;
    [Range(0f, 100f)] public float minIntensity = 0.5f;
    [Range(0f, 100f)] public float maxIntensity = 2.0f;
    public float speed = 1.0f;

    [Header("Flicker Settings")]
    public float flickerNoiseScale = 5.0f;

    [Header("Erratic Settings")]
    public float erraticMinInterval = 0.05f;
    public float erraticMaxInterval = 0.2f;

    private float[] _originalIntensities;
    private float _dimStartTime;
    private float _initialDimIntensity;
    private float _currentDimDuration;
    private float _nextErraticChangeTime;
    private float _targetErraticIntensity;

    void Awake()
    {
        // Get Light components if none are assigned in the array
        if (targetLights == null || targetLights.Length == 0)
        {
            targetLights = GetComponentsInChildren<Light>();
        }

        // Initialize originalIntensities array and store values
        if (targetLights != null && targetLights.Length > 0)
        {
            _originalIntensities = new float[targetLights.Length];
            for (int i = 0; i < targetLights.Length; i++)
            {
                if (targetLights[i] != null)
                {
                    _originalIntensities[i] = targetLights[i].intensity;
                }
            }
        }
        else
        {
            Debug.LogWarning("No Light components were assigned. Disabling script.", this);
            enabled = false; // Disable script if no lights are found to avoid unnecessary Update calls
        }
    }

    void Update()
    {
        for (int i = 0; i < targetLights.Length; i++)
        {
            if (targetLights[i] == null) continue; // Skip if a light reference is missing at runtime

            switch (effectType)
            {
                case LightEffectType.None:
                    // Ensure index is valid for original intensities
                    if (i < _originalIntensities.Length)
                    {
                        targetLights[i].intensity = _originalIntensities[i];
                    }
                    break;
                case LightEffectType.Pulse:
                    ApplyPulseEffect(targetLights[i]);
                    break;
                case LightEffectType.Flicker:
                    ApplyFlickerEffect(targetLights[i]);
                    break;
                case LightEffectType.Dim:
                    ApplyDimEffect(targetLights[i]);
                    break;
                case LightEffectType.Erratic:
                    ApplyErraticEffect(targetLights[i], i);
                    break;
            }
        }
    }

    public void StopEffects()
    {
        effectType = LightEffectType.None;
        // The Update method will handle reverting intensities when effectType is None
    }

    private void ApplyPulseEffect(Light light)
    {
        light.intensity = Mathf.Lerp(minIntensity, maxIntensity, (Mathf.Sin(Time.time * speed) + 1) * 0.5f);
    }

    /// <summary>
    /// Applies a flickering effect using Perlin noise for smoother randomness.
    /// </summary>
    private void ApplyFlickerEffect(Light light)
    {
        float noise = Mathf.PerlinNoise(Time.time * speed, 0.0f);
        light.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise * flickerNoiseScale);
    }

    public void StartDimEffect(float duration)
    {
        effectType = LightEffectType.Dim;
        _dimStartTime = Time.time;
        // Store initial intensity for the start of the dim
        // Assuming targetLights[0] is the primary light, or you iterate for all.
        // If you want per-light dim, you'd need to store _initialDimIntensity per light.
        if (targetLights != null && targetLights.Length > 0 && targetLights[0] != null)
        {
            _initialDimIntensity = targetLights[0].intensity;
        }
        _currentDimDuration = duration;
    }

    private void ApplyDimEffect(Light light)
    {
        // If the effect has just started or hasn't finished, interpolate
        if (Time.time < _dimStartTime + _currentDimDuration)
        {
            float t = (Time.time - _dimStartTime) / _currentDimDuration;
            light.intensity = Mathf.Lerp(_initialDimIntensity, minIntensity, t);
        }
        else
        {
            light.intensity = minIntensity;
        }
    }

    private void ApplyErraticEffect(Light light, int index)
    {
        if (Time.time >= _nextErraticChangeTime)
        {
            _targetErraticIntensity = Random.Range(minIntensity, maxIntensity + 0.001f);
            _nextErraticChangeTime = Time.time + Random.Range(erraticMinInterval, erraticMaxInterval);
        }

        light.intensity = Mathf.Lerp(light.intensity, _targetErraticIntensity, Time.deltaTime * speed * 15f); // Multiply speed for faster transition
    }
}