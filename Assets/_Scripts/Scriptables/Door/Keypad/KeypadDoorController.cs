using Cysharp.Threading.Tasks;
using UnityEngine;

public class KeypadDoorController : BaseDoorController
{
    [Header("Door Specific")]
    public string correctCode = "6767";
    public int maxCodeLength = 4;
    public float codeResetDelay = 2f;

    public KeypadDoorActivator doorActivator1;
    public KeypadDoorActivator doorActivator2;

    public override Color32 MovingStateColor => successColor;

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
            doorActivator1.keypadTweener.ToggleLogo(false);
            doorActivator1.keypadTweener.ToggleText(true);
            doorActivator1.keypadTweener.ChangeScreenText(message);
            doorActivator1.keypadTweener.ChangeScreenColor(color, true);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.keypadTweener.ToggleLogo(false);
            doorActivator2.keypadTweener.ToggleText(true);
            doorActivator2.keypadTweener.ChangeScreenText(message);
            doorActivator2.keypadTweener.ChangeScreenColor(color, true);
        }
    }

    protected override void OnApplyBrokenVisuals(Color color)
    {
        string brokenMessage = "-- CODE 4 --Technician dispatched";

        if (doorActivator1 != null)
        {
            doorActivator1.keypadTweener.ToggleLogo(false);
            doorActivator1.keypadTweener.ToggleText(true);
            doorActivator1.keypadTweener.ChangeScreenColor(color, true);
            doorActivator1.keypadTweener.ChangeScreenText(brokenMessage);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.keypadTweener.ToggleLogo(false);
            doorActivator2.keypadTweener.ToggleText(true);
            doorActivator2.keypadTweener.ChangeScreenColor(color, true);
            doorActivator2.keypadTweener.ChangeScreenText(brokenMessage);
        }
    }

    protected override void OnResetActivatorVisuals(Color color)
    {
        if (doorActivator1 != null)
        {
            doorActivator1.keypadTweener.ToggleLogo(true);
            doorActivator1.keypadTweener.ToggleText(false);
            doorActivator1.keypadTweener.ChangeScreenColor(color, true, 0.4f);
        }
        if (doorActivator2 != null)
        {
            doorActivator2.keypadTweener.ToggleLogo(true);
            doorActivator2.keypadTweener.ToggleText(false);
            doorActivator2.keypadTweener.ChangeScreenColor(color, true, 0.4f);
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
            ApplyGrantedVisuals();
        }
        else
        {
            ApplyDeniedVisuals();
        }
    }

    private void ApplyGrantedVisuals()
    {
        if (doorActivator1 != null)
        {
            doorActivator1.keypadTweener.ToggleLogo(false);
            doorActivator1.keypadTweener.ToggleText(true);
            doorActivator1.keypadTweener.ChangeScreenColor(SuccessStateColor, true, 0.5f);
            doorActivator1.keypadTweener.ChangeScreenText("ACCESS\nGRANTED");
            doorActivator1.ResetButtonDisplay().Forget();
        }
        if (doorActivator2 != null)
        {
            doorActivator2.keypadTweener.ToggleLogo(false);
            doorActivator2.keypadTweener.ToggleText(true);
            doorActivator2.keypadTweener.ChangeScreenColor(SuccessStateColor, true, 0.5f);
            doorActivator2.keypadTweener.ChangeScreenText("ACCESS\nGRANTED");
            doorActivator2.ResetButtonDisplay().Forget();
        }
    }

    private void ApplyDeniedVisuals()
    {
        if (doorActivator1 != null)
        {
            doorActivator1.keypadTweener.ToggleLogo(false);
            doorActivator1.keypadTweener.ToggleText(true);
            doorActivator1.keypadTweener.ChangeScreenColor(FailureStateColor, true, 0.5f);
            doorActivator1.keypadTweener.ChangeScreenText("ACCESS\nDENIED");
            doorActivator1.ResetButtonDisplay().Forget();
        }
        if (doorActivator2 != null)
        {
            doorActivator2.keypadTweener.ToggleLogo(false);
            doorActivator2.keypadTweener.ToggleText(true);
            doorActivator2.keypadTweener.ChangeScreenColor(FailureStateColor, true, 0.5f);
            doorActivator2.keypadTweener.ChangeScreenText("ACCESS\nDENIED");
            doorActivator2.ResetButtonDisplay().Forget();
        }
    }
}