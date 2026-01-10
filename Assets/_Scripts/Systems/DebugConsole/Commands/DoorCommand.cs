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
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string action = args[0].ToLower();

            CinemachineCamera cameraMain = Core.Player.CameraMain;
            if (cameraMain == null) return;

            Ray ray = new(cameraMain.transform.position, cameraMain.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, 15f)) return;

            MonoBehaviour activator = FindActivator(hit.collider.gameObject);
            BaseDoorController doorController = null;

            if (activator == null)
            {
                doorController = FindDoorController(hit.collider.gameObject);
            }

            if (activator == null && doorController == null)
            {
                StudioEventEmitter fmodCollision = hit.collider.GetComponent<StudioEventEmitter>();
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();

                if (fmodCollision != null && rb != null)
                {
                    Transform grandparent = hit.collider.transform.parent?.parent;
                    if (grandparent != null)
                    {
                        activator = FindActivator(grandparent.gameObject);

                        if (activator == null)
                        {
                            doorController = FindDoorController(grandparent.gameObject);
                        }
                    }
                }
            }

            if (activator == null && doorController == null)
            {
                ConsoleManager.LogToConsole("The object hit is not a valid door activator.".AsError());
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
                    ConsoleManager.LogToConsole("Invalid argument. Use 'toggle' or 'break'.".AsError());
                    break;
            }
        }

        private MonoBehaviour FindActivator(GameObject obj)
        {
            var doorActivator = obj.GetComponentInChildren<BaseDoorActivator>();
            if (doorActivator != null) return doorActivator;

            var gateActivator = obj.GetComponentInChildren<BaseGateActivator>();
            return gateActivator;
        }

        private BaseDoorController FindDoorController(GameObject obj)
        {
            var controller = obj.GetComponent<BaseDoorController>();
            if (controller != null) return controller;

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
                            ConsoleManager.LogToConsole($"{activatorType} door toggled.".AsSuccess());
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
                            ConsoleManager.LogToConsole($"{activatorType} gate toggled.".AsSuccess());
                            await (UniTask)method.Invoke(controller, null);
                            return;
                        }
                    }
                }
            }

            ConsoleManager.LogToConsole("Could not find controller for activator.".AsError());
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
                            ConsoleManager.LogToConsole($"{activatorType} door broken.".AsSuccess());
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
                            ConsoleManager.LogToConsole($"{activatorType} gate toggled.".AsSuccess());
                            await (UniTask)method.Invoke(controller, null);
                            return;
                        }
                    }
                }
            }

            ConsoleManager.LogToConsole("Could not find controller for activator.".AsError());
        }

        private async UniTask HandleToggleController(BaseDoorController controller)
        {
            var method = controller.GetType().GetMethod("ToggleDoor");
            if (method != null)
            {
                ConsoleManager.LogToConsole($"Cell door toggled.".AsSuccess());
                await (UniTask)method.Invoke(controller, null);
            }
        }

        private async UniTask HandleBreakController(BaseDoorController controller)
        {
            var method = controller.GetType().GetMethod("BreakDoor");
            if (method != null)
            {
                ConsoleManager.LogToConsole($"Cell door broken.".AsSuccess());
                await (UniTask)method.Invoke(controller, null);
            }
        }
    }
}