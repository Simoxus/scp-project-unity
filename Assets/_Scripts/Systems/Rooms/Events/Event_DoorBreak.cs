using Cysharp.Threading.Tasks;
using UnityEngine;

public class Event_DoorBreak : RoomEvent
{
    [Header("Trigger")]
    public BoxTrigger trigger;

    [Header("Settings")]
    public int doorIndex = 0;
    public float breakDelay = 2f;

    private bool _hasTriggered = false;

    public override void EventUpdate()
    {
        base.EventUpdate();

        // If using trigger mode, wait for trigger
        if (trigger != null)
        {
            if (!_hasTriggered && trigger.GetState())
            {
                _hasTriggered = true;
                BreakDoorDelayed().Forget();
            }
        }
    }

    public override void EventStart()
    {
        base.EventStart();
    }

    private async UniTask BreakDoorDelayed()
    {
        await UniTask.WaitForSeconds(breakDelay, cancellationToken: _eventCts.Token);

        if (parentRoom.buttonDoors == null || parentRoom.buttonDoors.Length == 0)
            return;

        foreach (var buttonDoor in parentRoom.buttonDoors)
        {
            if (buttonDoor != null)
            {
                await buttonDoor.BreakDoor();
            }
        }

        EventFinish();
    }
}