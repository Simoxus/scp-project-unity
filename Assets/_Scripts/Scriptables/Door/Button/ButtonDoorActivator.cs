using Cysharp.Threading.Tasks;
using UnityEngine;

public class ButtonDoorActivator : BaseDoorActivator
{
    public override BaseDoorController DoorController => targetDoorController;

    [Space]
    public ButtonDoorVisual ButtonVisual;
    [SerializeField] private ButtonDoorController targetDoorController;

    public override void Interact()
    {
        if (targetDoorController == null) return;

        ButtonVisual.PlayTween().Forget();

        if (targetDoorController.currentState == BaseDoorController.DoorState.Broken || targetDoorController.locked)
        {
            FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.ButtonErrorSound, transform.position);
            return;
        }

        FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.ButtonSound, transform.position);
        targetDoorController.ToggleDoor().Forget();
    }

    public override void StartPulseEffect(Color startColor, float? customDuration = null, float? customIntensity = null)
    {
        if (ButtonVisual != null)
        {
            ButtonVisual.StartPulse(startColor, customDuration, customIntensity);
        }
    }

    public override void StopPulseEffect()
    {
        if (ButtonVisual != null)
        {
            ButtonVisual.StopPulse();
        }
    }

    public void TransitionToPulseEffect(Color targetColor, float transitionDuration, float pulseDuration, float pulseIntensity)
    {
        if (ButtonVisual != null)
        {
            ButtonVisual.TransitionToPulse(targetColor, transitionDuration, pulseDuration, pulseIntensity);
        }
    }
}