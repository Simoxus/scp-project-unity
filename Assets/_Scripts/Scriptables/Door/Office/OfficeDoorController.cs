using Cysharp.Threading.Tasks;
using FMODUnity;
using Newtonsoft.Json.Linq;
using PrimeTween;
using System;
using System.Threading;
using UnityEngine;

public class OfficeDoorController : MonoBehaviour
{
    [Header("FMOD Settings")]
    public EventReference doorSoundEvent;
    public string fmodParameterName = "State";

    [Header("Door Visuals")]
    public GameObject door;
    public GameObject doorHandle1;
    public GameObject doorHandle2;
    public GameObject doorActivator1;
    public GameObject doorActivator2;

    [Header("Handle Visuals")]
    public BoxCollider activatorCollider1;
    public BoxCollider activatorCollider2;

    [Header("Door Offsets")]
    public Quaternion openedRotation;
    public Quaternion closedRotation = Quaternion.identity; // Default to no rotation :)

    [Header("State Values")]
    public bool isOpen = false;
    public bool isMoving = false;

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

        await Tween.LocalRotation(door.transform, openedRotation, 1.1f, Ease.InOutCubic).ToYieldInstruction().ToUniTask();
        await UniTask.WaitForSeconds(0.14f, ignoreTimeScale: false);

        isMoving = false;
    }

    private async UniTask CloseDoorAsync()
    {
        isMoving = true;

        await Tween.LocalRotation(door.transform, closedRotation, 1.2f, Ease.InOutCubic).ToYieldInstruction().ToUniTask();
        await UniTask.WaitForSeconds(0.14f, ignoreTimeScale: false);

        isMoving = false;
    }

    private void SetActivatorsState(bool enabled)
    {
        activatorCollider1.enabled = enabled;
        activatorCollider2.enabled = enabled;
    }
}
