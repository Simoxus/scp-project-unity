using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Global Settings")]
    [SerializeField] private bool occlusionEnabled = true;
    [SerializeField] private string occlusionParameterName = "Occlusion";
    [SerializeField] private LayerMask occlusionLayers = -1;
    [SerializeField, Range(0.05f, 0.5f)] private float updateInterval = 0.15f;
    [SerializeField, Range(1f, 20f)] private float smoothSpeed = 8f;

    [Header("Occlusion Settings")]
    [SerializeField, Range(1, 5)] private int defaultRaysPerSound = 2;
    [SerializeField, Range(0f, 45f)] private float defaultRaySpread = 22f;
    [SerializeField, Range(0f, 100f)] private float defaultMaxDistance = 50f;
    [SerializeField, Range(0f, 0.2f)] private float occlusionDeadzone = 0.05f;

    [Header("Distance-Based Falloff")]
    [SerializeField, Range(0.5f, 5f)] private float occlusionFalloffDistance = 2f;
    [SerializeField, Range(0f, 0.75f)] private float closeProximityMultiplier = 0.3f;

    [Header("Material Transition Smoothing")]
    [SerializeField, Range(1f, 20f)] private float materialTransitionSpeed = 5f;

    [Header("Material Occlusion Settings")]
    [SerializeField]
    private OcclusionMaterial[] occlusionMaterials = new OcclusionMaterial[]
    {
        new OcclusionMaterial { tag = "Tile", occlusionAmount = 0.2f },
        new OcclusionMaterial { tag = "Metal", occlusionAmount = 0.34f },
        new OcclusionMaterial { tag = "Concrete", occlusionAmount = 0.5f },
        new OcclusionMaterial { tag = "Glass", occlusionAmount = 0.23f },
        new OcclusionMaterial { tag = "GateFrame", occlusionAmount = 0.4f }
    };

    [SerializeField, Range(0f, 1f)] private float defaultOcclusionAmount = 0.6f;

    [Header("Immediate Occlusion Check")]
    [SerializeField] private bool useImmediateCheck = true;
    [SerializeField, Range(0.01f, 0.1f)] private float immediateCheckInterval = 0.03f;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = false;
    [SerializeField] private bool showOcclusionDebug = false;

    private Dictionary<int, TrackedSound> _trackedSounds = new Dictionary<int, TrackedSound>();
    private Dictionary<string, float> _materialOcclusionCache;
    private Transform _listenerTransform;
    private int _nextSoundId = 0;
    private CancellationTokenSource _occlusionCts;

    // Bus and VCA management
    private Bus _gameplayBus;
    private Bus _persistentBus;
    private VCA _masterVCA;
    private VCA _sfxVCA;
    private VCA _musicVCA;
    private VCA _voVCA;
    private VCA _uiVCA;

    public void SetMasterVolume(float volume) => _masterVCA.setVolume(Mathf.Clamp01(volume));
    public void SetSFXVolume(float volume) => _sfxVCA.setVolume(Mathf.Clamp01(volume));
    public void SetMusicVolume(float volume) => _musicVCA.setVolume(Mathf.Clamp01(volume));
    public void SetVOVolume(float volume) => _voVCA.setVolume(Mathf.Clamp01(volume));
    public void SetUIVolume(float volume) => _uiVCA.setVolume(Mathf.Clamp01(volume));

    public float GetMasterVolume() { _masterVCA.getVolume(out float v); return v; }
    public float GetSFXVolume() { _sfxVCA.getVolume(out float v); return v; }
    public float GetMusicVolume() { _musicVCA.getVolume(out float v); return v; }
    public float GetVOVolume() { _voVCA.getVolume(out float v); return v; }
    public float GetUIVolume() { _uiVCA.getVolume(out float v); return v; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Log.VerboseWarning($"Duplicate instance of {GetType().Name} found. Destroying the new one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize buses and VCAs
        _gameplayBus = RuntimeManager.GetBus("bus:/Gameplay");
        _persistentBus = RuntimeManager.GetBus("bus:/Persistent");
        _masterVCA = RuntimeManager.GetVCA("vca:/Master");
        _sfxVCA = RuntimeManager.GetVCA("vca:/SFX");
        _musicVCA = RuntimeManager.GetVCA("vca:/Music");
        _voVCA = RuntimeManager.GetVCA("vca:/VO");
        _uiVCA = RuntimeManager.GetVCA("vca:/UI");

        // Build material occlusion cache
        BuildMaterialCache();
    }

    private void Start()
    {
        FindListener();

        _occlusionCts = new CancellationTokenSource();

        // Start async update loops
        OcclusionCalculationLoop(_occlusionCts.Token).Forget();
        OcclusionApplicationLoop(_occlusionCts.Token).Forget();
        CleanupLoop(_occlusionCts.Token).Forget();

        if (useImmediateCheck)
        {
            ImmediateOcclusionCheckLoop(_occlusionCts.Token).Forget();
        }
    }

    private void OnDestroy()
    {
        _occlusionCts?.Cancel();
        _occlusionCts?.Dispose();
    }

    private void BuildMaterialCache()
    {
        _materialOcclusionCache = new Dictionary<string, float>();
        foreach (var material in occlusionMaterials)
        {
            if (!string.IsNullOrEmpty(material.tag))
            {
                _materialOcclusionCache[material.tag] = Mathf.Clamp01(material.occlusionAmount);
            }
        }
    }

    private void FindListener()
    {
        StudioListener listener = FindAnyObjectByType<StudioListener>();
        _listenerTransform = listener != null ? listener.transform : Camera.main?.transform;
    }

    public void ToggleGameSounds(bool doPause) => _gameplayBus.setPaused(doPause);

    public void SetOcclusionEnabled(bool enabled)
    {
        occlusionEnabled = enabled;

        if (!enabled)
        {
            foreach (var sound in _trackedSounds.Values)
            {
                if (sound.isValid && sound.instance.isValid())
                {
                    sound.instance.setParameterByName(occlusionParameterName, 0f);
                    sound.currentOcclusion = 0f;
                    sound.targetOcclusion = 0f;
                }
            }
        }
    }

    public bool IsOcclusionEnabled() => occlusionEnabled;

    public int RegisterSound(EventInstance instance, Vector3 position, int raysPerSound = -1, float raySpread = -1f, float maxDistance = -1f)
    {
        int id = _nextSoundId++;

        var trackedSound = new TrackedSound
        {
            instance = instance,
            position = position,
            raysPerSound = raysPerSound < 0 ? defaultRaysPerSound : Mathf.Clamp(raysPerSound, 1, 5),
            raySpread = raySpread < 0 ? defaultRaySpread : Mathf.Clamp(raySpread, 0f, 45f),
            maxDistance = maxDistance < 0 ? defaultMaxDistance : maxDistance,
            needsImmediateCheck = true
        };

        _trackedSounds[id] = trackedSound;

        if (useImmediateCheck && _listenerTransform != null)
        {
            PerformImmediateOcclusionCheck(trackedSound);
        }

        return id;
    }

    public void UnregisterSound(int soundId) => _trackedSounds.Remove(soundId);

    public void UpdateSoundPosition(int soundId, Vector3 position)
    {
        if (_trackedSounds.TryGetValue(soundId, out var sound))
        {
            sound.position = position;
            sound.needsImmediateCheck = true;
        }
    }

    private async UniTaskVoid ImmediateOcclusionCheckLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_listenerTransform != null && occlusionEnabled)
            {
                foreach (var sound in _trackedSounds.Values)
                {
                    if (sound.needsImmediateCheck && sound.isValid)
                    {
                        PerformImmediateOcclusionCheck(sound);
                        sound.needsImmediateCheck = false;
                    }
                }
            }

            await UniTask.Delay(TimeSpan.FromSeconds(immediateCheckInterval), cancellationToken: token);
        }
    }

    private void PerformImmediateOcclusionCheck(TrackedSound sound)
    {
        if (_listenerTransform == null || !occlusionEnabled)
            return;

        Vector3 listenerPos = _listenerTransform.position;
        float distance = Vector3.Distance(sound.position, listenerPos);

        if (distance > sound.maxDistance)
        {
            sound.targetOcclusion = 0f;
            // Don't reset currentOcclusion - let it smooth to 0
        }
        else
        {
            float rawOcclusion = CalculateOcclusion(sound.position, listenerPos, sound.raysPerSound, sound.raySpread, distance);
            sound.targetOcclusion = rawOcclusion;
            // Don't set currentOcclusion directly - let smoothing handle it
        }
    }

    private async UniTaskVoid OcclusionCalculationLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_listenerTransform == null)
            {
                FindListener();
            }
            else
            {
                await CalculateAllOcclusionAsync(token);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(updateInterval), cancellationToken: token);
        }
    }

    private async UniTaskVoid OcclusionApplicationLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            ApplyOcclusion();
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private async UniTaskVoid CleanupLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            CleanupInvalidSounds();
            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: token);
        }
    }

    private async UniTask CalculateAllOcclusionAsync(CancellationToken token)
    {
        if (_listenerTransform == null || !occlusionEnabled)
            return;

        Vector3 listenerPos = _listenerTransform.position;
        List<TrackedSound> soundsList = new List<TrackedSound>(_trackedSounds.Values);

        foreach (var sound in soundsList)
        {
            if (token.IsCancellationRequested || !sound.isValid)
                break;

            float distance = Vector3.Distance(sound.position, listenerPos);

            if (distance > sound.maxDistance)
            {
                sound.targetOcclusion = 0f;
            }
            else
            {
                float rawOcclusion = CalculateOcclusion(sound.position, listenerPos, sound.raysPerSound, sound.raySpread, distance);

                if (Mathf.Abs(rawOcclusion - sound.targetOcclusion) > occlusionDeadzone)
                {
                    sound.targetOcclusion = rawOcclusion;
                }
            }

            if (soundsList.Count > 10)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
    }

    private float CalculateOcclusion(Vector3 soundPos, Vector3 listenerPos, int raysPerSound, float raySpread, float totalDistance)
    {
        if (totalDistance < 0.1f)
            return 0f;

        Vector3 direction = (listenerPos - soundPos).normalized;
        float totalOcclusionWeight = 0f;

        for (int i = 0; i < raysPerSound; i++)
        {
            Vector3 rayDir = direction;

            if (i > 0 && raySpread > 0)
            {
                float angle = raySpread * Mathf.Deg2Rad;
                Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * angle;
                rayDir = (direction + randomOffset).normalized;
            }

            if (Physics.Raycast(soundPos, rayDir, out RaycastHit hit, totalDistance, occlusionLayers))
            {
                if (hit.distance < totalDistance - 0.1f)
                {
                    // Get material-specific occlusion
                    float materialOcclusion = GetMaterialOcclusion(hit.collider);

                    // Apply distance-based falloff (reduces occlusion when close to wall)
                    float distanceToWall = hit.distance;
                    float proximityFactor = Mathf.Clamp01(distanceToWall / occlusionFalloffDistance);
                    float proximityMultiplier = Mathf.Lerp(closeProximityMultiplier, 1f, proximityFactor);

                    float finalOcclusion = materialOcclusion * proximityMultiplier;
                    totalOcclusionWeight += finalOcclusion;

                    if (showDebugRays)
                    {
                        Color debugColor = Color.Lerp(Color.yellow, Color.red, finalOcclusion);
                        Debug.DrawLine(soundPos, hit.point, debugColor, updateInterval);
                    }
                }
                else if (showDebugRays)
                {
                    Debug.DrawLine(soundPos, listenerPos, Color.green, updateInterval);
                }
            }
            else if (showDebugRays)
            {
                Debug.DrawLine(soundPos, listenerPos, Color.green, updateInterval);
            }
        }

        float occlusionPercent = totalOcclusionWeight / raysPerSound;
        return Mathf.Clamp01(occlusionPercent);
    }

    private float GetMaterialOcclusion(Collider collider)
    {
        // Use CompareTag for performance
        foreach (var material in occlusionMaterials)
        {
            if (!string.IsNullOrEmpty(material.tag) && collider.CompareTag(material.tag))
            {
                return material.occlusionAmount;
            }
        }

        return defaultOcclusionAmount;
    }

    private void ApplyOcclusion()
    {
        if (!occlusionEnabled)
            return;

        float deltaTime = Time.deltaTime;

        foreach (var sound in _trackedSounds.Values)
        {
            if (!sound.isValid || !sound.instance.isValid())
                continue;

            // Determine appropriate smoothing speed based on change direction and magnitude
            float occlusionDifference = sound.targetOcclusion - sound.currentOcclusion;
            float adaptiveSpeed = smoothSpeed;

            // Use material transition speed when there's a significant change
            // This smooths out material transitions
            if (Mathf.Abs(occlusionDifference) > 0.1f)
            {
                adaptiveSpeed = materialTransitionSpeed;
            }

            // Smooth transition with adaptive speed
            sound.currentOcclusion = Mathf.Lerp(
                sound.currentOcclusion,
                sound.targetOcclusion,
                deltaTime * adaptiveSpeed
            );

            // Apply to FMOD parameter
            sound.instance.setParameterByName(occlusionParameterName, sound.currentOcclusion);

            if (showOcclusionDebug && sound.currentOcclusion > 0.01f)
            {
                Debug.Log($"Applied Occlusion: current={sound.currentOcclusion:F3}, target={sound.targetOcclusion:F3}, diff={occlusionDifference:F3}");
            }
        }
    }

    private void CleanupInvalidSounds()
    {
        var toRemove = new List<int>();

        foreach (var kvp in _trackedSounds)
        {
            var result = kvp.Value.instance.getPlaybackState(out PLAYBACK_STATE state);

            if (result != FMOD.RESULT.OK ||
                state == PLAYBACK_STATE.STOPPED ||
                !kvp.Value.instance.isValid())
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var id in toRemove)
        {
            _trackedSounds.Remove(id);
        }
    }

    [Serializable]
    private class OcclusionMaterial
    {
        public string tag;
        [Range(0f, 1f)] public float occlusionAmount;
    }

    private class TrackedSound
    {
        public EventInstance instance;
        public Vector3 position;
        public float currentOcclusion;
        public float targetOcclusion;
        public bool isValid = true;
        public int raysPerSound;
        public float raySpread;
        public float maxDistance;
        public bool needsImmediateCheck;
    }
}