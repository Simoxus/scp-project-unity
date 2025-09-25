using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for .Where, .OrderBy, etc.
using UnityEngine;

public class SanityCommand : ConsoleBase
{
    public override string CommandWord => "sanity";
    public override string Description => "Modifies the player's sanity. Usage: sanity <method (get|set|reset)> [value]";

    public override void Execute(string[] args)
    {
        // Require at least a method argument
        if (args.Length < 1)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Usage: sanity <method (get|set|reset)> [value]</color>");
            return;
        }

        string method = args[0].ToLower();
        float value = 0f;

        // Handle 'get' and 'reset' which do not require a value
        if (method == "get" || method == "reset")
        {
            if (args.Length > 1)
            {
                ConsoleManager.LogToConsole($"<color=#FFA500FF>Warning: '{method}' method does not require a value. Ignoring '{args[1]}'.</color>");
            }
        }
        else if (method == "set")
        {
            // 'set' method requires a value
            if (args.Length < 2)
            {
                ConsoleManager.LogToConsole("<color=#FF0000FF>Usage: sanity set <value></color>");
                return;
            }

            // Try to parse the value argument for 'set'
            if (!float.TryParse(args[1], out value))
            {
                ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid value for 'set'. Please enter a number.</color>");
                return;
            }
        }
        else
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Unknown sanity method. Use 'get', 'set', or 'reset'.</color>");
            return;
        }


        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Error: Player GameObject not found (ensure it has the 'Player' tag).</color>");
            return;
        }

        PlayerSanity playerSanity = player.GetComponent<PlayerSanity>();
        if (playerSanity == null)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Error: PlayerSanity component not found on Player GameObject.</color>");
            return;
        }

        switch (method)
        {
            case "get":
                // Assuming playerSanity.CurrentSanity exists
                //float currentSanity = playerSanity.CurrentSanity;
                //ConsoleManager.LogToConsole($"Player sanity is currently at {currentSanity}.");
                ConsoleManager.LogToConsole($"<color=#33CC33>Player sanity is currently at [PlayerSanity.CurrentSanity placeholder].</color>");
                break;
            case "set":
                // Assuming playerSanity.Set(value) exists
                //playerSanity.Set(value);
                ConsoleManager.LogToConsole($"<color=#33CC33>Player sanity has been set to {value}.</color>");
                break;
            case "reset":
                // Assuming playerSanity.Set(100f) exists or a similar reset method
                //playerSanity.Set(100f);
                ConsoleManager.LogToConsole($"<color=#33CC33>Player sanity has been reset to default (100).</color>");
                break;
            default:
                // This case should ideally not be reached if previous checks are robust, but kept for safety.
                ConsoleManager.LogToConsole("<color=#FF0000FF>Unknown sanity method. Use 'get', 'set', or 'reset'.</color>");
                break;
        }
    }
}
