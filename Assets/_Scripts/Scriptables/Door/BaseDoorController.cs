using Cysharp.Threading.Tasks;
using EditorAttributes;
using FMODUnity;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseDoorController : MonoBehaviour
{
    public enum DoorState
    {
        Moving,
        Opened,
        Closed,
        Broken
    }

    [Space]
    public GameObject door;
    public GameObject doorFront;
    public GameObject doorBack;
    public BoxCollider doorTrigger;
    public ParticleSystem sparksEmitter;
    public NavMeshObstacle navMeshObstacle;

    [Header("Initial State")]
    public bool locked = false;
    [SerializeField, ShowField(nameof(locked))]
    public string lockedMessage = string.Empty;
    public bool startOpened = false;
    [SerializeField, ShowField(nameof(startOpened))]
    public bool chanceToStartOpened = false;
    [SerializeField, ShowField(nameof(chanceToStartOpened)), Range(0, 1)]
    public float percentChanceToStartOpened = 0.5f;
    public bool breakable = true;

    [Header("Color States")]
    public Color32 defaultColor = new Color32(255, 255, 255, 255);
    public Color32 successColor = new Color32(212, 255, 203, 255);
    public Color32 failureColor = new Color32(255, 153, 158, 255);
    public Color32 movingColor = new Color32(255, 244, 153, 255);
    public Color32 brokenColor = new Color32(255, 115, 88, 255);
    public Color32 lockedColor = new Color32(255, 153, 158, 255);

    public virtual Color32 DefaultStateColor => defaultColor;
    public virtual Color32 SuccessStateColor => successColor;
    public virtual Color32 FailureStateColor => failureColor;
    public virtual Color32 MovingStateColor => movingColor;
    public virtual Color32 BrokenStateColor => brokenColor;
    public virtual Color32 LockedStateColor => lockedColor;

    [Header("FMOD Settings")]
    public bool useGateSounds = false;
    public string fmodParameterName = "State";

    [Header("Door Sliding")]
    public Vector3 doorFrontOpenOffset = new Vector3(-4.3f, 0f, 0f);
    public Vector3 doorBackOpenOffset = new Vector3(4.3f, 0f, 0f);
    public Vector3 doorFrontCloseOffset;
    public Vector3 doorBackCloseOffset;

    [Header("Door Rotating")]
    public Vector3 rotationAxis = Vector3.up;
    public float openRotationAngle = 90f;
    public float closeRotationAngle = 0f;

    [Header("Tweening & Physics")]
    public Ease easeStyle = Ease.InOutSine;
    public Vector2 breakImpulseRange = new Vector2(1.3f, 1.7f);
    public float doorMoveDuration = 1.5f;
    public float doorBreakForce = 27f;
    public float doorBreakDownwardForce = 15f;
    public float doorBreakTorque = 35f;

    [Header("State Values"), ReadOnly]
    public DoorState currentState = DoorState.Closed;

    private int _debrisLayer;
    private Vector3 _initialFrontLocalPosition;
    private Vector3 _initialBackLocalPosition;
    private Quaternion _initialDoorLocalRotation;
    protected CancellationTokenSource _doorCts;

    private bool IsRotatingDoor => doorFront == null && doorBack == null && door != null;

    protected virtual void Awake()
    {
        _debrisLayer = LayerMask.NameToLayer("Debris");

        if (navMeshObstacle != null)
        {
            navMeshObstacle.carving = true;
            navMeshObstacle.carveOnlyStationary = false;
            UpdateNavMeshObstacle();
        }

        if (IsRotatingDoor)
        {
            _initialDoorLocalRotation = door.transform.localRotation;

            if (startOpened && currentState is not (DoorState.Opened or DoorState.Broken))
            {
                currentState = DoorState.Opened;
                door.transform.localRotation = _initialDoorLocalRotation * Quaternion.AngleAxis(openRotationAngle, rotationAxis);
            }
        }
        else
        {
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
        }

        _doorCts = new CancellationTokenSource();
    }

    protected virtual void Start()
    {
        if (!IsRotatingDoor)
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

    [ContextMenu("Open Door")]
    public void OpenDoor()
    {
        if (locked) return;
        if (_doorCts == null || _doorCts.IsCancellationRequested) return;
        if (currentState == DoorState.Moving || currentState == DoorState.Broken || door == null) return;
        if (currentState == DoorState.Opened) return;

        if (IsRotatingDoor)
        {
            FMODHelper.PlayOneShot3D(
                Core.AudioDataAccess.Doors.DoorOfficeSound,
                door.transform.position,
                parameters: new[] { (fmodParameterName, 1.0f) },
                useOcclusion: true
            );

            SetDoorState(DoorState.Moving);
            OpenDoorAsync(_doorCts.Token).Forget();
        }
        else
        {
            FMODHelper.PlayOneShot3D(
                GetSoundEvent(Core.AudioDataAccess.Doors.DoorSound, Core.AudioDataAccess.Doors.GateSound),
                door.transform.position,
                parameters: new[] { (fmodParameterName, 1.0f) },
                useOcclusion: true
            );

            SetDoorState(DoorState.Moving);
            StartActivatorsPulse(MovingStateColor, 0.5f, 1.04f);

            OpenDoorWithVisualsAsync(_doorCts.Token).Forget();
        }
    }

    [ContextMenu("Close Door")]
    public void CloseDoor()
    {
        if (locked) return;
        if (_doorCts == null || _doorCts.IsCancellationRequested) return;
        if (currentState == DoorState.Moving || currentState == DoorState.Broken || door == null) return;
        if (currentState == DoorState.Closed) return;

        if (IsRotatingDoor)
        {
            FMODHelper.PlayOneShot3D(
                Core.AudioDataAccess.Doors.DoorOfficeSound,
                door.transform.position,
                parameters: new[] { (fmodParameterName, 0.0f) },
                useOcclusion: true
            );

            SetDoorState(DoorState.Moving);
            CloseDoorAsync(_doorCts.Token).Forget();
        }
        else
        {
            FMODHelper.PlayOneShot3D(
                GetSoundEvent(Core.AudioDataAccess.Doors.DoorSound, Core.AudioDataAccess.Doors.GateSound),
                door.transform.position,
                parameters: new[] { (fmodParameterName, 0.0f) },
                useOcclusion: true
            );

            SetDoorState(DoorState.Moving);
            StartActivatorsPulse(MovingStateColor, 0.5f, 1.04f);

            CloseDoorWithVisualsAsync(_doorCts.Token).Forget();
        }
    }

    public void OpenDoorImmediate()
    {
        if (locked) return;
        if (door == null) return;
        if (currentState == DoorState.Broken) return;

        if (IsRotatingDoor)
        {
            door.transform.localRotation = _initialDoorLocalRotation * Quaternion.AngleAxis(openRotationAngle, rotationAxis);
            SetDoorState(DoorState.Opened);
        }
        else
        {
            if (doorFront == null || doorBack == null) return;

            doorFront.transform.localPosition = _initialFrontLocalPosition + doorFrontOpenOffset;
            doorBack.transform.localPosition = _initialBackLocalPosition + doorBackOpenOffset;

            SetDoorState(DoorState.Opened);
            ApplyOpenableState();
        }
    }

    public void CloseDoorImmediate()
    {
        if (locked) return;
        if (door == null) return;
        if (currentState == DoorState.Broken) return;

        if (IsRotatingDoor)
        {
            door.transform.localRotation = _initialDoorLocalRotation * Quaternion.AngleAxis(closeRotationAngle, rotationAxis);
            SetDoorState(DoorState.Closed);
        }
        else
        {
            if (doorFront == null || doorBack == null) return;

            doorFront.transform.localPosition = _initialFrontLocalPosition + doorFrontCloseOffset;
            doorBack.transform.localPosition = _initialBackLocalPosition + doorBackCloseOffset;

            SetDoorState(DoorState.Closed);
            ApplyOpenableState();
        }
    }

    private async UniTask OpenDoorWithVisualsAsync(CancellationToken token)
    {
        try
        {
            await SetActivatorsState(enabled: false);
            await OpenDoorAsync(token);
            await SetActivatorsState(enabled: true);
        }
        catch (OperationCanceledException)
        {
            SetDoorState(DoorState.Closed);
            await SetActivatorsState(enabled: true);
        }
    }

    private async UniTask CloseDoorWithVisualsAsync(CancellationToken token)
    {
        try
        {
            await SetActivatorsState(enabled: false);
            await CloseDoorAsync(token);
            await SetActivatorsState(enabled: true);
        }
        catch (OperationCanceledException)
        {
            SetDoorState(DoorState.Opened);
            await SetActivatorsState(enabled: true);
        }
    }

    [ContextMenu("Toggle Door")]
    public async UniTask ToggleDoor()
    {
        if (_doorCts == null || _doorCts.IsCancellationRequested) return;
        if (currentState == DoorState.Moving || currentState == DoorState.Broken || door == null) return;
        if (locked) return;

        bool shouldOpen = currentState == DoorState.Closed;
        float fmodParameterValue = shouldOpen ? 1.0f : 0.0f;

        if (IsRotatingDoor)
        {
            FMODHelper.PlayOneShot3D(
                Core.AudioDataAccess.Doors.DoorOfficeSound,
                door.transform.position,
                parameters: new[] { (fmodParameterName, fmodParameterValue) }
            );

            SetDoorState(DoorState.Moving);

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
            }
            catch (OperationCanceledException)
            {
                Log.VerboseInfo("ToggleDoor operation was cancelled.");
                SetDoorState(shouldOpen ? DoorState.Closed : DoorState.Opened);
            }
            catch (Exception ex)
            {
                Log.VerboseWarning($"ToggleDoor operation failed: {ex}");
                SetDoorState(shouldOpen ? DoorState.Closed : DoorState.Opened);
            }
        }
        else
        {
            await SetActivatorsState(enabled: false);

            FMODHelper.PlayOneShot3D(
                GetSoundEvent(Core.AudioDataAccess.Doors.DoorSound, Core.AudioDataAccess.Doors.GateSound),
                door.transform.position,
                parameters: new[] { (fmodParameterName, fmodParameterValue) }
            );

            SetDoorState(DoorState.Moving);
            StartActivatorsPulse(MovingStateColor, 0.5f, 1.04f);

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
    }

    private async UniTask OpenDoorAsync(CancellationToken token)
    {
        if (IsRotatingDoor)
        {
            if (door == null) return;

            Quaternion targetRotation = _initialDoorLocalRotation * Quaternion.AngleAxis(openRotationAngle, rotationAxis);

            await Tween.LocalRotation(door.transform, targetRotation, doorMoveDuration, easeStyle)
                .ToYieldInstruction()
                .ToUniTask()
                .AttachExternalCancellation(token);

            if (this == null) return;

            SetDoorState(DoorState.Opened);
        }
        else
        {
            if (doorFront == null || doorBack == null) return;

            Vector3 targetPosFront = _initialFrontLocalPosition + doorFrontOpenOffset;
            Vector3 targetPosBack = _initialBackLocalPosition + doorBackOpenOffset;

            await UniTask.WhenAll(
                Tween.LocalPosition(doorFront.transform, targetPosFront, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask(),
                Tween.LocalPosition(doorBack.transform, targetPosBack, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask()
            ).AttachExternalCancellation(token);

            if (this == null) return;

            SetDoorState(DoorState.Opened);
            ApplyOpenableState();
        }
    }

    private async UniTask CloseDoorAsync(CancellationToken token)
    {
        if (IsRotatingDoor)
        {
            if (door == null) return;

            Quaternion targetRotation = _initialDoorLocalRotation * Quaternion.AngleAxis(closeRotationAngle, rotationAxis);

            await Tween.LocalRotation(door.transform, targetRotation, doorMoveDuration, easeStyle)
                .ToYieldInstruction()
                .ToUniTask()
                .AttachExternalCancellation(token);

            if (this == null) return;

            SetDoorState(DoorState.Closed);
        }
        else
        {
            if (doorFront == null || doorBack == null) return;

            Vector3 targetPosFront = _initialFrontLocalPosition + doorFrontCloseOffset;
            Vector3 targetPosBack = _initialBackLocalPosition + doorBackCloseOffset;

            await UniTask.WhenAll(
                Tween.LocalPosition(doorFront.transform, targetPosFront, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask(),
                Tween.LocalPosition(doorBack.transform, targetPosBack, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask()
            ).AttachExternalCancellation(token);

            if (this == null) return;

            SetDoorState(DoorState.Closed);
            ApplyOpenableState();
        }
    }

    [ContextMenu("Break Door")]
    public async UniTask BreakDoor()
    {
        if (IsRotatingDoor) return;
        if (doorFront == null || doorBack == null) return;
        if (currentState == DoorState.Broken) return;

        if (breakable)
        {
            _doorCts.Cancel();
            _doorCts = new CancellationTokenSource();

            doorFront.transform.localPosition = _initialFrontLocalPosition + doorFrontCloseOffset;
            doorBack.transform.localPosition = _initialBackLocalPosition + doorBackCloseOffset;

            SetDoorState(DoorState.Broken);
            ApplyBrokenState();

            if (door != null)
            {
                FMODHelper.PlayOneShot3D(
                    GetSoundEvent(Core.AudioDataAccess.Doors.DoorBreakSound, Core.AudioDataAccess.Doors.GateBreakSound),
                    door.transform.position,
                    useOcclusion: true,
                    occlusionMinDuration: 1.5f
                );
                FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.DoorBrokenSound, door.transform.position);

                if (Core.CameraManager != null)
                {
                    Core.CameraManager.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(breakImpulseRange.x, breakImpulseRange.y));
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

                await UniTask.WaitForSeconds(0.2f, ignoreTimeScale: false);

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
    }

    public void SetBrokenStateWithoutPhysics()
    {
        if (IsRotatingDoor) return;

        _doorCts?.Cancel();
        _doorCts = new CancellationTokenSource();

        SetDoorState(DoorState.Broken);
        ApplyBrokenState();
    }

    public void ApplyLockedStateFromPersistence()
    {
        ApplyLockedState();
    }

    private EventReference GetSoundEvent(EventReference doorSound, EventReference gateSound)
    {
        return useGateSounds ? gateSound : doorSound;
    }

    private void UpdateNavMeshObstacle()
    {
        if (navMeshObstacle == null) return;

        bool shouldCarve = currentState == DoorState.Closed || currentState == DoorState.Broken;
        navMeshObstacle.enabled = shouldCarve;
    }

    private void SetDoorState(DoorState newState)
    {
        currentState = newState;
        UpdateNavMeshObstacle();
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

    protected abstract void OnSetActivatorsState(bool enabled);
    protected abstract void OnApplyLockedVisuals(Color color, string message);
    protected abstract void OnApplyBrokenVisuals(Color color);
    protected abstract void OnResetActivatorVisuals(Color color);
    protected abstract void OnStopActivatorsPulse();
    protected abstract void OnStartActivatorsPulse(Color color, float? customDuration = null, float? customIntensity = null);
}