using Cysharp.Threading.Tasks;
using FMODUnity;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;

public class ButtonDoorController : MonoBehaviour
{
    [Header("References")]
    public PlayerAccess player;

    [Header("FMOD Settings")]
    public EventReference doorSoundEvent;
    public EventReference doorBreakSoundEvent;
    public string fmodParameterName = "State";

    [Header("Door Visuals")]
    public GameObject door;
    public GameObject doorFront;
    public GameObject doorBack;
    public GameObject doorFrontButton;
    public GameObject doorBackButton;
    public GameObject doorActivator1;
    public GameObject doorActivator2;

    [Header("Door Offsets")]
    public Vector3 doorFrontOpenOffset;
    public Vector3 doorBackOpenOffset;
    public Vector3 doorFrontCloseOffset;
    public Vector3 doorBackCloseOffset;
    public Vector3 doorFrontBrokenOffset;
    public Vector3 doorBackBrokenOffset;

    [Header("State Values")]
    public bool isOpen = false;
    public bool isMoving = false;
    public bool isBroken = false;

    private int _debrisLayer;
    private Vector3 _initialFrontLocalPosition;
    private Vector3 _initialBackLocalPosition;

    private Ease _easeStyle = Ease.InOutQuad;

    private CancellationTokenSource _cts;
    private CancellationToken _token;

    private void Awake()
    {
        // Get layer ID
        _debrisLayer = LayerMask.NameToLayer("Debris");

        _initialFrontLocalPosition = doorFront.transform.localPosition;
        _initialBackLocalPosition = doorBack.transform.localPosition;

        doorFront.transform.localPosition = _initialFrontLocalPosition + doorFrontCloseOffset;
        doorBack.transform.localPosition = _initialBackLocalPosition - doorBackCloseOffset;

        // Initalize cancellation tokens for the tweens
        _cts = new CancellationTokenSource();
        _token = _cts.Token;

        if (isOpen) { _ = ToggleDoor(); }
    }

    [ContextMenu("Toggle Door")]
    public async UniTask ToggleDoor()
    {
        if (isMoving) { return; }
        isOpen = !isOpen;

        // Set the FMOD parameter
        float fmodParameterValue = isOpen ? 1.0f : 0.0f;

        FMODHelper.PlayOneShotWithParameters(
            doorSoundEvent, // Use the path from EventReference
            door.transform.position,
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
        Vector3 targetPosFront = _initialFrontLocalPosition + doorFrontOpenOffset;
        Vector3 targetPosBack = _initialBackLocalPosition + doorBackOpenOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(doorFront.transform, targetPosFront, 1.4f, _easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(doorBack.transform, targetPosBack, 1.42f, _easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(_token);
        await UniTask.WaitForSeconds(0.15f, ignoreTimeScale: false);

        doorFront.transform.localPosition = targetPosFront;
        doorBack.transform.localPosition = targetPosBack;
        isMoving = false;
    }

    private async UniTask CloseDoorAsync()
    {
        isMoving = true;
        Vector3 targetPosFront = _initialFrontLocalPosition + doorFrontCloseOffset;
        Vector3 targetPosBack = _initialBackLocalPosition - doorBackCloseOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(doorFront.transform, targetPosFront, 1.5f, _easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(doorBack.transform, targetPosBack, 1.54f, _easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(_token);
        await UniTask.WaitForSeconds(0.15f, ignoreTimeScale: false);

        doorFront.transform.localPosition = targetPosFront;
        doorBack.transform.localPosition = targetPosBack;
        isMoving = false;
    }

    [ContextMenu("Break Door")]
    public async UniTask BreakDoor()
    {
        // Break door control buttons (deactivate their door actino functions)
        doorActivator1.GetComponent<ButtonDoorActivator>()?.BreakButton();
        doorActivator2.GetComponent<ButtonDoorActivator>()?.BreakButton();

        isBroken = true;
        isOpen = true;

        // Change the door scales so they don't get stuck (due to the torque force) :)
        Vector3 targetDoorScale = new Vector3(0.02f, 0.02f, 0f);
        doorFront.transform.localScale -= targetDoorScale;
        doorBack.transform.localScale -= targetDoorScale;

        _cts.Cancel(); // Cancels any ongoing door tweens
        _cts = new CancellationTokenSource(); // Reset the cancellation token for Unitask
        _token = _cts.Token;

        Vector3 targetPosFrontClosed = _initialFrontLocalPosition + doorFrontCloseOffset;
        Vector3 targetPosBackClosed = _initialBackLocalPosition - doorBackCloseOffset;
        doorFront.transform.localPosition = targetPosFrontClosed;
        doorBack.transform.localPosition = targetPosBackClosed;

        doorFront.layer = _debrisLayer;
        doorBack.layer = _debrisLayer;

        FMODHelper.PlayOneShot3D(doorBreakSoundEvent, door.transform.position);

        Rigidbody frontRigidbody = doorFront.GetComponent<Rigidbody>();
        Rigidbody backRigidbody = doorBack.GetComponent<Rigidbody>();

        if (frontRigidbody != null) frontRigidbody.isKinematic = false;
        if (backRigidbody != null) backRigidbody.isKinematic = false;

        float downwardForce = 15f;
        float forwardForce = 250f;
        float torqueForce = 35f;

        Vector3 doorForward = door.transform.forward;
        Vector3 downDirection = Vector3.down;
        Vector3 initialPushDirection = (doorForward * forwardForce) + (downDirection * downwardForce);

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
        if (doorActivator1 != null)
        {
            ButtonDoorActivator activator1Component = doorActivator1.GetComponent<ButtonDoorActivator>();
            if (activator1Component != null && activator1Component.activatorCollider != null)
            {
                activator1Component.activatorCollider.enabled = enabled;
            }
        }

        if (doorActivator2 != null)
        {
            ButtonDoorActivator activator2Component = doorActivator2.GetComponent<ButtonDoorActivator>();
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