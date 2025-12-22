using Cysharp.Threading.Tasks;
using FMODUnity;
using UnityEngine;

public class ButtonGateActivator : BaseGateActivator
{
    [Header("Script References")]
    public ButtonGateVisual buttonVisual;
    public ButtonGateController targetGateController;

    [Header("FMOD Events")]
    public EventReference buttonSoundEvent;
    public EventReference buttonFailSoundEvent;

    [Header("Color States")]
    public Color defaultColor = new Color(1f, 1f, 1f); // 255, 255, 255 (white)
    public Color movingColor = new Color(1f, 0.953f, 0.459f); // 255, 243, 117 (yellow)
    public Color brokenColor = new Color(1f, 0.451f, 0.345f); // 255, 115, 88 (orange)
    public Color lockedColor = new Color(0.078f, 0.078f, 0.078f); // 32, 32, 32 (dark gray)

    protected override void Start()
    {
        base.Start();

        if (targetGateController.locked)
        {
            StopPulseEffect();
            buttonVisual.ToggleLogo(false);
            buttonVisual.ToggleText(true);
            buttonVisual.ChangeScreenText(targetGateController.lockedMessage);
            buttonVisual.ChangeScreenColor(lockedColor, true);
            StartPulseEffect(lockedColor);
        }
    }

    public override void Interact()
    {
        if (targetGateController == null) return;

        buttonVisual.PlayTween().Forget();

        if (targetGateController.locked)
        {
            FMODHelper.PlayOneShot3D(buttonFailSoundEvent, transform.position);
            return;
        }

        if (targetGateController.currentState != ButtonGateController.GateState.Broken)
        {
            FMODHelper.PlayOneShot3D(buttonSoundEvent, transform.position);
            targetGateController.ToggleGate().Forget();
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

    public void ResetToDefaultColor()
    {
        if (buttonVisual != null)
        {
            StopPulseEffect();
            buttonVisual.ChangeScreenColor(defaultColor, true, 0.4f);
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