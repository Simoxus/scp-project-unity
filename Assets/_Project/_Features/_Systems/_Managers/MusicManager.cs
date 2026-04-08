using Cysharp.Threading.Tasks;
using FMOD.Studio;
using FMODUnity;
using System;
using System.Threading;
using UnityEngine;

public class MusicManager : Singleton<MusicManager>
{
    [Space]
    [SerializeField] private EventReference defaultMusic;
    [SerializeField] private float crossfadeDuration = 2f;
    [SerializeField] private float gracePeriod = 1f;

    private EventInstance _currentMusicInstance;
    private EventReference _currentMusicReference;
    private EventReference _pendingMusicReference;
    private EventReference _currentZoneMusic;
    private bool _isCrossfading;
    private CancellationTokenSource _gracePeriodCts;

    private EventReference _currentAmbientLoop;
    private float _currentMinPlayInterval;
    private float _currentMaxPlayInterval;
    private CancellationTokenSource _ambientLoopCts;

    protected override void OnSingletonAwake()
    {
        _currentMusicReference = default;
        _pendingMusicReference = default;
    }

    protected override void OnSingletonDestroy()
    {
        _gracePeriodCts?.Cancel();
        _gracePeriodCts?.Dispose();

        _ambientLoopCts?.Cancel();
        _ambientLoopCts?.Dispose();

        if (_currentMusicInstance.isValid())
        {
            _currentMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _currentMusicInstance.release();
        }
    }

    public void PlayDefaultMusic()
    {
        PlayMusic(defaultMusic);
    }

    public void PlayMusic(EventReference musicEvent)
    {
        if (musicEvent.IsNull) return;

        _gracePeriodCts?.Cancel();
        _gracePeriodCts?.Dispose();
        _gracePeriodCts = null;

        if (IsSameMusic(musicEvent, _currentMusicReference))
        {
            _pendingMusicReference = default;
            return;
        }

        _pendingMusicReference = default;

        if (_isCrossfading)
        {
            CrossfadeToMusic(musicEvent).Forget();
        }
        else
        {
            if (_currentMusicInstance.isValid())
            {
                CrossfadeToMusic(musicEvent).Forget();
            }
            else
            {
                StartMusic(musicEvent);
            }
        }
    }

    public void PlayMusicWithGracePeriod(EventReference musicEvent)
    {
        if (musicEvent.IsNull) return;

        if (IsSameMusic(musicEvent, _currentMusicReference))
        {
            _gracePeriodCts?.Cancel();
            _gracePeriodCts?.Dispose();
            _gracePeriodCts = null;
            _pendingMusicReference = default;
            return;
        }

        if (IsSameMusic(musicEvent, _pendingMusicReference))
        {
            return;
        }

        _gracePeriodCts?.Cancel();
        _gracePeriodCts?.Dispose();

        _pendingMusicReference = musicEvent;
        _gracePeriodCts = new CancellationTokenSource();

        HandleGracePeriod(_gracePeriodCts.Token).Forget();
    }

    public void StopMusic(bool immediate = false)
    {
        _gracePeriodCts?.Cancel();
        _gracePeriodCts?.Dispose();
        _gracePeriodCts = null;
        _pendingMusicReference = default;

        if (_currentMusicInstance.isValid())
        {
            if (immediate)
            {
                _currentMusicInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                _currentMusicInstance.release();
            }
            else
            {
                FadeOutMusic().Forget();
            }
        }

        _currentMusicReference = default;
    }

    public void SetZoneMusic(EventReference zoneMusic)
    {
        _currentZoneMusic = zoneMusic;
    }

    public void PlayZoneMusic()
    {
        if (!_currentZoneMusic.IsNull)
        {
            PlayMusicWithGracePeriod(_currentZoneMusic);
        }
        else if (!defaultMusic.IsNull)
        {
            PlayMusicWithGracePeriod(defaultMusic);
        }
    }

    public void SetAmbientLoop(EventReference ambientLoop, float minInterval, float maxInterval)
    {
        _ambientLoopCts?.Cancel();
        _ambientLoopCts?.Dispose();
        _ambientLoopCts = null;

        _currentAmbientLoop = ambientLoop;
        _currentMinPlayInterval = minInterval;
        _currentMaxPlayInterval = maxInterval;

        if (!ambientLoop.IsNull)
        {
            _ambientLoopCts = new CancellationTokenSource();
            PlayAmbientLoopRoutine(_ambientLoopCts.Token).Forget();
        }
    }

    public void StopAmbientLoop()
    {
        _ambientLoopCts?.Cancel();
        _ambientLoopCts?.Dispose();
        _ambientLoopCts = null;

        _currentAmbientLoop = default;
    }

    private async UniTaskVoid PlayAmbientLoopRoutine(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                float waitTime = UnityEngine.Random.Range(_currentMinPlayInterval, _currentMaxPlayInterval);
                await UniTask.Delay(TimeSpan.FromSeconds(waitTime), cancellationToken: cancellationToken);

                if (!cancellationToken.IsCancellationRequested && !_currentAmbientLoop.IsNull)
                {
                    RuntimeManager.PlayOneShot(_currentAmbientLoop);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async UniTaskVoid HandleGracePeriod(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(gracePeriod), cancellationToken: cancellationToken);

            if (!cancellationToken.IsCancellationRequested && !_pendingMusicReference.IsNull)
            {
                EventReference musicToPlay = _pendingMusicReference;
                _pendingMusicReference = default;
                PlayMusic(musicToPlay);
            }
        }
        catch (OperationCanceledException ex)
        {
            Log.Exception(ex);
        }
    }

    private void StartMusic(EventReference musicEvent)
    {
        _currentMusicInstance = RuntimeManager.CreateInstance(musicEvent);
        _currentMusicInstance.start();
        _currentMusicReference = musicEvent;
    }

    private async UniTaskVoid CrossfadeToMusic(EventReference newMusic)
    {
        _isCrossfading = true;

        EventInstance oldInstance = _currentMusicInstance;
        EventInstance newInstance = RuntimeManager.CreateInstance(newMusic);

        newInstance.setVolume(0f);
        newInstance.start();

        float elapsed = 0f;
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);

            if (oldInstance.isValid())
            {
                oldInstance.setVolume(1f - t);
            }

            newInstance.setVolume(t);

            await UniTask.Yield();
        }

        if (oldInstance.isValid())
        {
            oldInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            oldInstance.release();
        }

        _currentMusicInstance = newInstance;
        _currentMusicReference = newMusic;
        _isCrossfading = false;
    }

    private async UniTaskVoid FadeOutMusic()
    {
        EventInstance instance = _currentMusicInstance;
        float elapsed = 0f;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);

            if (instance.isValid())
            {
                instance.setVolume(1f - t);
            }

            await UniTask.Yield();
        }

        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            instance.release();
        }
    }

    private bool IsSameMusic(EventReference a, EventReference b)
    {
        if (a.IsNull && b.IsNull) return true;
        if (a.IsNull || b.IsNull) return false;
        return a.Guid.Equals(b.Guid);
    }
}