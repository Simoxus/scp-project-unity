using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

namespace Console.Commands
{
    public class DoorCommand : BaseConsole
    {
        public override string CommandWord => "door";
        public override string Description => "Casts a ray from the player's camera to toggle or break a door.";
        protected override string RawUsage => "door <toggle|break>";

        public override async void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                return;
            }

            string action = args[0].ToLower();

            Player player = Player.Instance;
            if (player == null)
            {
                ConsoleManager.LogToConsole("<color=#FF0000FF>Player GameObject not found.</color>");
                return;
            }

            CinemachineCamera cameraMain = player.cameraMain;
            if (cameraMain == null)
            {
                ConsoleManager.LogToConsole("<color=#FF0000FF>Main Cinemachine camera was not assigned in the Player.</color>");
                return;
            }

            // Raycast from the player's camera
            Ray ray = new(cameraMain.transform.position, cameraMain.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, 15f)) return;

            // Try to get references
            ButtonDoorActivator buttonActivator = hit.collider.GetComponent<ButtonDoorActivator>();
            KeycardDoorActivator keycardActivator = hit.collider.GetComponent<KeycardDoorActivator>();
            KeypadDoorActivator keypadActivator = hit.collider.GetComponent<KeypadDoorActivator>();

            // Check if we hit anything valid
            bool hasActivator = buttonActivator != null || keycardActivator != null || keypadActivator != null;

            if (!hasActivator)
            {
                ConsoleManager.LogToConsole("<color=#ADD8E6FF>The object hit is not a valid door activator.</color>");
                return;
            }

            switch (action)
            {
                case "toggle":
                    await HandleToggle(buttonActivator, keycardActivator, keypadActivator);
                    break;

                case "break":
                    await HandleBreak(buttonActivator, keycardActivator, keypadActivator);
                    break;

                default:
                    ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid argument. Use 'toggle' or 'break'.</color>");
                    break;
            }
        }

        private async UniTask HandleToggle(
            ButtonDoorActivator buttonActivator, KeycardDoorActivator keycardActivator, KeypadDoorActivator keypadActivator)
        {
            // Handle activators
            if (buttonActivator != null)
            {
                ConsoleManager.LogToConsole($"<color=#33CC33>Button door toggled.</color>");
                await buttonActivator.targetDoorController.ToggleDoor();
                return;
            }
            if (keycardActivator != null)
            {
                ConsoleManager.LogToConsole($"<color=#33CC33>Keycard door toggled.</color>");
                await keycardActivator.targetDoorController.ToggleDoor();
                return;
            }
            if (keypadActivator != null)
            {
                ConsoleManager.LogToConsole($"<color=#33CC33>Keypad door toggled.</color>");
                await keypadActivator.targetDoorController.ToggleDoor();
                return;
            }
        }

        private async UniTask HandleBreak(
            ButtonDoorActivator buttonActivator, KeycardDoorActivator keycardActivator, KeypadDoorActivator keypadActivator)
        {
            // Handle activators
            if (buttonActivator != null)
            {
                ConsoleManager.LogToConsole($"<color=#33CC33>Button door broken.</color>");
                await buttonActivator.targetDoorController.BreakDoor();
                return;
            }
            if (keycardActivator != null)
            {
                ConsoleManager.LogToConsole($"<color=#33CC33>Keycard door broken.</color>");
                await keycardActivator.targetDoorController.BreakDoor();
                return;
            }
            if (keypadActivator != null)
            {
                ConsoleManager.LogToConsole($"<color=#33CC33>Keypad door broken.</color>");
                await keypadActivator.targetDoorController.BreakDoor();
                return;
            }
        }
    }
}

