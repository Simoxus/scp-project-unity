using Cysharp.Threading.Tasks;
using UnityEngine;

public class KeycardDoorController : BaseDoorController
{
    [Header("Door Specific")]
    public int requiredKeycardLevel = 0;
    public KeycardDoorActivator doorActivator1;
    public KeycardDoorActivator doorActivator2;

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
            doorActivator1.KeycardVisual.ToggleLogo(false);
            doorActivator1.KeycardVisual.ToggleText(true);
            doorActivator1.KeycardVisual.ChangeScreenText(message);
            doorActivator1.KeycardVisual.ChangeScreenColor(color, true);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.KeycardVisual.ToggleLogo(false);
            doorActivator2.KeycardVisual.ToggleText(true);
            doorActivator2.KeycardVisual.ChangeScreenText(message);
            doorActivator2.KeycardVisual.ChangeScreenColor(color, true);
        }
    }

    protected override void OnApplyBrokenVisuals(Color color)
    {
        string brokenMessage = "-- CODE 4 --\nTechnician dispatched";

        if (doorActivator1 != null)
        {
            doorActivator1.KeycardVisual.ToggleLogo(false);
            doorActivator1.KeycardVisual.ToggleText(true);
            doorActivator1.KeycardVisual.ChangeScreenColor(color, true);
            doorActivator1.KeycardVisual.ChangeScreenText(brokenMessage);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.KeycardVisual.ToggleLogo(false);
            doorActivator2.KeycardVisual.ToggleText(true);
            doorActivator2.KeycardVisual.ChangeScreenColor(color, true);
            doorActivator2.KeycardVisual.ChangeScreenText(brokenMessage);
        }
    }

    protected override void OnResetActivatorVisuals(Color color)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.KeycardVisual.ToggleLogo(true);
            doorActivator1.KeycardVisual.ToggleText(false);
            doorActivator1.KeycardVisual.ChangeScreenColor(color, true, 0.4f);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.KeycardVisual.ToggleLogo(true);
            doorActivator2.KeycardVisual.ToggleText(false);
            doorActivator2.KeycardVisual.ChangeScreenColor(color, true, 0.4f);
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
            doorActivator1.KeycardVisual.ToggleLogo(false);
            doorActivator1.KeycardVisual.ToggleText(true);
            doorActivator1.KeycardVisual.ChangeScreenColor(successColor, true, 1f);
            doorActivator1.KeycardVisual.ChangeScreenText($"LEVEL {clearanceLevel} DETECTED");
            doorActivator1.ResetButtonDisplay().Forget();
        }
        if (doorActivator2 != null)
        {
            doorActivator2.KeycardVisual.ToggleLogo(false);
            doorActivator2.KeycardVisual.ToggleText(true);
            doorActivator2.KeycardVisual.ChangeScreenColor(successColor, true, 1f);
            doorActivator2.KeycardVisual.ChangeScreenText($"LEVEL {clearanceLevel} DETECTED");
            doorActivator2.ResetButtonDisplay().Forget();
        }
    }

    private void ApplyDeniedVisuals(string clearanceLevel)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.KeycardVisual.ToggleLogo(false);
            doorActivator1.KeycardVisual.ToggleText(true);
            doorActivator1.KeycardVisual.ChangeScreenColor(failureColor, true, 1f);
            doorActivator1.KeycardVisual.ChangeScreenText($"LEVEL {clearanceLevel} REQUIRED");
            doorActivator1.ResetButtonDisplay().Forget();
        }
        if (doorActivator2 != null)
        {
            doorActivator2.KeycardVisual.ToggleLogo(false);
            doorActivator2.KeycardVisual.ToggleText(true);
            doorActivator2.KeycardVisual.ChangeScreenColor(failureColor, true, 1f);
            doorActivator2.KeycardVisual.ChangeScreenText($"LEVEL {clearanceLevel} REQUIRED");
            doorActivator2.ResetButtonDisplay().Forget();
        }
    }
}