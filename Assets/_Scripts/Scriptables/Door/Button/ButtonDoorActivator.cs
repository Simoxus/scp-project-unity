using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;
public class ButtonDoorActivator : BaseDoorActivator
{
    [Header("Script References")]
    public ButtonDoorVisual buttonVisual;
    public ButtonDoorController targetDoorController;
    [Header("FMOD Events")]
    public EventReference buttonSoundEvent;
    public EventReference buttonFailSoundEvent;
    public override void Interact()
    {
        if (targetDoorController == null) return;
        buttonVisual.PlayTween().Forget();
        if (targetDoorController.locked)
        {
            FMODHelper.PlayOneShot3D(buttonFailSoundEvent, transform.position);
            return;
        }
        if (targetDoorController.currentState != ButtonDoorController.DoorState.Broken)
        {
            FMODHelper.PlayOneShot3D(buttonSoundEvent, transform.position);
            targetDoorController.ToggleDoor().Forget();
        }
        else
        {
            FMODHelper.PlayOneShot3D(buttonFailSoundEvent, transform.position);
        }
    }
    public override void StartPulseEffect(Color startColor, float? customDuration = null, float? customIntensity = null)
    {
        if (buttonVisual != null)
        {
            buttonVisual.StartPulse(startColor, customDuration, customIntensity);
        }
    }
    public override void StopPulseEffect()
    {
        if (buttonVisual != null)
        {
            buttonVisual.StopPulse();
        }
    }
    public void TransitionToPulseEffect(Color targetColor, float transitionDuration, float pulseDuration, float pulseIntensity)
    {
        if (buttonVisual != null)
        {
            buttonVisual.TransitionToPulse(targetColor, transitionDuration, pulseDuration, pulseIntensity);
        }
    }
}