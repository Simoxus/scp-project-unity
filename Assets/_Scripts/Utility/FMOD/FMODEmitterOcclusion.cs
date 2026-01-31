using Cysharp.Threading.Tasks;
using FMODUnity;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(StudioEventEmitter))]
public class FMODEmitterOcclusion : MonoBehaviour
{
    [Space]
    [SerializeField] private bool useOcclusion = true;
    [SerializeField, Range(0.1f, 5f)] private float movementThreshold = 0.5f;

    private StudioEventEmitter _emitter;
    private int _occlusionId = -1;
    private bool _wasPlayingLastFrame;
    private Vector3 _lastPosition;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _emitter = GetComponent<StudioEventEmitter>();
        _lastPosition = transform.position;
    }

    private void OnEnable()
    {
        _cts = new CancellationTokenSource();

        if (useOcclusion && _emitter != null && _emitter.IsPlaying())
        {
            _wasPlayingLastFrame = true;
            RegisterOcclusion();
        }

        MonitorPlaybackAsync(_cts.Token).Forget();
    }

    private void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        UnregisterOcclusion();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        UnregisterOcclusion();
    }

    private async UniTaskVoid MonitorPlaybackAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (!useOcclusion || _emitter == null)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                continue;
            }

            bool isCurrentlyPlaying = _emitter.IsPlaying();

            if (isCurrentlyPlaying && !_wasPlayingLastFrame)
            {
                RegisterOcclusion();
            }
            else if (!isCurrentlyPlaying && _wasPlayingLastFrame)
            {
                UnregisterOcclusion();
            }

            _wasPlayingLastFrame = isCurrentlyPlaying;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void LateUpdate()
    {
        if (_occlusionId >= 0 && useOcclusion)
        {
            Vector3 currentPos = transform.position;
            if (Vector3.Distance(currentPos, _lastPosition) > movementThreshold)
            {
                UpdateOcclusionPosition();
                _lastPosition = currentPos;
            }
        }
    }

    private void RegisterOcclusion()
    {
        if (_occlusionId >= 0) UnregisterOcclusion();
        if (Core.AudioManager == null) return;

        if (_emitter != null && _emitter.EventInstance.isValid())
        {
            _occlusionId = Core.AudioManager.RegisterSound(
                _emitter.EventInstance,
                transform.position,
                useOcclusion: true
            );

            _lastPosition = transform.position;
        }
    }

    private void UnregisterOcclusion()
    {
        if (_occlusionId >= 0 && Core.AudioManager != null)
        {
            Core.AudioManager.UnregisterSound(_occlusionId);
        }

        _occlusionId = -1;
    }

    private void UpdateOcclusionPosition()
    {
        if (_occlusionId >= 0 && Core.AudioManager != null)
        {
            Core.AudioManager.UpdateSoundPosition(_occlusionId, transform.position);
        }
    }

    public void SetOcclusionEnabled(bool enabled)
    {
        useOcclusion = enabled;
        if (!enabled)
        {
            UnregisterOcclusion();
        }
        else if (_emitter != null && _emitter.IsPlaying())
        {
            RegisterOcclusion();
        }
    }

    public void ForcePositionUpdate()
    {
        if (_occlusionId >= 0)
        {
            _lastPosition = transform.position;
            UpdateOcclusionPosition();
        }
    }

    public int OcclusionId => _occlusionId;
    public bool IsRegistered => _occlusionId >= 0;
}