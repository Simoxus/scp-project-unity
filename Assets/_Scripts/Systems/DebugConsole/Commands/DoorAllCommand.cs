using Cysharp.Threading.Tasks;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Console.Commands
{
    public class DoorAllCommand : BaseConsole
    {
        public override string CommandWord => "doorall";
        public override string Description => "Toggles or breaks all doors and gates.";
        protected override string RawUsage => "doorall <toggle|break>";

        public override async void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                return;
            }

            string action = args[0].ToLower();

            if (action != "toggle" && action != "break")
            {
                ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid argument. Use 'toggle' or 'break'.</color>");
                return;
            }

            int doorCount = 0;
            int gateCount = 0;

            var doorTasks = new System.Collections.Generic.List<UniTask>();
            var gateTasks = new System.Collections.Generic.List<UniTask>();

            // Find all doors
            GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
            foreach (GameObject door in doors)
            {
                var activator = door.GetComponentInChildren<BaseDoorActivator>();
                if (activator != null)
                {
                    var controllerField = activator.GetType().GetField("targetDoorController");
                    if (controllerField != null)
                    {
                        var controller = controllerField.GetValue(activator);
                        if (controller != null)
                        {
                            var controllerType = controller.GetType();
                            MethodInfo method = null;

                            if (action == "toggle")
                                method = controllerType.GetMethod("ToggleDoor");
                            else
                                method = controllerType.GetMethod("BreakDoor");

                            if (method != null)
                            {
                                doorTasks.Add((UniTask)method.Invoke(controller, null));
                                doorCount++;
                            }
                        }
                    }
                }
            }

            // Find all gates
            GameObject[] gates = GameObject.FindGameObjectsWithTag("Gate");
            foreach (GameObject gate in gates)
            {
                var activator = gate.GetComponentInChildren<BaseGateActivator>();
                if (activator != null)
                {
                    var controllerField = activator.GetType().GetField("targetGateController");
                    if (controllerField != null)
                    {
                        var controller = controllerField.GetValue(activator);
                        if (controller != null)
                        {
                            var controllerType = controller.GetType();
                            MethodInfo method = null;

                            if (action == "toggle")
                                method = controllerType.GetMethod("ToggleGate");
                            else
                                method = controllerType.GetMethod("BreakGate");

                            if (method != null)
                            {
                                gateTasks.Add((UniTask)method.Invoke(controller, null));
                                gateCount++;
                            }
                        }
                    }
                }
            }

            // Find all cell doors (doors without activators)
            foreach (GameObject door in doors)
            {
                var controller = door.GetComponent<BaseDoorController>();
                if (controller != null)
                {
                    // Check if it has an activator - if not, it's a cell door
                    var hasActivator = door.GetComponentInChildren<BaseDoorActivator>() != null;
                    if (!hasActivator)
                    {
                        var controllerType = controller.GetType();
                        MethodInfo method = null;

                        if (action == "toggle")
                            method = controllerType.GetMethod("ToggleDoor");
                        else
                            method = controllerType.GetMethod("BreakDoor");

                        if (method != null)
                        {
                            doorTasks.Add((UniTask)method.Invoke(controller, null));
                            doorCount++;
                        }
                    }
                }
            }

            // Wait for all doors and gates to complete simultaneously
            await UniTask.WhenAll(doorTasks.Concat(gateTasks));

            string actionText = action == "toggle" ? "toggled" : "broken";
            ConsoleManager.LogToConsole($"<color=#33CC33>{doorCount} doors {actionText}, {gateCount} gates toggled.</color>");
        }
    }
}