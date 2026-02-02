using FMODUnity;
using Unity.Cinemachine;
using UnityEngine;

namespace Console.Commands
{
    public class DoorCommand : BaseConsole
    {
        public override string CommandWord => "door";
        public override string Description => "Casts a ray from the camera to open, close, toggle, or break a door.";
        protected override string RawUsage => "door <open|close|toggle|break>";

        public override async void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string action = args[0].ToLower();

            CinemachineCamera cameraMain = Core.Player.CameraMain;
            if (cameraMain == null)
            {
                return;
            }

            Ray ray = new(cameraMain.transform.position, cameraMain.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, 15f))
            {
                return;
            }

            BaseDoorController doorController = FindDoorController(hit.collider.gameObject);

            // If not found on hit object, try grandparent (for doors with physics colliders)
            if (doorController == null)
            {
                StudioEventEmitter fmodCollision = hit.collider.GetComponent<StudioEventEmitter>();
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();

                if (fmodCollision != null && rb != null)
                {
                    Transform grandparent = hit.collider.transform.parent?.parent;
                    if (grandparent != null)
                    {
                        doorController = FindDoorController(grandparent.gameObject);
                    }
                }
            }

            if (doorController == null)
            {
                ConsoleManager.LogToConsole("The object hit is not a valid door.".AsError());
                return;
            }

            string doorTypeName = GetDoorTypeName(hit.collider.gameObject);

            switch (action)
            {
                case "open":
                    doorController.OpenDoor();
                    ConsoleManager.LogToConsole($"{doorTypeName} door opened.".AsSuccess());
                    break;

                case "close":
                    doorController.CloseDoor();
                    ConsoleManager.LogToConsole($"{doorTypeName} door closed.".AsSuccess());
                    break;

                case "toggle":
                    await doorController.ToggleDoor();
                    ConsoleManager.LogToConsole($"{doorTypeName} door toggled.".AsSuccess());
                    break;

                case "break":
                    await doorController.BreakDoor();
                    ConsoleManager.LogToConsole($"{doorTypeName} door broken.".AsSuccess());
                    break;

                default:
                    ConsoleManager.LogToConsole(Usage.AsError());
                    break;
            }
        }

        private BaseDoorController FindDoorController(GameObject obj)
        {
            // First try to get controller from activator using property
            var activator = obj.GetComponentInChildren<BaseDoorActivator>();
            if (activator != null && activator.DoorController != null)
            {
                return activator.DoorController;
            }

            // Otherwise get controller directly
            var directController = obj.GetComponent<BaseDoorController>();
            if (directController != null)
            {
                return directController;
            }

            return obj.GetComponentInChildren<BaseDoorController>();
        }

        private string GetDoorTypeName(GameObject obj)
        {
            var activator = obj.GetComponentInChildren<BaseDoorActivator>();
            if (activator != null)
            {
                return activator.GetType().Name.Replace("DoorActivator", "");
            }
            return "Cell";
        }
    }
}