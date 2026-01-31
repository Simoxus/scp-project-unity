using UnityEngine;

public class ButtonDoorController : BaseDoorController
{
    [Header("Door Specific")]
    public ButtonDoorActivator doorActivator1;
    public ButtonDoorActivator doorActivator2;

    protected override void OnSetActivatorsState(bool enabled)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.SetButtonState(enabled);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.SetButtonState(enabled);
        }
    }

    protected override void OnApplyLockedVisuals(Color color, string message)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.ButtonVisual.ToggleLogo(false);
            doorActivator1.ButtonVisual.ToggleText(true);
            doorActivator1.ButtonVisual.ChangeScreenText(message);
            doorActivator1.ButtonVisual.ChangeScreenColor(color, true);
        }

        if (doorActivator2 != null)
        {
            doorActivator2.ButtonVisual.ToggleLogo(false);
            doorActivator2.ButtonVisual.ToggleText(true);
            doorActivator2.ButtonVisual.ChangeScreenText(message);
            doorActivator2.ButtonVisual.ChangeScreenColor(color, true);
        }
    }

    protected override void OnApplyBrokenVisuals(Color color)
    {
        string brokenMessage = "-- CODE 4 --\nTechnician dispatched";

        if (doorActivator1 != null)
        {
            doorActivator1.ButtonVisual.ToggleLogo(false);
            doorActivator1.ButtonVisual.ToggleText(true);
            doorActivator1.ButtonVisual.ChangeScreenColor(color, true);
            doorActivator1.ButtonVisual.ChangeScreenText(brokenMessage);
        }

        if (doorActivator2 != null)
        {
            doorActivator2.ButtonVisual.ToggleLogo(false);
            doorActivator2.ButtonVisual.ToggleText(true);
            doorActivator2.ButtonVisual.ChangeScreenColor(color, true);
            doorActivator2.ButtonVisual.ChangeScreenText(brokenMessage);
        }
    }

    protected override void OnResetActivatorVisuals(Color color)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.ButtonVisual.ToggleLogo(true);
            doorActivator1.ButtonVisual.ToggleText(false);
            doorActivator1.ButtonVisual.ChangeScreenColor(color, true, 0.4f);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.ButtonVisual.ToggleLogo(true);
            doorActivator2.ButtonVisual.ToggleText(false);
            doorActivator2.ButtonVisual.ChangeScreenColor(color, true, 0.4f);
        }
    }

    protected override void OnStopActivatorsPulse()
    {
        if (doorActivator1 != null)
        {
            doorActivator1.StopPulseEffect();
        }
        if (doorActivator2 != null)
        {
            doorActivator2.StopPulseEffect();
        }
    }

    protected override void OnStartActivatorsPulse(Color color, float? customDuration = null, float? customIntensity = null)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.StartPulseEffect(color, customDuration, customIntensity);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.StartPulseEffect(color, customDuration, customIntensity);
        }
    }
}