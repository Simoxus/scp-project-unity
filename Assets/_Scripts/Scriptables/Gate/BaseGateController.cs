using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;

public abstract class BaseGateController : MonoBehaviour
{
    public enum GateState
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
    public Color32 movingColor = new Color32(255, 244, 153, 255); // light yellow
    public Color32 brokenColor = new Color32(255, 115, 88, 255); // salmon
    public Color32 grantedColor = new Color32(212, 255, 203, 255); // light green
    public Color32 deniedColor = new Color32(255, 153, 158, 255); // light red
    public Color32 lockedColor = new Color32(255, 153, 158, 255); // light red

    [Header("Environment Settings")]
    public bool breakableByEnvironment = true;

    [Header("FMOD Settings")]
    public string fmodParameterName = "State";

    [Header("Gate Visuals")]
    public GameObject gate;
    public GameObject gateLeft;
    public GameObject gateRight;
    public ParticleSystem sparksEmitter;

    [Header("Gate Offsets")]
    public Vector3 gateLeftOpenOffset = new Vector3(-4.3f, 0f, 0f);
    public Vector3 gateRightOpenOffset = new Vector3(4.3f, 0f, 0f);
    public Vector3 gateLeftCloseOffset;
    public Vector3 gateRightCloseOffset;

    [Header("Tweening & Physics")]
    public float gateMoveDuration = 1.5f;
    public float gateBreakForce = 27f;
    public float gateBreakDownwardForce = 15f;
    public float gateBreakTorque = 35f;
    public Ease easeStyle = Ease.InOutQuad;
    public Vector2 impulseVelocityRange = new Vector2(1.6f, 2f);

    [Header("State Values"), ReadOnly]
    public GateState currentState = GateState.Closed;

    private int _debrisLayer;
    private Vector3 _initialFrontLocalPosition;
    private Vector3 _initialBackLocalPosition;
    protected CancellationTokenSource _gateCts;

