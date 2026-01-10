using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;

public abstract class BaseDoorController : MonoBehaviour
{
    public enum DoorState
    {
        Moving,
        Opened,
        Closed,
        Broken
    }

    [Header("Initial State")]
    public bool locked = false;
    [SerializeField, ShowField(nameof(locked))]
    public string lockedMessage = string.Empty;
    [Space]
    public bool startOpened = false;
    [SerializeField, ShowField(nameof(startOpened))]
    public bool chanceToStartOpened = false;
    [SerializeField, ShowField(nameof(chanceToStartOpened)), Range(0, 1)]
    public float percentChanceToStartOpened = 0.5f;

    [Header("Color States")]
    public Color32 defaultColor = new Color32(255, 255, 255, 255); // white
    public Color32 successColor = new Color32(212, 255, 203, 255); // light green
    public Color32 failureColor = new Color32(255, 153, 158, 255); // light red
    public Color32 movingColor = new Color32(255, 244, 153, 255); // light yellow
    public Color32 brokenColor = new Color32(255, 115, 88, 255); // salmon
    public Color32 lockedColor = new Color32(255, 153, 158, 255); // light red

    public virtual Color32 DefaultStateColor => defaultColor;
    public virtual Color32 SuccessStateColor => successColor;
    public virtual Color32 FailureStateColor => failureColor;
    public virtual Color32 MovingStateColor => movingColor;
    public virtual Color32 BrokenStateColor => brokenColor;
    public virtual Color32 LockedStateColor => lockedColor;

    [Header("Environment Settings")]
    public bool breakableByEnvironment = true;

    [Header("FMOD Settings")]
    public string fmodParameterName = "State";

    [Header("Door Visuals")]
    public GameObject door;
    public GameObject doorFront;
    public GameObject doorBack;
    public ParticleSystem sparksEmitter;

    [Header("Door Offsets")]
    public Vector3 doorFrontOpenOffset = new Vector3(-4.3f, 0f, 0f);
    public Vector3 doorBackOpenOffset = new Vector3(4.3f, 0f, 0f);
    public Vector3 doorFrontCloseOffset;
    public Vector3 doorBackCloseOffset;

    [Header("Tweening & Physics")]
    public float doorMoveDuration = 1.5f;
    public float doorBreakForce = 27f;
    public float doorBreakDownwardForce = 15f;
    public float doorBreakTorque = 35f;
    public Ease easeStyle = Ease.InOutQuad;
    public Vector2 impulseVelocityRange = new Vector2(1.6f, 2f);

    [Header("State Values"), ReadOnly]
    public DoorState currentState = DoorState.Closed;

    private int _debrisLayer;
    private Vector3 _initialFrontLocalPosition;
    private Vector3 _initialBackLocalPosition;
    protected CancellationTokenSource _doorCts;

    protected virtual void Awake()
    {
        _debrisLayer = LayerMask.NameToLayer("Debris");

        if (doorFront != null)
        {
            _initialFrontLocalPosition = doorFront.transform.localPosition;
        }

        if (doorBack != null)
        {
            _initialBackLocalPosition = doorBack.transform.localPosition;
        }

        if (startOpened && currentState is not (DoorState.Opened or DoorState.Broken))
        {
            bool shouldOpen = true;

            if (chanceToStartOpened)
            {
                float randomValue = UnityEngine.Random.Range(0f, 1f);
                if (randomValue > percentChanceToStartOpened)
                {
                    shouldOpen = false;
                }
            }

            if (shouldOpen)
            {
                currentState = DoorState.Opened;
                doorFront.transform.localPosition = _initialFrontLocalPosition + doorFrontOpenOffset;
                doorBack.transform.localPosition = _initialBackLocalPosition + doorBackOpenOffset;
            }
        }

        _doorCts = new CancellationTokenSource();
    }

    protected virtual void Start()
    {
        if (locked)
        {
            ApplyLockedState();
        }
        else
        {
            ApplyOpenableState();
        }
    }

    protected virtual void OnDestroy()
    {
        if (_doorCts != null && !_doorCts.IsCancellationRequested)
        {
            _doorCts.Cancel();
        }
        _doorCts?.Dispose();
        _doorCts = null;
    }

    [ContextMenu("Toggle Door")]
    public async UniTask ToggleDoor()
    {
        if (_doorCts == null || _doorCts.IsCancellationRequested)
        {
            return;
        }

        if (currentState == DoorState.Moving || currentState == DoorState.Broken || door == null)
        {
            return;
        }

        if (locked)
        {
            return;
        }

        bool shouldOpen = currentState == DoorState.Closed;
        float fmodParameterValue = shouldOpen ? 1.0f : 0.0f;

        await SetActivatorsState(enabled: false);

        FMODHelper.PlayOneShotWithParameters(
            Core.AudioDataAccess.Doors.DoorSound,
            door.transform.position,
            parameters: (fmodParameterName, fmodParameterValue)
        );

        SetDoorState(DoorState.Moving);
        StartActivatorsPulse(MovingStateColor, 0.5f, 1.2f);

        try
        {
            if (shouldOpen)
            {
                await OpenDoorAsync(_doorCts.Token);
            }
            else
            {
                await CloseDoorAsync(_doorCts.Token);
            }

            await SetActivatorsState(true);
        }
        catch (OperationCanceledException)
        {
            Log.VerboseInfo("ToggleDoor operation was cancelled.");
            SetDoorState(shouldOpen ? DoorState.Closed : DoorState.Opened);
            await SetActivatorsState(true);
        }
        catch (Exception ex)
        {
            Log.VerboseWarning($"ToggleDoor operation failed: {ex}");
            SetDoorState(shouldOpen ? DoorState.Closed : DoorState.Opened);
            await SetActivatorsState(true);
        }
    }

