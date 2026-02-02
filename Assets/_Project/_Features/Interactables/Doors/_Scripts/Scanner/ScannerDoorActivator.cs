using Cysharp.Threading.Tasks;
using UnityEngine;

public class ScannerDoorActivator : BaseDoorActivator
{
    public override BaseDoorController DoorController => targetDoorController;

    [Space]
    public ScannerDoorVisual buttonVisual;
    public ScannerDoorController targetDoorController;

    public override void Interact()
    {
        if (targetDoorController == null) return;

        if (targetDoorController.currentState == ScannerDoorController.DoorState.Broken ||
            targetDoorController.locked)
        {
            FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.ButtonErrorSound, transform.position);
            return;
        }

        SetButtonState(false);

        int playerHandType = GetPlayerHandType();
        bool handCheckSuccessful = IsCorrectHandType(playerHandType);

        FMODHelper.PlayOneShot3D(
            Core.AudioDataAccess.Doors.ButtonKeypadSound,
            transform.position,
            parameters: new[] { ("Result", handCheckSuccessful ? 0.0f : 1.0f) }
        );

        if (handCheckSuccessful)
        {
            targetDoorController.ToggleDoor().Forget();

            if (Core.Player.Inventory != null && Core.UI.Inventory.IsVisible)
            {
                Core.UI.Inventory.Hide();
            }
        }

        targetDoorController.UpdateActivatorVisuals(handCheckSuccessful, targetDoorController.requiredHandType.ToString());
    }

    private int GetPlayerHandType()
    {
        if (Core.Player.Inventory == null)
            return -1;

        ItemData equippedItem = Core.Player.Inventory.EquippedItem;
        if (equippedItem == null)
            return -1;

        var handBehavior = Core.Player.Inventory.GetEquippedBehavior<SeveredHandBehavior>();
        if (handBehavior != null)
        {
            Core.Player.Inventory.UnequipItem();
            return handBehavior.handType;
        }

        return 0;
    }

    private bool IsCorrectHandType(int playerHandType)
    {
        return playerHandType == targetDoorController.requiredHandType;
    }

    public async UniTask ResetButtonDisplay()
    {
        await UniTask.WaitForSeconds(1.6f, ignoreTimeScale: false);
        buttonVisual.ToggleLogo(true);
        buttonVisual.ToggleText(false);
        buttonVisual.ChangeScreenColor(targetDoorController.SuccessStateColor, true, 0.8f);
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