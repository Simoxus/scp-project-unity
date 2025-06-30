using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for .Where, .OrderBy, etc.
using UnityEngine;

public class HealthCommand : ConsoleBase
{
    public override string CommandWord => "health";
    public override string Description => "Modifies the player's health. Usage: health <method (set|heal|damage)> <value>";

    public override void Execute(string[] args)
    {
        if (args.Length != 2)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: health <method (set|heal|damage)> <value></color>");
            return;
        }

        string method = args[0].ToLower();
        float value;

        if (!float.TryParse(args[1], out value))
        {
            ConsoleManager.LogToConsole("<color=red>Invalid value. Please enter a number.</color>");
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            ConsoleManager.LogToConsole("<color=red>Error: Player GameObject not found (ensure it has the 'Player' tag).</color>");
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            ConsoleManager.LogToConsole("<color=red>Error: PlayerHealth component not found on Player GameObject.</color>");
            return;
        }

        switch (method)
        {
            case "set":
                playerHealth.Set(value);
                ConsoleManager.LogToConsole($"Player health set to {value}.");
                break;
            case "heal":
                playerHealth.Heal(value);
                ConsoleManager.LogToConsole($"Player healed by {value}.");
                break;
            case "damage":
                playerHealth.Take(value);
                ConsoleManager.LogToConsole($"Player took {value} damage.");
                break;
            default:
                ConsoleManager.LogToConsole("<color=red>Unknown health method. Use 'set', 'heal', or 'damage'.</color>");
                break;
        }
    }
}
