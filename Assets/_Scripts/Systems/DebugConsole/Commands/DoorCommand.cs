using Cysharp.Threading.Tasks;
using FMODUnity;
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

            // Try to find any door or gate activator
            MonoBehaviour activator = FindActivator(hit.collider.gameObject);
            BaseDoorController doorController = null;

            // If no activator, try to find door controller directly (for cell doors)
            if (activator == null)
            {
                doorController = FindDoorController(hit.collider.gameObject);
            }

            // If no activator found, check if it's a door part with FMOD Collision and Rigidbody
            if (activator == null && doorController == null)
            {
                StudioEventEmitter fmodCollision = hit.collider.GetComponent<StudioEventEmitter>();
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();

                if (fmodCollision != null && rb != null)
                {
                    // Try to find activator in grandparent
                    Transform grandparent = hit.collider.transform.parent?.parent;
                    if (grandparent != null)
                    {
                        activator = FindActivator(grandparent.gameObject);

                        if (activator == null)
                        {
                            doorController = FindDoorController(grandparent.gameObject);
                        }

                        if (activator != null || doorController != null)
                        {
                            ConsoleManager.LogToConsole("<color=#ADD8E6FF>Found door controller in grandparent object.</color>");
                        }
                    }
                }
            }

            if (activator == null && doorController == null)
            {
                ConsoleManager.LogToConsole("<color=#ADD8E6FF>The object hit is not a valid door activator.</color>");
                return;
            }

            switch (action)
            {
                case "toggle":
                    if (activator != null)
                        await HandleToggle(activator);
                    else
                        await HandleToggleController(doorController);
                    break;

                case "break":
                    if (activator != null)
                        await HandleBreak(activator);
                    else
                        await HandleBreakController(doorController);
                    break;

                default:
                    ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid argument. Use 'toggle' or 'break'.</color>");
                    break;
            }
        }

        private MonoBehaviour FindActivator(GameObject obj)
        {
            // Try to find BaseDoorActivator first
            var doorActivator = obj.GetComponentInChildren<BaseDoorActivator>();
            if (doorActivator != null) return doorActivator;

            // Try to find BaseGateActivator
            var gateActivator = obj.GetComponentInChildren<BaseGateActivator>();
            return gateActivator;
        }

        private BaseDoorController FindDoorController(GameObject obj)
        {
            // Check hit object first
            var controller = obj.GetComponent<BaseDoorController>();
            if (controller != null) return controller;

            // Check children
            controller = obj.GetComponentInChildren<BaseDoorController>();
            return controller;
        }

        private async UniTask HandleToggle(MonoBehaviour activator)
        {
            if (activator is BaseDoorActivator doorActivator)
            {
                var controllerField = doorActivator.GetType().GetField("targetDoorController");
                if (controllerField != null)
                {
                    var controller = controllerField.GetValue(doorActivator);
                    if (controller != null)
                    {
                        var method = controller.GetType().GetMethod("ToggleDoor");
                        if (method != null)
                        {
                            string activatorType = doorActivator.GetType().Name.Replace("DoorActivator", "");
                            ConsoleManager.LogToConsole($"<color=#33CC33>{activatorType} door toggled.</color>");
                            await (UniTask)method.Invoke(controller, null);
                            return;
                        }
                    }
                }
            }
            else if (activator is BaseGateActivator gateActivator)
            {
                var controllerField = gateActivator.GetType().GetField("targetGateController");
                if (controllerField != null)
                {
                    var controller = controllerField.GetValue(gateActivator);
                    if (controller != null)
                    {
                        var method = controller.GetType().GetMethod("ToggleGate");
                        if (method != null)
                        {
                            string activatorType = gateActivator.GetType().Name.Replace("GateActivator", "");
                            ConsoleManager.LogToConsole($"<color=#33CC33>{activatorType} gate toggled.</color>");
                            await (UniTask)method.Invoke(controller, null);
                            return;
                        }
                    }
                }
            }

            ConsoleManager.LogToConsole("<color=#FF0000FF>Could not find controller for activator.</color>");
        }

        private async UniTask HandleBreak(MonoBehaviour activator)
        {
            if (activator is BaseDoorActivator doorActivator)
            {
                var controllerField = doorActivator.GetType().GetField("targetDoorController");
                if (controllerField != null)
                {
                    var controller = controllerField.GetValue(doorActivator);
                    if (controller != null)
                    {
                        var method = controller.GetType().GetMethod("BreakDoor");
                        if (method != null)
                        {
                            string activatorType = doorActivator.GetType().Name.Replace("DoorActivator", "");
                            ConsoleManager.LogToConsole($"<color=#33CC33>{activatorType} door broken.</color>");
                            await (UniTask)method.Invoke(controller, null);
                            return;
                        }
                    }
                }
            }
            else if (activator is BaseGateActivator gateActivator)
            {
                var controllerField = gateActivator.GetType().GetField("targetGateController");
                if (controllerField != null)
                {
                    var controller = controllerField.GetValue(gateActivator);
                    if (controller != null)
                    {
                        var method = controller.GetType().GetMethod("ToggleGate");
                        if (method != null)
                        {
                            string activatorType = gateActivator.GetType().Name.Replace("GateActivator", "");
                            ConsoleManager.LogToConsole($"<color=#33CC33>{activatorType} gate toggled.</color>");
                            await (UniTask)method.Invoke(controller, null);
                            return;
                        }
                    }
                }
            }

            ConsoleManager.LogToConsole("<color=#FF0000FF>Could not find controller for activator.</color>");
        }

        private async UniTask HandleToggleController(BaseDoorController controller)
        {
            var method = controller.GetType().GetMethod("ToggleDoor");
            if (method != null)
            {
                ConsoleManager.LogToConsole($"<color=#33CC33>Cell door toggled.</color>");
                await (UniTask)method.Invoke(controller, null);
            }
        }

        private async UniTask HandleBreakController(BaseDoorController controller)
        {
            var method = controller.GetType().GetMethod("BreakDoor");
            if (method != null)
            {
                ConsoleManager.LogToConsole($"<color=#33CC33>Cell door broken.</color>");
                await (UniTask)method.Invoke(controller, null);
            }
        }
    }
}