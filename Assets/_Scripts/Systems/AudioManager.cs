// please go away

using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
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

    [Header("Performance Optimization")]
    [SerializeField, Range(5f, 50f)] private float priorityDistance = 15f;
    [SerializeField, Range(10, 100)] private int maxRaysPerFrame = 50;
    [SerializeField, Range(0.5f, 5f)] private float performanceBudgetMs = 2f;
    [SerializeField] private bool useJobSystem = true;

    [Header("Distance-Based Falloff")]
    [SerializeField, Range(0.5f, 9f)] private float occlusionFalloffDistance = 2f;
    [SerializeField, Range(0f, 4f)] private float closeProximityMultiplier = 0.3f;

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
        new OcclusionMaterial { tag = "GateFrame", occlusionAmount = 0.4f },
        new OcclusionMaterial { tag = "Door", occlusionAmount = 0.25f }
    };

    [SerializeField, Range(0f, 1f)] private float defaultOcclusionAmount = 0.6f;

    [Header("Debug")]
    [SerializeField] private bool showDebugRays = false;
    [SerializeField] private bool showPerformanceStats = false;

    private Dictionary<int, TrackedSound> _trackedSounds = new Dictionary<int, TrackedSound>();
    private Dictionary<string, float> _materialOcclusionCache;
    private Transform _listenerTransform;
    private int _nextSoundId = 0;
    private CancellationTokenSource _occlusionCts;

    private System.Diagnostics.Stopwatch _perfStopwatch = new System.Diagnostics.Stopwatch();
    private float _lastFrameTimeMs = 0f;
    private int _lastFrameRayCount = 0;
    private int _adaptiveQualityLevel = 2;

    private List<(TrackedSound sound, float distSqr)> _sortedSounds = new List<(TrackedSound, float)>(100);
    private List<RaycastCommand> _raycastCommands = new List<RaycastCommand>(200);
    private List<(TrackedSound sound, int startIndex, int rayCount, float distance)> _raycastResults =
        new List<(TrackedSound, int, int, float)>(100);

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

    protected override void OnAwake()
    {
        // Initialize buses and VCAs
        _gameplayBus = RuntimeManager.GetBus("bus:/Gameplay");
        _persistentBus = RuntimeManager.GetBus("bus:/Persistent");
        _masterVCA = RuntimeManager.GetVCA("vca:/Master");
        _sfxVCA = RuntimeManager.GetVCA("vca:/SFX");
        _musicVCA = RuntimeManager.GetVCA("vca:/Music");
        _voVCA = RuntimeManager.GetVCA("vca:/VO");
        _uiVCA = RuntimeManager.GetVCA("vca:/UI");

        BuildMaterialCache();
    }

    private void Start()
    {
        FindListener();
        _occlusionCts = new CancellationTokenSource();

        // Single unified update loop instead of three separate ones
        UnifiedUpdateLoop(_occlusionCts.Token).Forget();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _occlusionCts?.Cancel();
        _occlusionCts?.Dispose();
    }

    private void BuildMaterialCache()
    {
        _materialOcclusionCache = new Dictionary<string, float>(occlusionMaterials.Length);
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
            lastUpdateFrame = -999
        };

        _trackedSounds[id] = trackedSound;
        return id;
    }

    public void UnregisterSound(int soundId) => _trackedSounds.Remove(soundId);

    public void UpdateSoundPosition(int soundId, Vector3 position)
    {
        if (_trackedSounds.TryGetValue(soundId, out var sound))
        {
            if (Vector3.Distance(sound.position, position) > 0.5f)
            {
                sound.position = position;
            }
        }
    }

    private async UniTaskVoid UnifiedUpdateLoop(CancellationToken token)
    {
        float nextOcclusionUpdate = 0f;
        float nextCleanup = 1f;
        int frameCount = 0;

        while (!token.IsCancellationRequested)
        {
            frameCount++;
            float time = Time.time;

            if (_listenerTransform == null)
            {
                FindListener();
            }

            ApplyOcclusionSmoothing();

            if (occlusionEnabled && time >= nextOcclusionUpdate)
            {
                if (useJobSystem)
                {
                    await CalculateAllOcclusionJobSystemAsync(token);
                }
                else
                {
                    await CalculateAllOcclusionAsync(token);
                }

                nextOcclusionUpdate = time + updateInterval;
            }

            if (time >= nextCleanup)
            {
                CleanupInvalidSounds();
                nextCleanup = time + 1f;
            }

            // Performance stats
            if (showPerformanceStats && frameCount % 60 == 0)
            {
                Debug.Log($"[AudioManager] Last update: {_lastFrameTimeMs:F2}ms, Rays: {_lastFrameRayCount}, Quality: {_adaptiveQualityLevel}, Sounds: {_trackedSounds.Count}");
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private async UniTask CalculateAllOcclusionJobSystemAsync(CancellationToken token)
    {
        if (_listenerTransform == null || _trackedSounds.Count == 0)
            return;

        _perfStopwatch.Restart();

        Vector3 listenerPos = _listenerTransform.position;
        int currentFrame = Time.frameCount;

        // Clear reusable collections
        _sortedSounds.Clear();
        _raycastCommands.Clear();
        _raycastResults.Clear();

        // Collect and sort sounds by distance
        foreach (var sound in _trackedSounds.Values)
        {
            if (!sound.isValid) continue;
            float distSqr = (sound.position - listenerPos).sqrMagnitude;
            _sortedSounds.Add((sound, distSqr));
        }

        _sortedSounds.Sort((a, b) => a.distSqr.CompareTo(b.distSqr));

        // Adaptive ray count based on quality level
        int effectiveRaysPerSound = defaultRaysPerSound;
        int maxRaysThisFrame = maxRaysPerFrame;
        int totalRays = 0;
        int processedSounds = 0;

        // Build raycast commands
        foreach (var (sound, distSqr) in _sortedSounds)
        {
            if (token.IsCancellationRequested)
                break;

            float distance = Mathf.Sqrt(distSqr);

            // Skip if out of range
            if (distance > sound.maxDistance)
            {
                sound.targetOcclusion = 0f;
                continue;
            }

            bool isPriority = distance <= priorityDistance;
            int raysForThisSound = isPriority ? sound.raysPerSound : effectiveRaysPerSound;

            if (!isPriority && totalRays + raysForThisSound > maxRaysThisFrame)
            {
                break;
            }

            Vector3 direction = (listenerPos - sound.position).normalized;
            int startIndex = _raycastCommands.Count;

            for (int i = 0; i < raysForThisSound; i++)
            {
                Vector3 rayDir = direction;

                if (i > 0 && sound.raySpread > 0)
                {
                    float angle = sound.raySpread * Mathf.Deg2Rad;
                    Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * angle;
                    rayDir = (direction + randomOffset).normalized;
                }

                _raycastCommands.Add(new RaycastCommand(
                    sound.position,
                    rayDir,
                    new QueryParameters(occlusionLayers, false, QueryTriggerInteraction.Ignore, false),
                    distance
                ));
            }

            _raycastResults.Add((sound, startIndex, raysForThisSound, distance));
            totalRays += raysForThisSound;
            processedSounds++;
            sound.lastUpdateFrame = currentFrame;

            if (processedSounds % 20 == 0)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        if (_raycastCommands.Count > 0)
        {
            var commandsArray = new NativeArray<RaycastCommand>(_raycastCommands.ToArray(), Allocator.TempJob);
            var resultsArray = new NativeArray<RaycastHit>(_raycastCommands.Count, Allocator.TempJob);

            var raycastJob = RaycastCommand.ScheduleBatch(commandsArray, resultsArray, 16);

            while (!raycastJob.IsCompleted)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                if (_perfStopwatch.ElapsedMilliseconds > performanceBudgetMs * 2)
                {
                    break;
                }
            }

            raycastJob.Complete();

            foreach (var (sound, startIndex, rayCount, distance) in _raycastResults)
            {
                float totalOcclusionWeight = 0f;

                for (int i = 0; i < rayCount; i++)
                {
                    var hit = resultsArray[startIndex + i];

                    if (hit.collider != null && hit.distance < distance - 0.1f)
                    {
                        float materialOcclusion = GetMaterialOcclusionCached(hit.collider);

                        float distanceToWall = hit.distance;
                        float proximityFactor = Mathf.Clamp01(distanceToWall / occlusionFalloffDistance);
                        float proximityMultiplier = Mathf.Lerp(closeProximityMultiplier, 1f, proximityFactor);

                        totalOcclusionWeight += materialOcclusion * proximityMultiplier;

                        if (showDebugRays)
                        {
                            Color debugColor = Color.Lerp(Color.yellow, Color.red, materialOcclusion);
                            Debug.DrawLine(sound.position, hit.point, debugColor, updateInterval);
                        }
                    }
                }

                float occlusionPercent = totalOcclusionWeight / rayCount;
                float newOcclusion = Mathf.Clamp01(occlusionPercent);

                if (Mathf.Abs(newOcclusion - sound.targetOcclusion) > occlusionDeadzone)
                {
                    sound.targetOcclusion = newOcclusion;
                }
            }

            commandsArray.Dispose();
            resultsArray.Dispose();
        }

        _perfStopwatch.Stop();
        _lastFrameTimeMs = (float)_perfStopwatch.Elapsed.TotalMilliseconds;
        _lastFrameRayCount = totalRays;
    }

    private async UniTask CalculateAllOcclusionAsync(CancellationToken token)
    {
        if (_listenerTransform == null)
            return;

        _perfStopwatch.Restart();

        Vector3 listenerPos = _listenerTransform.position;
        int processedCount = 0;
        int totalRays = 0;

        foreach (var sound in _trackedSounds.Values)
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
                totalRays += sound.raysPerSound;

                if (Mathf.Abs(rawOcclusion - sound.targetOcclusion) > occlusionDeadzone)
                {
                    sound.targetOcclusion = rawOcclusion;
                }
            }

            processedCount++;
            if (processedCount % 10 == 0)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (_perfStopwatch.ElapsedMilliseconds > performanceBudgetMs)
            {
                break;
            }
        }

        _perfStopwatch.Stop();
        _lastFrameTimeMs = (float)_perfStopwatch.Elapsed.TotalMilliseconds;
        _lastFrameRayCount = totalRays;
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
                    float materialOcclusion = GetMaterialOcclusionCached(hit.collider);
                    float distanceToWall = hit.distance;
                    float proximityFactor = Mathf.Clamp01(distanceToWall / occlusionFalloffDistance);
                    float proximityMultiplier = Mathf.Lerp(closeProximityMultiplier, 1f, proximityFactor);

                    totalOcclusionWeight += materialOcclusion * proximityMultiplier;
                }
            }
        }

        return Mathf.Clamp01(totalOcclusionWeight / raysPerSound);
    }

    private float GetMaterialOcclusionCached(Collider collider)
    {
        // Use cached dictionary
        if (_materialOcclusionCache.TryGetValue(collider.tag, out float cachedValue))
        {
            return cachedValue;
        }
        return defaultOcclusionAmount;
    }

    private void ApplyOcclusionSmoothing()
    {
        if (!occlusionEnabled)
            return;

        float deltaTime = Time.deltaTime;

        foreach (var sound in _trackedSounds.Values)
        {
            if (!sound.isValid || !sound.instance.isValid())
                continue;

            float occlusionDifference = sound.targetOcclusion - sound.currentOcclusion;
            float adaptiveSpeed = Mathf.Abs(occlusionDifference) > 0.1f ? materialTransitionSpeed : smoothSpeed;

            sound.currentOcclusion = Mathf.Lerp(
                sound.currentOcclusion,
                sound.targetOcclusion,
                deltaTime * adaptiveSpeed
            );

            // Only set parameter if value changed meaningfully
            if (Mathf.Abs(occlusionDifference) > 0.001f)
            {
                sound.instance.setParameterByName(occlusionParameterName, sound.currentOcclusion);
            }
        }
    }

    private void CleanupInvalidSounds()
    {
        var toRemove = new List<int>(16);

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

        if (toRemove.Count > 0 && showDebugRays)
        {
            Debug.Log($"[AudioManager] Cleaned up {toRemove.Count} invalid sounds");
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
        public int lastUpdateFrame;
    }
}