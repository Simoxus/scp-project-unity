using Cysharp.Threading.Tasks;
using EditorAttributes;
using FMODUnity;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;

public class KeycardDoorController : MonoBehaviour
{
    public enum DoorState
    {
        Moving,
        Opened,
        Closed,
        Broken
    }

    [Header("Initial State")]
    public bool startOpened = false;
    [SerializeField, ShowField(nameof(startOpened))]
    public bool chanceToStartOpened = false;
    [SerializeField, ShowField(nameof(chanceToStartOpened)), Range(0, 1)]
    public float percentChanceToStartOpened = 0.5f;

    [Header("Environment Settings")]
    public bool breakableByEnvironment = true;

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
    public KeycardDoorActivator doorActivator1;
    public KeycardDoorActivator doorActivator2;
    public ParticleSystem sparksEmitter;

    [Header("Door Offsets")]
    public Vector3 doorFrontOpenOffset = new Vector3(-4.3f, 0f, 0f);
    public Vector3 doorBackOpenOffset = new Vector3(4.3f, 0f, 0f);
    public Vector3 doorFrontCloseOffset;
    public Vector3 doorBackCloseOffset;
    public Vector3 doorFrontBrokenOffset;
    public Vector3 doorBackBrokenOffset;

    [Header("Tweening & Physics")]
    public float doorMoveDuration = 1.5f;
    public float doorBreakForce = 27f;
    public float doorBreakDownwardForce = 15f;
    public float doorBreakTorque = 35f;
    public Ease easeStyle = Ease.InOutQuad;
    public Vector2 impulseVelocityRange = new Vector2(1.6f, 2f);

    [Header("State Values"), EditorAttributes.ReadOnly]
    public DoorState currentState = DoorState.Closed;

    private int _debrisLayer;
    private Vector3 _initialFrontLocalPosition;
    private Vector3 _initialBackLocalPosition;

    private CancellationTokenSource _cts;

    private void Awake()
    {
        // Get layer ID
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
                // Generate random float between 0.0 and 1.0
                float randomValue = UnityEngine.Random.Range(0f, 1f);

                // Check if random value is greater than the percentage chance
                if (randomValue > percentChanceToStartOpened) { shouldOpen = false; }
            }

            if (shouldOpen)
            {
                currentState = DoorState.Opened;
                doorFront.transform.localPosition = _initialFrontLocalPosition + doorFrontOpenOffset;
                doorBack.transform.localPosition = _initialBackLocalPosition + doorBackOpenOffset;
            }
        }

        // Initalize cancellation token
        _cts = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    [ContextMenu("Toggle Door")]
    public async UniTask ToggleDoor()
    {
        if (currentState == DoorState.Moving || currentState == DoorState.Broken || door == null)
        {
            return;
        }

        bool shouldOpen = currentState == DoorState.Closed;
        float fmodParameterValue = shouldOpen ? 1.0f : 0.0f; // Set FMOD parameter based on action

        await SetActivatorsState(enabled: false);

        FMODHelper.PlayOneShotWithParameters(
            doorSoundEvent,
            door.transform.position,
            (fmodParameterName, fmodParameterValue)
        );

        SetDoorState(DoorState.Moving);
        await SetActivatorsState(false);

        try
        {
            if (shouldOpen)
            {
                await OpenDoorAsync(_cts.Token);
            }
            else
            {
                await CloseDoorAsync(_cts.Token);
            }

            await SetActivatorsState(true);
        }
        catch (OperationCanceledException)
        {
            Debug.Log($"ToggleDoor operation was cancelled.", this);
            SetDoorState(shouldOpen ? DoorState.Closed : DoorState.Opened);

            await SetActivatorsState(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"ToggleDoor operation failed: {ex}", this);
            SetDoorState(shouldOpen ? DoorState.Closed : DoorState.Opened);

            await SetActivatorsState(true);
        }
    }

    private async UniTask OpenDoorAsync(CancellationToken token)
    {
        if (doorFront == null || doorBack == null) { return; }

        Vector3 targetPosFront = _initialFrontLocalPosition + doorFrontOpenOffset;
        Vector3 targetPosBack = _initialBackLocalPosition + doorBackOpenOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(doorFront.transform, targetPosFront, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(doorBack.transform, targetPosBack, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(token);

        CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.06f, 0.04f));

        SetDoorState(DoorState.Opened);
    }

    private async UniTask CloseDoorAsync(CancellationToken token)
    {
        if (doorFront == null || doorBack == null) { return; }

        Vector3 targetPosFront = _initialFrontLocalPosition + doorFrontCloseOffset;
        Vector3 targetPosBack = _initialBackLocalPosition + doorBackCloseOffset;

        await UniTask.WhenAll(
            Tween.LocalPosition(doorFront.transform, targetPosFront, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask(),
            Tween.LocalPosition(doorBack.transform, targetPosBack, doorMoveDuration, easeStyle).ToYieldInstruction().ToUniTask()
        ).AttachExternalCancellation(token);

        CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(0.04f, 0.06f));

        SetDoorState(DoorState.Closed);
    }

    [ContextMenu("Break Door")]
    public async UniTask BreakDoor()
    {
        if (doorFront == null || doorBack == null) { return; }
        if (currentState == DoorState.Broken) { return; }

        _cts.Cancel(); // Cancels any ongoing door tweens
        SetDoorState(DoorState.Broken);

        // Deactivate buttons
        if (doorActivator1 != null) doorActivator1?.BreakButton();
        if (doorActivator2 != null) doorActivator2?.BreakButton();

        // Play sound and generate camera shake
        if (door != null)
        {
            FMODHelper.PlayOneShot3D(doorBreakSoundEvent, door.transform.position);
            CameraManager.Instance.GenerateShakeWithVector3(UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(impulseVelocityRange.x, impulseVelocityRange.y));

            sparksEmitter?.Play();

            // Get Rigidbody components
            Rigidbody frontRigidbody = doorFront.GetComponent<Rigidbody>();
            Rigidbody backRigidbody = doorBack.GetComponent<Rigidbody>();

            await UniTask.WaitForSeconds(0.4f, ignoreTimeScale: false);

            // Enable physics and apply force/torque
            if (frontRigidbody != null)
            {
                doorFront.layer = _debrisLayer;
                frontRigidbody.isKinematic = false;
                frontRigidbody.AddForce(door.transform.forward * doorBreakForce + Vector3.down * doorBreakDownwardForce, ForceMode.Impulse);
                frontRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * doorBreakTorque, ForceMode.Impulse);
            }

            if (backRigidbody != null)
            {
                doorBack.layer = _debrisLayer;
                backRigidbody.isKinematic = false;
                backRigidbody.AddForce(door.transform.forward * doorBreakForce + Vector3.down * doorBreakDownwardForce, ForceMode.Impulse);
                backRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * doorBreakTorque, ForceMode.Impulse);
            }
        }
    }

    public void UpdateActivatorVisuals(bool success, string clearanceLevel)
    {
        if (doorActivator1 != null)
        {
            if (success)
            {
                doorActivator1.DisplayGranted(clearanceLevel);
            }
            else
            {
                doorActivator1.DisplayDenied(clearanceLevel);
            }
        }

        if (doorActivator2 != null)
        {
            if (success)
            {
                doorActivator2.DisplayGranted(clearanceLevel);
            }
            else
            {
                doorActivator2.DisplayDenied(clearanceLevel);
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

        if (doorActivator1 != null)
        {
            doorActivator1.SetButtonState(enabled);
        }

        if (doorActivator2 != null)
        {
            doorActivator2.SetButtonState(enabled);
        }
    }
}