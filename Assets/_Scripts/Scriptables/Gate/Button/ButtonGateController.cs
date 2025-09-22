using Cysharp.Threading.Tasks;
using FMODUnity;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;

public class ButtonGateController : MonoBehaviour
{
    [Header("References")]
    public Player player;

    [Header("FMOD Settings")]
    public EventReference gateSoundEvent;
    public EventReference gateBreakSoundEvent;
    public string fmodParameterName = "State";

    [Header("Gate Visuals")]
    public GameObject gate;
    public GameObject gateLeft;
    public GameObject gateRight;
    public GameObject gateFrontButton;
    public GameObject gateBackButton;
    public GameObject gateActivator1;
    public GameObject gateActivator2;

    [Header("Gate Offsets")]
    public Vector3 gateLeftOpenOffset;
    public Vector3 gateRightOpenOffset;
    public Vector3 gateLeftCloseOffset;
    public Vector3 gateRightCloseOffset;
    public Vector3 gateLeftBrokenOffset;
    public Vector3 doorRightBrokenOffset;

    [Header("State Values")]
    public bool isOpen = false;
    public bool isMoving = false;
    public bool isBroken = false;

    private int _debrisLayer;
    private Vector3 _initialLeftLocalPosition;
    private Vector3 _initialRightLocalPosition;

    private Ease _easeStyle = Ease.InOutQuad;

    private CancellationTokenSource _cts;
    private CancellationToken _token;

    private void Awake()
    {
        // Get layer ID
        _debrisLayer = LayerMask.NameToLayer("Debris");

        _initialLeftLocalPosition = gateLeft.transform.localPosition;
        _initialRightLocalPosition = gateRight.transform.localPosition;

        gateLeft.transform.localPosition = _initialLeftLocalPosition + gateLeftCloseOffset;
        gateRight.transform.localPosition = _initialRightLocalPosition - gateRightCloseOffset;

        // Initalize cancellation tokens for the tweens
        _cts = new CancellationTokenSource();
        _token = _cts.Token;

        if (isOpen) { _ = ToggleDoor(); }
    }

    private void Start()
    {
        player = player != null ? player : Player.Instance;
    }

    [ContextMenu("Toggle Gate")]
    public async UniTask ToggleDoor()
    {
        if (isMoving) { return; }
        isOpen = !isOpen;

        // Set the FMOD parameter
        float fmodParameterValue = isOpen ? 1.0f : 0.0f;

        FMODHelper.PlayOneShotWithParameters(
            gateSoundEvent, // Use the path from EventReference
            gate.transform.position,
            (fmodParameterName, fmodParameterValue)
        );

        try
        {
            if (isOpen) { await OpenDoorAsync(); }
            else { await CloseDoorAsync(); }

            SetActivatorsState(true);
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"Door operation was cancelled.", this);
            isMoving = false;
        }
    }

    private async UniTask OpenDoorAsync()
    {
        isMoving = true;
        Vector3 targetPosLeft = _initialLeftLocalPosition + gateLeftOpenOffset;
        Vector3 targetPosRight = _initialRightLocalPosition + gateRightOpenOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(gateLeft.transform, targetPosLeft, 3.3f, _easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(gateRight.transform, targetPosRight, 3.2f, _easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(_token);
        await UniTask.WaitForSeconds(0.15f, ignoreTimeScale: false);

        gateLeft.transform.localPosition = targetPosLeft;
        gateRight.transform.localPosition = targetPosRight;
        isMoving = false;
    }

    private async UniTask CloseDoorAsync()
    {
        isMoving = true;
        Vector3 targetPosLeft = _initialLeftLocalPosition + gateLeftCloseOffset;
        Vector3 targetPosRight = _initialRightLocalPosition + gateLeftCloseOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(gateLeft.transform, targetPosLeft, 3.1f, _easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(gateRight.transform, targetPosRight, 3f, _easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(_token);
        await UniTask.WaitForSeconds(0.15f, ignoreTimeScale: false);

        gateLeft.transform.localPosition = targetPosLeft;
        gateRight.transform.localPosition = targetPosRight;
        isMoving = false;
    }

    [ContextMenu("Break Gate")]
    public async UniTask BreakDoor()
    {
        // Break gate control buttons (deactivate their gate action functions)
        gateActivator1.GetComponent<ButtonGateActivator>()?.BreakButton();
        gateActivator2.GetComponent<ButtonGateActivator>()?.BreakButton();

        isBroken = true;
        isOpen = true;

        // Change the gate door scales so they don't get stuck (due to the torque force) :)
        Vector3 targetGateScale = new Vector3(0.02f, 0.02f, 0f);
        gateLeft.transform.localScale -= targetGateScale;
        gateRight.transform.localScale -= targetGateScale;

        _cts.Cancel(); // Cancels any ongoing gate door tweens
        _cts = new CancellationTokenSource(); // Reset the cancellation token for Unitask
        _token = _cts.Token;

        Vector3 targetPosFrontClosed = _initialLeftLocalPosition + gateLeftCloseOffset;
        Vector3 targetPosBackClosed = _initialRightLocalPosition - gateRightCloseOffset;
        gateLeft.transform.localPosition = targetPosFrontClosed;
        gateRight.transform.localPosition = targetPosBackClosed;

        gateLeft.layer = _debrisLayer;
        gateRight.layer = _debrisLayer;

        FMODHelper.PlayOneShot3D(gateBreakSoundEvent, gate.transform.position);

        Rigidbody frontRigidbody = gateLeft.GetComponent<Rigidbody>();
        Rigidbody backRigidbody = gateRight.GetComponent<Rigidbody>();

        if (frontRigidbody != null) frontRigidbody.isKinematic = false;
        if (backRigidbody != null) backRigidbody.isKinematic = false;

        float downwardForce = 15f;
        float forwardForce = 250f;
        float torqueForce = 35f;

        Vector3 gateForward = gate.transform.forward;
        Vector3 downDirection = Vector3.down;
        Vector3 initialPushDirection = (gateForward * forwardForce) + (downDirection * downwardForce);

        Vector3 randomTorqueFront = UnityEngine.Random.insideUnitSphere * torqueForce;
        Vector3 randomTorqueBack = UnityEngine.Random.insideUnitSphere * torqueForce;

        frontRigidbody.AddForce(initialPushDirection, ForceMode.Impulse);
        backRigidbody.AddForce(initialPushDirection, ForceMode.Impulse);

        float insideRange = 1f;
        float outsideRange = 2f;
        Vector3 randomVelocity = UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(insideRange, outsideRange);
        player?.cameraImpulseSource?.GenerateImpulseWithVelocity(randomVelocity);

        await UniTask.WaitForSeconds(0.7f, ignoreTimeScale: false);
        frontRigidbody.AddTorque(randomTorqueFront, ForceMode.Impulse);
        backRigidbody.AddTorque(randomTorqueBack, ForceMode.Impulse);
    }

    private void SetActivatorsState(bool enabled)
    {
        if (gateActivator1 != null)
        {
            ButtonGateActivator activator1Component = gateActivator1.GetComponent<ButtonGateActivator>();
            if (activator1Component != null && activator1Component.activatorCollider != null)
            {
                activator1Component.activatorCollider.enabled = enabled;
            }
        }

        if (gateActivator2 != null)
        {
            ButtonGateActivator activator2Component = gateActivator2.GetComponent<ButtonGateActivator>();
            if (activator2Component != null && activator2Component.activatorCollider != null)
            {
                activator2Component.activatorCollider.enabled = enabled;
            }
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
