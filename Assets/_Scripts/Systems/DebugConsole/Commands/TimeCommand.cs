using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Console.Commands
{
    public class TimeCommand : ConsoleBase
    {
        public override string CommandWord => "time";
        public override string Description => "Modifies the scene's timeScale.";
        protected override string RawUsage => "time <get|set> [value]";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                return;
            }

            string method = args[0].ToLower();
            float value = 0f;

            if (method == "get")
            {
                if (args.Length > 1)
                {
                    ConsoleManager.LogToConsole($"<color=#FFA500FF>Warning: 'get' method does not require a value. Ignoring '{args[1]}'.</color>");
                }
                ConsoleManager.LogToConsole($"Current timeScale: {Time.timeScale}");
            }
            else if (method == "set")
            {
                if (args.Length < 2)
                {
                    ConsoleManager.LogToConsole("<color=#FF0000FF>Usage: time set <value></color>");
                    return;
                }

                if (!float.TryParse(args[1], out value))
                {
                    ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid value for 'set'. Please enter a number.</color>");
                    return;
                }

                // Set the timeScale
                Time.timeScale = value;
                ConsoleManager.LogToConsole($"TimeScale set to {value}.");
            }
            else
            {
                ConsoleManager.LogToConsole("<color=#FF0000FF>Unknown time method. Use 'get' or 'set'.</color>");
                return;
            }
        }
    }
}
