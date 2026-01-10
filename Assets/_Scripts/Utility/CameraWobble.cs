using UnityEngine;

public class CameraWobble : MonoBehaviour
{
    [Header("Wobble Intensity")]
    [SerializeField] private float positionAmplitude = 0.06f;
    [SerializeField] private float rotationAmplitude = 0.6f;

    [Header("Motion Settings")]
    [SerializeField] private float baseFrequency = 0.5f;
    [SerializeField] private float smoothing = 7f;

    [Header("Layered Noise")]
    [SerializeField] private bool useLayeredNoise = true;
    [SerializeField] private float secondLayerScale = 0.3f;
    [SerializeField] private float secondLayerFrequency = 1.6f;

    [Header("Fade In/Out")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Advanced")]
    [SerializeField] private Vector3 positionFrequencyMultiplier = new Vector3(1f, 1.3f, 0.8f);
    [SerializeField] private Vector3 rotationFrequencyMultiplier = new Vector3(1.2f, 0.9f, 1.1f);

    private Vector3 _noiseOffsets;
    private float _noiseTime;
    private float _enabledTime;
    private bool _initialized;

    private Vector3 _currentPositionOffset;
    private Vector3 _targetPositionOffset;
    private Quaternion _currentRotationOffset = Quaternion.identity;
    private Quaternion _targetRotationOffset = Quaternion.identity;

    private Vector3 _appliedPositionOffset;
    private Quaternion _appliedRotationOffset = Quaternion.identity;

    private void OnEnable()
    {
        _initialized = false;
        _enabledTime = 0f;
        _currentPositionOffset = Vector3.zero;
        _currentRotationOffset = Quaternion.identity;
        _appliedPositionOffset = Vector3.zero;
        _appliedRotationOffset = Quaternion.identity;
    }

    private void OnDisable()
    {
        transform.localPosition -= _appliedPositionOffset;
        transform.localRotation *= Quaternion.Inverse(_appliedRotationOffset);

        _initialized = false;
    }

    private void LateUpdate()
    {
        if (!_initialized)
            Initialize();

        float deltaTime = Time.unscaledDeltaTime;
        _noiseTime += deltaTime * baseFrequency;
        _enabledTime += deltaTime;

        float fadeMultiplier = fadeInDuration > 0
            ? fadeInCurve.Evaluate(Mathf.Clamp01(_enabledTime / fadeInDuration))
            : 1f;

        transform.localPosition -= _appliedPositionOffset;
        transform.localRotation *= Quaternion.Inverse(_appliedRotationOffset);
        _targetPositionOffset = CalculatePositionOffset() * fadeMultiplier;
        _targetRotationOffset = CalculateRotationOffset(fadeMultiplier);

        if (smoothing > 0)
        {
            float smoothSpeed = smoothing * deltaTime;
            _currentPositionOffset = Vector3.Lerp(_currentPositionOffset, _targetPositionOffset, smoothSpeed);
            _currentRotationOffset = Quaternion.Slerp(_currentRotationOffset, _targetRotationOffset, smoothSpeed);
        }
        else
        {
            _currentPositionOffset = _targetPositionOffset;
            _currentRotationOffset = _targetRotationOffset;
        }

        _appliedPositionOffset = transform.localRotation * _currentPositionOffset;
        _appliedRotationOffset = _currentRotationOffset;

        transform.localPosition += _appliedPositionOffset;
        transform.localRotation *= _appliedRotationOffset;
    }

    private Vector3 CalculatePositionOffset()
    {
        Vector3 noise = new Vector3(
            GetLayeredNoise(_noiseTime, _noiseOffsets.x, positionFrequencyMultiplier.x),
            GetLayeredNoise(_noiseTime, _noiseOffsets.y, positionFrequencyMultiplier.y),
            GetLayeredNoise(_noiseTime, _noiseOffsets.z, positionFrequencyMultiplier.z)
        );

        return noise * positionAmplitude;
    }

    private Quaternion CalculateRotationOffset(float fadeMultiplier)
    {
        Vector3 rotationNoise = new Vector3(
            GetLayeredNoise(_noiseTime, _noiseOffsets.x + 500f, rotationFrequencyMultiplier.x),
            GetLayeredNoise(_noiseTime, _noiseOffsets.y + 500f, rotationFrequencyMultiplier.y),
            GetLayeredNoise(_noiseTime, _noiseOffsets.z + 500f, rotationFrequencyMultiplier.z)
        ) * rotationAmplitude * fadeMultiplier;

        return Quaternion.Euler(rotationNoise);
    }

    private float GetLayeredNoise(float time, float offset, float frequencyMult)
    {
        float noise1 = (Mathf.PerlinNoise(time * frequencyMult, offset) - 0.5f) * 2f;

        if (!useLayeredNoise) return noise1;

        float noise2 = (Mathf.PerlinNoise(time * frequencyMult * secondLayerFrequency, offset + 1000f) - 0.5f) * 2f;

        return noise1 + (noise2 * secondLayerScale);
    }

    private void Initialize()
    {
        _initialized = true;
        _noiseTime = 0f;
        _enabledTime = 0f;

        if (_noiseOffsets == Vector3.zero)
            ReSeed();
    }

    public void ReSeed()
    {
        _noiseOffsets = new Vector3(
            Random.Range(-1000f, 1000f),
            Random.Range(-1000f, 1000f),
            Random.Range(-1000f, 1000f)
        );
    }
}