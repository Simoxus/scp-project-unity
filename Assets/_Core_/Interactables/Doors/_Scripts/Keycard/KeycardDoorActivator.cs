using Cysharp.Threading.Tasks;
using UnityEngine;

public class KeycardDoorActivator : BaseDoorActivator
{
    public override BaseDoorController DoorController => targetDoorController;

    [Space]
    public KeycardDoorVisual KeycardVisual;
    [SerializeField] private KeycardDoorController targetDoorController;

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

        FMODHelper.PlayOneShot3D(
            Core.AudioDataAccess.Doors.ButtonKeycardSound,
            transform.position,
            parameters: new[] { ("Result", keycardCheckSuccessful ? 0.0f : 1.0f) }
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
        KeycardVisual.ToggleLogo(true);
        KeycardVisual.ToggleText(false);
        KeycardVisual.ChangeScreenColor(targetDoorController.SuccessStateColor, true, 0.8f);
        KeycardVisual.ChangeScreenText("HI");
        await UniTask.WaitForSeconds(0.15f, ignoreTimeScale: false);

        SetButtonState(true);
    }

    public override void StartPulseEffect(Color startColor, float? customDuration = null, float? customIntensity = null)
    {
        if (KeycardVisual != null)
        {
            KeycardVisual.StartPulse(startColor, customDuration, customIntensity);
        }
    }

    public override void StopPulseEffect()
    {
        if (KeycardVisual != null)
        {
            KeycardVisual.StopPulse();
        }
    }

    public void TransitionToPulseEffect(Color targetColor, float transitionDuration, float pulseDuration, float pulseIntensity)
    {
        if (KeycardVisual != null)
        {
            KeycardVisual.TransitionToPulse(targetColor, transitionDuration, pulseDuration, pulseIntensity);
        }
    }
}