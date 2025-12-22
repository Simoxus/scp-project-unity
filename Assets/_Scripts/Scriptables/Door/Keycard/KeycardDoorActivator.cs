using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;
public class KeycardDoorActivator : BaseDoorActivator
{
    [Header("Script References")]
    public KeycardDoorVisual buttonVisual;
    public KeycardDoorController targetDoorController;
    [Header("FMOD Events")]
    public EventReference keycardSoundEvent;
    public EventReference keycardFailSoundEvent;
    public override void Interact()
    {
        if (targetDoorController == null) return;
        if (targetDoorController.currentState == KeycardDoorController.DoorState.Broken)
        {
            FMODHelper.PlayOneShot3D(keycardFailSoundEvent, transform.position);
            return;
        }
        if (targetDoorController.locked)
        {
            FMODHelper.PlayOneShot3D(keycardFailSoundEvent, transform.position);
            return;
        }
        SetButtonState(false);
        bool keycardCheckSuccessful = IsCorrectKeycardLevel(playerKeycardLevel: 1);
        FMODHelper.PlayOneShotWithParameters(
            keycardSoundEvent,
            transform.position,
            ("Result", keycardCheckSuccessful ? 0.0f : 1.0f)
        );
        if (keycardCheckSuccessful)
        {
            targetDoorController.ToggleDoor().Forget();
        }
        targetDoorController.UpdateActivatorVisuals(keycardCheckSuccessful, targetDoorController.requiredKeycardLevel.ToString());
    }
    public bool IsCorrectKeycardLevel(int playerKeycardLevel)
    {
        return playerKeycardLevel >= targetDoorController.requiredKeycardLevel;
    }
    public async UniTask ResetButtonDisplay()
    {
        await UniTask.WaitForSeconds(1.6f, ignoreTimeScale: false);
        buttonVisual.ToggleLogo(true);
        buttonVisual.ToggleText(false);
        buttonVisual.ChangeScreenColor(targetDoorController.defaultColor, true, 0.8f);
        buttonVisual.ChangeScreenText("HI");
        await UniTask.WaitForSeconds(0.15f, ignoreTimeScale: false);
        SetButtonState(true);
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