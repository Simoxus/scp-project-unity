using UnityEngine;

public class HandleDoorController : BaseDoorController
{
    [Header("Door Specific")]
    public HandleDoorActivator doorActivator1;
    public HandleDoorActivator doorActivator2;

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
    }

    protected override void OnApplyBrokenVisuals(Color color)
    {
    }

    protected override void OnResetActivatorVisuals(Color color)
    {
    }

    protected override void OnStopActivatorsPulse()
    {
    }

    protected override void OnStartActivatorsPulse(Color color, float? customDuration = null, float? customIntensity = null)
    {
    }
}