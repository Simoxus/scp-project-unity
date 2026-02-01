using Cysharp.Threading.Tasks;
using UnityEngine;

public class ScannerDoorController : BaseDoorController
{
    [Space]
    [Header("Door Specific")]
    public int requiredHandType = 0;

    public ScannerDoorActivator doorActivator1;
    public ScannerDoorActivator doorActivator2;

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
            doorActivator1.buttonVisual.ToggleLogo(false);
            doorActivator1.buttonVisual.ToggleText(true);
            doorActivator1.buttonVisual.ChangeScreenText(message);
            doorActivator1.buttonVisual.ChangeScreenColor(color, true);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.buttonVisual.ToggleLogo(false);
            doorActivator2.buttonVisual.ToggleText(true);
            doorActivator2.buttonVisual.ChangeScreenText(message);
            doorActivator2.buttonVisual.ChangeScreenColor(color, true);
        }
    }

    protected override void OnApplyBrokenVisuals(Color color)
    {
        string brokenMessage = "-- CODE 4 --\nTechnician dispatched";

        if (doorActivator1 != null)
        {
            doorActivator1.buttonVisual.ToggleLogo(false);
            doorActivator1.buttonVisual.ToggleText(true);
            doorActivator1.buttonVisual.ChangeScreenColor(color, true);
            doorActivator1.buttonVisual.ChangeScreenText(brokenMessage);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.buttonVisual.ToggleLogo(false);
            doorActivator2.buttonVisual.ToggleText(true);
            doorActivator2.buttonVisual.ChangeScreenColor(color, true);
            doorActivator2.buttonVisual.ChangeScreenText(brokenMessage);
        }
    }

    protected override void OnResetActivatorVisuals(Color color)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.buttonVisual.ToggleLogo(true);
            doorActivator1.buttonVisual.ToggleText(false);
            doorActivator1.buttonVisual.ChangeScreenColor(color, true, 0.4f);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.buttonVisual.ToggleLogo(true);
            doorActivator2.buttonVisual.ToggleText(false);
            doorActivator2.buttonVisual.ChangeScreenColor(color, true, 0.4f);
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

    public void UpdateActivatorVisuals(bool success, string clearanceLevel)
    {
        if (success)
        {
            ApplyGrantedVisuals(clearanceLevel);
        }
        else
        {
            ApplyDeniedVisuals(clearanceLevel);
        }
    }

    private void ApplyGrantedVisuals(string clearanceLevel)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.buttonVisual.ToggleLogo(false);
            doorActivator1.buttonVisual.ToggleText(true);
            doorActivator1.buttonVisual.ChangeScreenColor(successColor, true, 1f);
            doorActivator1.buttonVisual.ChangeScreenText($"DNA verified. Access granted.");
            doorActivator1.ResetButtonDisplay().Forget();
        }
        if (doorActivator2 != null)
        {
            doorActivator2.buttonVisual.ToggleLogo(false);
            doorActivator2.buttonVisual.ToggleText(true);
            doorActivator2.buttonVisual.ChangeScreenColor(successColor, true, 1f);
            doorActivator2.buttonVisual.ChangeScreenText($"DNA verified. Access granted.");
            doorActivator2.ResetButtonDisplay().Forget();
        }
    }

    private void ApplyDeniedVisuals(string clearanceLevel)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.buttonVisual.ToggleLogo(false);
            doorActivator1.buttonVisual.ToggleText(true);
            doorActivator1.buttonVisual.ChangeScreenColor(failureColor, true, 1f);
            doorActivator1.buttonVisual.ChangeScreenText($"DNA does not match known list. Access denied.");
            doorActivator1.ResetButtonDisplay().Forget();
        }
        if (doorActivator2 != null)
        {
            doorActivator2.buttonVisual.ToggleLogo(false);
            doorActivator2.buttonVisual.ToggleText(true);
            doorActivator2.buttonVisual.ChangeScreenColor(failureColor, true, 1f);
            doorActivator2.buttonVisual.ChangeScreenText($"DNA does not match known list. Access denied.");
            doorActivator2.ResetButtonDisplay().Forget();
        }
    }
}