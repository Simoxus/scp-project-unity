using Cysharp.Threading.Tasks;
using EditorAttributes;
using FMODUnity;
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
    public Color defaultColor = new Color(1f, 1f, 1f); // 255, 255, 255 (white)
    public Color movingColor = new Color(1f, 0.953f, 0.459f); // 255, 243, 117 (yellow)
    public Color brokenColor = new Color(1f, 0.451f, 0.345f); // 255, 115, 88 (orange)
    public Color grantedColor = new Color(0.392f, 1f, 0.392f); // 100, 255, 100 (green)
    public Color deniedColor = new Color(1f, 0.278f, 0.278f); // 255, 71, 71 (red)
    public Color lockedColor = new Color(0.078f, 0.078f, 0.078f); // 32, 32, 32 (dark gray)

    [Header("Environment Settings")]
    public bool breakableByEnvironment = true;

    [Header("FMOD Settings")]
    public EventReference doorToggleSound;
    public EventReference doorBreakSound;
    public EventReference doorBrokenSound;
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
            ResetActivatorColors();
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
            doorToggleSound,
            door.transform.position,
            parameters: (fmodParameterName, fmodParameterValue)
        );

        SetDoorState(DoorState.Moving);
        StartActivatorsPulse(movingColor, 0.5f, 1.2f);

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
        ResetActivatorColors();
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
        ResetActivatorColors();
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
            FMODHelper.PlayOneShotWithDynamicOcclusion(doorBreakSound, door.transform.position, 1.5f);
            FMODHelper.PlayOneShot3D(doorBrokenSound, door.transform.position);

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

    private void ApplyLockedState()
    {
        StopActivatorsPulse();
        OnApplyLockedVisuals(lockedColor, lockedMessage);
        StartActivatorsPulse(lockedColor);
    }

    private void ApplyBrokenState()
    {
        StopActivatorsPulse();
        OnApplyBrokenVisuals(brokenColor);
        StartActivatorsPulse(brokenColor);
    }

    private void ResetActivatorColors()
    {
        OnResetActivatorVisuals(defaultColor);
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
    protected abstract void OnTransitionToPulse(Color targetColor, float transitionDuration);
}