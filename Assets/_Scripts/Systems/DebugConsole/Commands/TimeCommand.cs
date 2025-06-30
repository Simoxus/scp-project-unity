using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimeCommand : ConsoleBase
{
    public override string CommandWord => "time";
    public override string Description => "Modifies the scene's timeScale. Usage: time <method (get|set)> [value]";

    public override void Execute(string[] args)
    {
        // Require at least a method argument
        if (args.Length < 1)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: time <method (get|set)> [value]</color>");
            return;
        }

        string method = args[0].ToLower(); // Get the method (e.g., "get", "set")
        float value = 0f; // Default value, used only for 'set'

        // Handle 'get' which does not require a value
        if (method == "get")
        {
            if (args.Length > 1)
            {
                ConsoleManager.LogToConsole($"<color=orange>Warning: 'get' method does not require a value. Ignoring '{args[1]}'.</color>");
            }
            ConsoleManager.LogToConsole($"Current timeScale: {Time.timeScale}");
        }
        else if (method == "set")
        {
            // 'set' method requires a value
            if (args.Length < 2)
            {
                ConsoleManager.LogToConsole("<color=red>Usage: time set <value></color>");
                return;
            }

            // Try to parse the value argument for 'set'
            if (!float.TryParse(args[1], out value))
            {
                ConsoleManager.LogToConsole("<color=red>Invalid value for 'set'. Please enter a number.</color>");
                return;
            }

            // Set the game's timeScale
            Time.timeScale = value;
            ConsoleManager.LogToConsole($"TimeScale set to {value}.");
        }
        else
        {
            ConsoleManager.LogToConsole("<color=red>Unknown time method. Use 'get' or 'set'.</color>");
            return;
        }
    }
}