    private async UniTask OpenDoorAsync(CancellationToken token)
    {
        if (doorFront == null || doorBack == null) return;

        Vector3 targetPosFront = _initialFrontLocalPosition + doorFrontOpenOffset;
        Vector3 targetPosBack = _initialBackLocalPosition + doorBackOpenOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(doorFront.transform, targetPosFront, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(doorBack.transform, targetPosBack, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(token);

        if (this == null || CameraManager.Instance == null)
            return;

        CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.04f, 0.06f));

        SetDoorState(DoorState.Opened);
        StopActivatorsPulse();
        ApplyOpenableState();
    }

    private async UniTask CloseDoorAsync(CancellationToken token)
    {
        if (doorFront == null || doorBack == null) return;

        Vector3 targetPosFront = _initialFrontLocalPosition + doorFrontCloseOffset;
        Vector3 targetPosBack = _initialBackLocalPosition + doorBackCloseOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(doorFront.transform, targetPosFront, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(doorBack.transform, targetPosBack, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(token);

        if (this == null || CameraManager.Instance == null)
            return;

        CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.04f, 0.06f));

        SetDoorState(DoorState.Closed);
        StopActivatorsPulse();
        ApplyOpenableState();
    }

    [ContextMenu("Break Door")]
    public async UniTask BreakDoor()
    {
        if (doorFront == null || doorBack == null) return;
        if (currentState == DoorState.Broken) return;

        // Cancel any ongoing door operations
        _doorCts.Cancel();
        _doorCts = new CancellationTokenSource();

        // Force close the door first if it's not already closed
        doorFront.transform.localPosition = _initialFrontLocalPosition + doorFrontCloseOffset;
        doorBack.transform.localPosition = _initialBackLocalPosition + doorBackCloseOffset;

        SetDoorState(DoorState.Broken);
        ApplyBrokenState();

        if (door != null)
        {
            FMODHelper.PlayOneShotWithDynamicOcclusion(Core.AudioDataAccess.Doors.DoorBreakSound, door.transform.position, 1.5f);
            FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.DoorBrokenSound, door.transform.position);

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(impulseVelocityRange.x, impulseVelocityRange.y));
            }

            sparksEmitter?.Play();

            Rigidbody frontRigidbody = doorFront.GetComponent<Rigidbody>();
            Rigidbody backRigidbody = doorBack.GetComponent<Rigidbody>();

            Collider frontCollider = doorFront.GetComponent<Collider>();
            Collider backCollider = doorBack.GetComponent<Collider>();

            if (frontCollider != null && backCollider != null)
            {
                Physics.IgnoreCollision(frontCollider, backCollider, true);
            }

            await UniTask.WaitForSeconds(0.4f, ignoreTimeScale: false);

            if (frontRigidbody != null)
            {
                doorFront.layer = _debrisLayer;
                frontRigidbody.isKinematic = false;
                frontRigidbody.AddForce(doorFront.transform.forward * doorBreakForce + Vector3.down * doorBreakDownwardForce, ForceMode.Impulse);
                frontRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * doorBreakTorque, ForceMode.Impulse);
            }

            if (backRigidbody != null)
            {
                doorBack.layer = _debrisLayer;
                backRigidbody.isKinematic = false;
                backRigidbody.AddForce(doorBack.transform.forward * doorBreakForce + Vector3.down * doorBreakDownwardForce, ForceMode.Impulse);
                backRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * doorBreakTorque, ForceMode.Impulse);
            }
        }
    }

    private void SetDoorState(DoorState newState)
    {
        currentState = newState;
    }

    private async UniTask SetActivatorsState(bool enabled)
    {
        await UniTask.WaitForSeconds(0.09f, ignoreTimeScale: false);
        OnSetActivatorsState(enabled);
    }

    private void ApplyOpenableState()
    {
        StopActivatorsPulse();
        OnResetActivatorVisuals(SuccessStateColor);
    }

    private void ApplyLockedState()
    {
        StopActivatorsPulse();
        OnApplyLockedVisuals(LockedStateColor, lockedMessage);
        StartActivatorsPulse(LockedStateColor);
    }

    private void ApplyBrokenState()
    {
        StopActivatorsPulse();
        OnApplyBrokenVisuals(BrokenStateColor);
        StartActivatorsPulse(BrokenStateColor);
    }

    private void StartActivatorsPulse(Color color, float? customDuration = null, float? customIntensity = null)
    {
        OnStartActivatorsPulse(color, customDuration, customIntensity);
    }

    private void StopActivatorsPulse()
    {
        OnStopActivatorsPulse();
    }

    // for child classes to implement
    protected abstract void OnSetActivatorsState(bool enabled);
    protected abstract void OnApplyLockedVisuals(Color color, string message);
    protected abstract void OnApplyBrokenVisuals(Color color);
    protected abstract void OnResetActivatorVisuals(Color color);
    protected abstract void OnStopActivatorsPulse();
    protected abstract void OnStartActivatorsPulse(Color color, float? customDuration = null, float? customIntensity = null);
}