using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Space]
    [SerializeField] private bool occlusionEnabled = true;
    [SerializeField] private string occlusionParameterName = "Occlusion";

    [Header("Performance")]
    [SerializeField, Range(0.05f, 0.5f)] private float updateInterval = 0.15f;
    [SerializeField, Range(1f, 20f)] private float smoothSpeed = 10f;
    [SerializeField, Range(0f, 0.2f)] private float parameterDeadzone = 0.01f;

    [Header("Occlusion Settings")]
    [SerializeField] private LayerMask occlusionMask = -1;
    [SerializeField, Range(0f, 1f)] private float maxOcclusion = 0.95f;
    [SerializeField, Range(0f, 50f)] private float maxOcclusionDistance = 30f;

    // Bus and VCA management
    private Bus _gameplayBus;
    private Bus _persistentBus;
    private VCA _masterVCA, _sfxVCA, _musicVCA, _voVCA, _uiVCA;

    private Dictionary<int, TrackedSound> _trackedSounds = new Dictionary<int, TrackedSound>();
    private Transform _listenerTransform;
    private int _nextSoundId = 0;
    private CancellationTokenSource _updateCts;
    private RaycastHit[] _raycastHits;
    private const int MAX_RAYCAST_HITS = 8;
    private float _lastUpdateTime = 0f;
    private System.Diagnostics.Stopwatch _perfStopwatch = new System.Diagnostics.Stopwatch();

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

    protected override void OnSingletonAwake()
    {
        // Initialize
        _gameplayBus = RuntimeManager.GetBus("bus:/Gameplay");
        _persistentBus = RuntimeManager.GetBus("bus:/Persistent");
        _masterVCA = RuntimeManager.GetVCA("vca:/Master");
        _sfxVCA = RuntimeManager.GetVCA("vca:/SFX");
        _musicVCA = RuntimeManager.GetVCA("vca:/Music");
        _voVCA = RuntimeManager.GetVCA("vca:/VO");
        _uiVCA = RuntimeManager.GetVCA("vca:/UI");

        _raycastHits = new RaycastHit[MAX_RAYCAST_HITS];
    }

    private void Start()
    {
        FindListener();
        _updateCts = new CancellationTokenSource();
        UpdateLoop(_updateCts.Token).Forget();
    }

    protected override void OnSingletonDestroy()
    {
        _updateCts?.Cancel();
        _updateCts?.Dispose();
    }

    private void FindListener()
    {
        StudioListener listener = FindAnyObjectByType<StudioListener>();
        _listenerTransform = listener != null ? listener.transform : Camera.main?.transform;
    }

    public void ToggleGameSounds(bool pause) => _gameplayBus.setPaused(pause);

    public void SetOcclusionEnabled(bool enabled)
    {
        occlusionEnabled = enabled;
        if (!enabled)
        {
            foreach (var sound in _trackedSounds.Values)
            {
                if (sound.instance.isValid())
                {
                    sound.instance.setParameterByName(occlusionParameterName, 0f);
                    sound.currentOcclusion = 0f;
                    sound.targetOcclusion = 0f;
                }
            }
        }
    }

    public bool IsOcclusionEnabled() => occlusionEnabled;

    public int RegisterSound(EventInstance instance, Vector3 position, float maxDistance = -1f, bool useOcclusion = true)
    {
        if (!instance.isValid()) return -1;

        int id = _nextSoundId++;
        var tracked = new TrackedSound
        {
            instance = instance,
            position = position,
            maxDistance = maxDistance < 0 ? maxOcclusionDistance : maxDistance,
            wantsOcclusion = useOcclusion
        };
        _trackedSounds[id] = tracked;

        return id;
    }

    public void UnregisterSound(int id)
    {
        _trackedSounds.Remove(id);
    }

    public void UpdateSoundPosition(int id, Vector3 position)
    {
        if (_trackedSounds.TryGetValue(id, out var sound))
            sound.position = position;
    }

    private async UniTaskVoid UpdateLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_listenerTransform == null) FindListener();

            ApplySmoothing();

            if (Time.time - _lastUpdateTime >= updateInterval)
            {
                await CalculateOcclusion(token);
                _lastUpdateTime = Time.time;
            }

            if (Time.frameCount % 60 == 0)
            {
                CleanupInvalidSounds();
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private async UniTask CalculateOcclusion(CancellationToken token)
    {
        if (_listenerTransform == null || _trackedSounds.Count == 0 || !occlusionEnabled)
            return;

        _perfStopwatch.Restart();
        Vector3 listenerPos = _listenerTransform.position;
        int raycastCount = 0;

        foreach (var sound in _trackedSounds.Values)
        {
            if (token.IsCancellationRequested) break;
            if (!sound.instance.isValid()) continue;
            if (!sound.wantsOcclusion) continue;

            float distance = Vector3.Distance(sound.position, listenerPos);

            // Skip if out of range
            if (distance > sound.maxDistance)
            {
                sound.targetOcclusion = 0f;
                continue;
            }

            // Skip if distance is too small
            if (distance < 0.1f)
            {
                sound.targetOcclusion = 0f;
                continue;
            }

            Vector3 direction = (sound.position - listenerPos).normalized;
            bool isOccluded = false;
            float hitDistance = 0f;

            // Non-allocating raycast for better performance
            int hitCount = Physics.RaycastNonAlloc(
                listenerPos,
                direction,
                _raycastHits,
                distance,
                occlusionMask,
                QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < hitCount; i++)
            {
                if (_raycastHits[i].distance < distance - 0.01f)
                {
                    isOccluded = true;
                    hitDistance = _raycastHits[i].distance;
                    break;
                }
            }

            raycastCount++;

            if (isOccluded)
            {
                float normalizedDistance = hitDistance / distance;
                float occlusionFactor = 1f - normalizedDistance;

                sound.targetOcclusion = Mathf.Lerp(0.3f, maxOcclusion, occlusionFactor);
            }
            else
            {
                sound.targetOcclusion = 0f;
            }

            // Yield periodically to avoid frame spikes
            if (raycastCount % 10 == 0)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        _perfStopwatch.Stop();
    }

    private void ApplySmoothing()
    {
        if (!occlusionEnabled) return;

        float dt = Time.deltaTime;

        foreach (var sound in _trackedSounds.Values)
        {
            if (!sound.instance.isValid()) continue;
            if (!sound.wantsOcclusion) continue;
            if (!sound.hasOcclusionParameter) continue;

            sound.currentOcclusion = Mathf.Lerp(sound.currentOcclusion, sound.targetOcclusion, dt * smoothSpeed);

            if (Mathf.Abs(sound.currentOcclusion - sound.lastAppliedOcclusion) > parameterDeadzone)
            {
                var result = sound.instance.setParameterByName(occlusionParameterName, sound.currentOcclusion);

                // If parameter not found, cache
                if (result == FMOD.RESULT.ERR_EVENT_NOTFOUND)
                {
                    sound.hasOcclusionParameter = false;
                    continue;
                }

                sound.lastAppliedOcclusion = sound.currentOcclusion;
            }
        }
    }

    private void CleanupInvalidSounds()
    {
        var toRemove = new List<int>();

        foreach (var kvp in _trackedSounds)
        {
            if (!kvp.Value.instance.isValid())
            {
                toRemove.Add(kvp.Key);
                continue;
            }

            var result = kvp.Value.instance.getPlaybackState(out PLAYBACK_STATE state);
            if (result != FMOD.RESULT.OK || state == PLAYBACK_STATE.STOPPED)
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var id in toRemove)
        {
            _trackedSounds.Remove(id);
        }
    }

    private class TrackedSound
    {
        public EventInstance instance;
        public Vector3 position;
        public float maxDistance;
        public bool wantsOcclusion;

        public float targetOcclusion;
        public float currentOcclusion;
        public float lastAppliedOcclusion;
        public bool hasOcclusionParameter = true;
    }
}