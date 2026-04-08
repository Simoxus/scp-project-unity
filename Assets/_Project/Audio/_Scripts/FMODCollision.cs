using Cysharp.Threading.Tasks;
using FMODUnity;
using System;
using System.Threading;
using UnityEngine;

public class FMODCollision : MonoBehaviour
{
    [Space]
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private EventReference fmodEvent;

    [Header("Timer Settings")]
    [SerializeField] private bool useTimer = false;
    [SerializeField] private float timerDuration = 20f;

    [Header("Performance Settings")]
    [SerializeField] private float minCollisionMagnitude = 0.1f;
    [SerializeField] private float collisionCooldown = 0.1f;

    private bool _collisionsEnabled = true;
    private float _lastCollisionTime;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private async void Start()
    {
        if (useTimer)
        {
            await StartTimerAsync(this.GetCancellationTokenOnDestroy());
        }
    }

    private async UniTask StartTimerAsync(CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(timerDuration),
                DelayType.DeltaTime,
                PlayerLoopTiming.Update,
                cancellationToken
            );

            _collisionsEnabled = false;
        }
        catch (OperationCanceledException ex)
        {
            Log.Exception(ex);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!_collisionsEnabled || Time.time - _lastCollisionTime < collisionCooldown)
            return;

        if (rigidBody == null) return;

        float collisionMagnitude = collision.relativeVelocity.magnitude * rigidBody.mass / 10f;

        if (collisionMagnitude < minCollisionMagnitude)
            return;

        _lastCollisionTime = Time.time;

        PlayCollisionSoundAsync().Forget();
    }

    private async UniTaskVoid PlayCollisionSoundAsync()
    {
        await UniTask.Yield();

        if (Core.AudioManager != null)
        {
            FMODHelper.PlayOneShot3D(
                fmodEvent,
                transform.position,
                useOcclusion: true,
                occlusionMinDuration: 1.5f
            );
        }
        else
        {
            FMODHelper.PlayOneShot3D(fmodEvent, transform.position);
        }
    }
}