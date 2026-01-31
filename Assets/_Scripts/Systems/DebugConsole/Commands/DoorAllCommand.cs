using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Console.Commands
{
    public class DoorAllCommand : BaseConsole
    {
        public override string CommandWord => "doorall";
        public override string Description => "Opens, closes, toggles, or breaks all doors.";
        protected override string RawUsage => "doorall <open|close|toggle|break>";

        public override async void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string action = args[0].ToLower();
            if (action != "open" && action != "close" && action != "toggle" && action != "break")
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            int doorCount = 0;
            var doorTasks = new System.Collections.Generic.List<UniTask>();

            // Find all doors
            GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
            foreach (GameObject door in doors)
            {
                var controller = GetDoorController(door);
                if (controller != null)
                {
                    switch (action)
                    {
                        case "open":
                            controller.OpenDoor();
                            doorCount++;
                            break;

                        case "close":
                            controller.CloseDoor();
                            doorCount++;
                            break;

                        case "toggle":
                            doorTasks.Add(controller.ToggleDoor());
                            doorCount++;
                            break;

                        case "break":
                            doorTasks.Add(controller.BreakDoor());
                            doorCount++;
                            break;
                    }
                }
            }

            // Wait for all async operations to complete
            if (doorTasks.Count > 0)
            {
                await UniTask.WhenAll(doorTasks);
            }

            string actionText = action switch
            {
                "open" => "opened",
                "close" => "closed",
                "toggle" => "toggled",
                "break" => "broken",
                _ => action
            };

            ConsoleManager.LogToConsole($"{doorCount} doors {actionText}.".AsSuccess());
        }

        private BaseDoorController GetDoorController(GameObject door) => door.GetComponent<BaseDoorController>();
    }
}