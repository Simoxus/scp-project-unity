using Cysharp.Threading.Tasks;
using UnityEngine;

public class KeycardDoorActivator : BaseDoorActivator
{
    [Header("Script References")]
    public KeycardDoorVisual buttonVisual;
    public KeycardDoorController targetDoorController;

    public override void Interact()
    {
        if (targetDoorController == null) return;

        if (targetDoorController.currentState == KeycardDoorController.DoorState.Broken ||
            targetDoorController.locked)
        {
            FMODHelper.PlayOneShot3D(Core.AudioDataAccess.Doors.ButtonErrorSound, transform.position);
            return;
        }

        SetButtonState(false);

        // Get the player's current keycard level
        int playerKeycardLevel = GetPlayerKeycardLevel();
        bool keycardCheckSuccessful = IsCorrectKeycardLevel(playerKeycardLevel);

        FMODHelper.PlayOneShotWithParameters(
            Core.AudioDataAccess.Doors.ButtonKeycardSound,
            transform.position,
            ("Result", keycardCheckSuccessful ? 0.0f : 1.0f)
        );

        if (keycardCheckSuccessful)
        {
            targetDoorController.ToggleDoor().Forget();

            if (Core.Player.Inventory != null && Core.UI.Inventory.IsVisible)
            {
                Core.UI.Inventory.Hide();
            }
        }

        targetDoorController.UpdateActivatorVisuals(keycardCheckSuccessful, targetDoorController.requiredKeycardLevel.ToString());
    }

    private int GetPlayerKeycardLevel()
    {
        if (Core.Player.Inventory == null)
            return -1;

        ItemData equippedItem = Core.Player.Inventory.EquippedItem;

        if (equippedItem == null)
            return -1;

        var keycardBehavior = Core.Player.Inventory.GetEquippedBehavior<KeycardBehavior>();
        if (keycardBehavior != null)
        {
            Core.Player.Inventory.UnequipItem();
            return keycardBehavior.keycardLevel;
        }

        return 0;
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