using Cysharp.Threading.Tasks;
using FMODUnity;
using System.Threading;
using UnityEngine;

[RequireComponent(typeof(StudioEventEmitter))]
public class FMODEmitterOcclusion : MonoBehaviour
{
    [SerializeField] private bool useOcclusion = true;
    [SerializeField] private float updateInterval = 0.1f;

    [Header("Occlusion Quality")]
    [SerializeField, Range(1, 5)] private int raysPerSound = 1;
    [SerializeField, Range(0f, 45f)] private float raySpread = 0f;

    [Header("Distance")]
    [SerializeField] private bool useCustomMaxDistance = true;
    [SerializeField, Range(0f, 300f)] private float maxDistance = 50f;

    private StudioEventEmitter emitter;
    private int occlusionId = -1;
    private bool wasPlayingLastFrame;
    private CancellationTokenSource cts;

    private void Awake()
    {
        emitter = GetComponent<StudioEventEmitter>();
    }

    private void OnEnable()
    {
        cts = new CancellationTokenSource();
        UpdateLoopAsync(cts.Token).Forget();
    }

    private void OnDisable()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        UnregisterOcclusion();
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        UnregisterOcclusion();
    }

    private async UniTaskVoid UpdateLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (!useOcclusion || emitter == null)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
                continue;
            }

            bool isCurrentlyPlaying = emitter.IsPlaying();

            if (isCurrentlyPlaying && !wasPlayingLastFrame)
            {
                RegisterOcclusion();
            }
            else if (!isCurrentlyPlaying && wasPlayingLastFrame)
            {
                UnregisterOcclusion();
            }
            else if (isCurrentlyPlaying && occlusionId >= 0)
            {
                UpdateOcclusionPosition();
                await UniTask.WaitForSeconds(updateInterval, cancellationToken: token);
                continue;
            }

            wasPlayingLastFrame = isCurrentlyPlaying;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void RegisterOcclusion()
    {
        if (occlusionId >= 0)
        {
            UnregisterOcclusion();
        }

        if (AudioManager.Instance == null)
            return;

        if (emitter != null && emitter.EventInstance.isValid())
        {
            float distance = useCustomMaxDistance ? maxDistance : -1f;
            occlusionId = AudioManager.Instance.RegisterSound(emitter.EventInstance, transform.position, raysPerSound, raySpread, distance);
        }
    }

    private void UnregisterOcclusion()
    {
        if (occlusionId >= 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.UnregisterSound(occlusionId);
        }
        occlusionId = -1;
    }

    private void UpdateOcclusionPosition()
    {
        if (occlusionId >= 0 && AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateSoundPosition(occlusionId, transform.position);
        }
    }

    public void SetOcclusionEnabled(bool enabled)
    {
        useOcclusion = enabled;
        if (!enabled)
        {
            UnregisterOcclusion();
        }
        else if (emitter != null && emitter.IsPlaying())
        {
            RegisterOcclusion();
        }
    }

    public void ForcePositionUpdate()
    {
        if (occlusionId >= 0)
        {
            UpdateOcclusionPosition();
        }
    }

    public int OcclusionId => occlusionId;
    public bool IsRegistered => occlusionId >= 0;
}