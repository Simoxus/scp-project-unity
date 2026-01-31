using UnityEngine;

namespace Console.Commands
{
    public class DoorAllImCommand : BaseConsole
    {
        public override string CommandWord => "doorallim";
        public override string Description => "Immediately opens or closes all doors.";
        protected override string RawUsage => "doorallim <open|close>";

        public override void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string action = args[0].ToLower();
            if (action != "open" && action != "close")
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            int doorCount = 0;

            GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
            foreach (GameObject door in doors)
            {
                var controller = GetDoorController(door);
                if (controller != null)
                {
                    if (action == "open")
                    {
                        controller.OpenDoorImmediate();
                    }
                    else
                    {
                        controller.CloseDoorImmediate();
                    }
                    doorCount++;
                }
            }

            string actionText = action == "open" ? "opened" : "closed";
            ConsoleManager.LogToConsole($"{doorCount} doors {actionText}.".AsSuccess());
        }

        private BaseDoorController GetDoorController(GameObject door) => door.GetComponent<BaseDoorController>();
    }
}