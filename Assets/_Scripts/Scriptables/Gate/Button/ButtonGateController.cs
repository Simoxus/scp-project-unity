using UnityEngine;

public class ButtonGateController : BaseGateController
{
    public ButtonGateActivator gateActivator1;
    public ButtonGateActivator gateActivator2;

    protected override void OnSetActivatorsState(bool enabled)
    {
        if (gateActivator1 != null)
        {
            gateActivator1.SetButtonState(enabled);
        }
        if (gateActivator2 != null)
        {
            gateActivator2.SetButtonState(enabled);
        }
    }

    protected override void OnApplyLockedVisuals(Color color, string message)
    {
        if (gateActivator1 != null)
        {
            gateActivator1.buttonVisual.ToggleLogo(false);
            gateActivator1.buttonVisual.ToggleText(true);
            gateActivator1.buttonVisual.ChangeScreenText(message);
            gateActivator1.buttonVisual.ChangeScreenColor(color, true);
        }
        if (gateActivator2 != null)
        {
            gateActivator2.buttonVisual.ToggleLogo(false);
            gateActivator2.buttonVisual.ToggleText(true);
            gateActivator2.buttonVisual.ChangeScreenText(message);
            gateActivator2.buttonVisual.ChangeScreenColor(color, true);
        }
    }

    protected override void OnApplyBrokenVisuals(Color color)
    {
        string brokenMessage = "-- CODE 4 --\nTechnician dispatched";

        if (gateActivator1 != null)
        {
            gateActivator1.buttonVisual.ToggleLogo(false);
            gateActivator1.buttonVisual.ToggleText(true);
            gateActivator1.buttonVisual.ChangeScreenColor(color, true);
            gateActivator1.buttonVisual.ChangeScreenText(brokenMessage);
        }
        if (gateActivator2 != null)
        {
            gateActivator2.buttonVisual.ToggleLogo(false);
            gateActivator2.buttonVisual.ToggleText(true);
            gateActivator2.buttonVisual.ChangeScreenColor(color, true);
            gateActivator2.buttonVisual.ChangeScreenText(brokenMessage);
        }
    }

    protected override void OnResetActivatorVisuals(Color color)
    {
        if (gateActivator1 != null)
        {
            gateActivator1.buttonVisual.ToggleLogo(true);
            gateActivator1.buttonVisual.ToggleText(false);
            gateActivator1.buttonVisual.ChangeScreenColor(color, true, 0.4f);
        }
        if (gateActivator2 != null)
        {
            gateActivator2.buttonVisual.ToggleLogo(true);
            gateActivator2.buttonVisual.ToggleText(false);
            gateActivator2.buttonVisual.ChangeScreenColor(color, true, 0.4f);
        }
    }

    protected override void OnStopActivatorsPulse()
    {
        if (gateActivator1 != null)
        {
            gateActivator1.StopPulseEffect();
        }
        if (gateActivator2 != null)
        {
            gateActivator2.StopPulseEffect();
        }
    }

    protected override void OnStartActivatorsPulse(Color color, float? customDuration = null, float? customIntensity = null)
    {
        if (gateActivator1 != null)
        {
            gateActivator1.StartPulseEffect(color, customDuration, customIntensity);
        }
        if (gateActivator2 != null)
        {
            gateActivator2.StartPulseEffect(color, customDuration, customIntensity);
        }
    }

    protected override void OnTransitionToPulse(Color targetColor, float transitionDuration)
    {
        if (gateActivator1 != null)
        {
            gateActivator1.TransitionToPulseEffect(targetColor, transitionDuration, 0.6f, 1.2f);
        }
        if (gateActivator2 != null)
        {
            gateActivator2.TransitionToPulseEffect(targetColor, transitionDuration, 0.6f, 1.2f);
        }
    }
}