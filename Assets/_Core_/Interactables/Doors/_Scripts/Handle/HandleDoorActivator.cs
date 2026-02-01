using Cysharp.Threading.Tasks;
using UnityEngine;

public class HandleDoorActivator : BaseDoorActivator
{
    public override BaseDoorController DoorController => targetDoorController;

    [Space]
    public HandleDoorVisual HandleVisual;
    [SerializeField] private HandleDoorController targetDoorController;

    public override void Interact()
    {
        if (targetDoorController == null) return;

        HandleVisual.PlayTween().Forget();

        if (targetDoorController.currentState == BaseDoorController.DoorState.Broken || targetDoorController.locked)
        {
            FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.DoorOfficeLeverSound, transform.position);
            return;
        }


        if (targetDoorController.currentState != BaseDoorController.DoorState.Broken)
        {
            targetDoorController.ToggleDoor().Forget();
        }
    }

    public override void StartPulseEffect(Color startColor, float? customDuration = null, float? customIntensity = null)
    {
    }

    public override void StopPulseEffect()
    {
    }
}