    protected virtual void Awake()
    {
        _debrisLayer = LayerMask.NameToLayer("Debris");

        if (gateLeft != null)
        {
            _initialFrontLocalPosition = gateLeft.transform.localPosition;
        }

        if (gateRight != null)
        {
            _initialBackLocalPosition = gateRight.transform.localPosition;
        }

        if (startOpened && currentState is not (GateState.Opened or GateState.Broken))
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
                currentState = GateState.Opened;
                gateLeft.transform.localPosition = _initialFrontLocalPosition + gateLeftOpenOffset;
                gateRight.transform.localPosition = _initialBackLocalPosition + gateRightOpenOffset;
            }
        }

        _gateCts = new CancellationTokenSource();
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
        if (_gateCts != null && !_gateCts.IsCancellationRequested)
        {
            _gateCts.Cancel();
        }
        _gateCts?.Dispose();
        _gateCts = null;
    }

    [ContextMenu("Toggle Gate")]
    public async UniTask ToggleGate()
    {
        if (_gateCts == null || _gateCts.IsCancellationRequested)
        {
            return;
        }

        if (currentState == GateState.Moving || currentState == GateState.Broken || gate == null)
        {
            return;
        }

        if (locked)
        {
            return;
        }

        bool shouldOpen = currentState == GateState.Closed;
        float fmodParameterValue = shouldOpen ? 1.0f : 0.0f;

        await SetActivatorsState(enabled: false);

        FMODHelper.PlayOneShotWithParametersAndOcclusion(
            AudioDataAccess.Instance.Doors.GateSound,
            gate.transform.position,
            minDuration: 1.5f,
            raysPerSound: 2,
            maxDistance: 70f,
            parameters: (fmodParameterName, fmodParameterValue)
        );

        SetGateState(GateState.Moving);
        StartActivatorsPulse(movingColor, 0.5f, 1.2f);

        try
        {
            if (shouldOpen)
            {
                await OpenGateAsync(_gateCts.Token);
            }
            else
            {
                await CloseGateAsync(_gateCts.Token);
            }

            await SetActivatorsState(true);
        }
        catch (OperationCanceledException)
        {
            Log.VerboseInfo("ToggleGate operation was cancelled.");
            SetGateState(shouldOpen ? GateState.Closed : GateState.Opened);
            await SetActivatorsState(true);
        }
        catch (Exception ex)
        {
            Log.VerboseWarning($"ToggleGate operation failed: {ex}");
            SetGateState(shouldOpen ? GateState.Closed : GateState.Opened);
            await SetActivatorsState(true);
        }
    }

    private async UniTask OpenGateAsync(CancellationToken token)
    {
        if (gateLeft == null || gateRight == null) return;

        Vector3 targetPosFront = _initialFrontLocalPosition + gateLeftOpenOffset;
        Vector3 targetPosBack = _initialBackLocalPosition + gateRightOpenOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(gateLeft.transform, targetPosFront, gateMoveDuration, easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(gateRight.transform, targetPosBack, gateMoveDuration, easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(token);

        if (this == null || CameraManager.Instance == null)
            return;

        CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.04f, 0.06f));

        SetGateState(GateState.Opened);
        StopActivatorsPulse();
        ResetActivatorColors();
    }

    private async UniTask CloseGateAsync(CancellationToken token)
    {
        if (gateLeft == null || gateRight == null) return;

        Vector3 targetPosFront = _initialFrontLocalPosition + gateRightCloseOffset;
        Vector3 targetPosBack = _initialBackLocalPosition + gateRightCloseOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(gateLeft.transform, targetPosFront, gateMoveDuration, easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(gateRight.transform, targetPosBack, gateMoveDuration, easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(token);

        if (this == null || CameraManager.Instance == null)
            return;

        CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.04f, 0.06f));

        SetGateState(GateState.Closed);
        StopActivatorsPulse();
        ResetActivatorColors();
    }

    [ContextMenu("Break Gate")]
    public async UniTask BreakGate()
    {
        if (gateLeft == null || gateRight == null) return;
        if (currentState == GateState.Broken) return;

        // Cancel any ongoing gate operations
        _gateCts.Cancel();
        _gateCts = new CancellationTokenSource();

        // Force close the gate first if it's not already closed
        gateLeft.transform.localPosition = _initialFrontLocalPosition + gateLeftCloseOffset;
        gateRight.transform.localPosition = _initialBackLocalPosition + gateRightCloseOffset;

        SetGateState(GateState.Broken);
        ApplyBrokenState();

        if (gate != null)
        {
            FMODHelper.PlayOneShotWithDynamicOcclusion(AudioDataAccess.Instance.Doors.GateBreakSound, gate.transform.position, 1.5f);
            FMODHelper.PlayOneShot3D(AudioDataAccess.Instance.Doors.GateBrokenSound, gate.transform.position);

            if (CameraManager.Instance != null)
            {
                CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(impulseVelocityRange.x, impulseVelocityRange.y));
            }

            sparksEmitter?.Play();

            Rigidbody frontRigidbody = gateLeft.GetComponent<Rigidbody>();
            Rigidbody backRigidbody = gateRight.GetComponent<Rigidbody>();

            Collider frontCollider = gateLeft.GetComponent<Collider>();
            Collider backCollider = gateRight.GetComponent<Collider>();

            if (frontCollider != null && backCollider != null)
            {
                Physics.IgnoreCollision(frontCollider, backCollider, true);
            }

            await UniTask.WaitForSeconds(0.4f, ignoreTimeScale: false);

            if (frontRigidbody != null)
            {
                gateLeft.layer = _debrisLayer;
                frontRigidbody.isKinematic = false;
                frontRigidbody.AddForce(gateLeft.transform.forward * gateBreakForce + Vector3.down * gateBreakDownwardForce, ForceMode.Impulse);
                frontRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * gateBreakTorque, ForceMode.Impulse);
            }

            if (backRigidbody != null)
            {
                gateRight.layer = _debrisLayer;
                backRigidbody.isKinematic = false;
                backRigidbody.AddForce(gateRight.transform.forward * gateBreakForce + Vector3.down * gateBreakDownwardForce, ForceMode.Impulse);
                backRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * gateBreakTorque, ForceMode.Impulse);
            }
        }
    }

    private void SetGateState(GateState newState)
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