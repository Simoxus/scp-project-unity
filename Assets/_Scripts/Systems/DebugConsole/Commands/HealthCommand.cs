using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for .Where, .OrderBy, etc.
using UnityEngine;

public class HealthCommand : ConsoleBase
{
    public override string CommandWord => "health";
    public override string Description => "Modifies the player's health. Usage: health <set|heal|damage> <value>";

    public override void Execute(string[] args)
    {
        if (args.Length != 2)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Usage: health <method (set|heal|damage)> <value></color>");
            return;
        }

        string method = args[0].ToLower();
        float value;

        if (!float.TryParse(args[1], out value))
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid value. Please enter a number.</color>");
            return;
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Error: Player GameObject not found (ensure it has the 'Player' tag).</color>");
            return;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Error: PlayerHealth component not found on Player GameObject.</color>");
            return;
        }

        switch (method)
        {
            case "set":
                playerHealth.Set(value);
                ConsoleManager.LogToConsole($"<color=#33CC33>Player health set to {value}.</color>");
                break;
            case "heal":
                playerHealth.Heal(value);
                ConsoleManager.LogToConsole($"<color=#33CC33>Player healed by {value}.</color>");
                break;
            case "damage":
                playerHealth.Take(value);
                ConsoleManager.LogToConsole($"<color=#33CC33>Player took {value} damage.</color>");
                break;
            default:
                ConsoleManager.LogToConsole("<color=#FF0000FF>Unknown health method. Use 'set', 'heal', or 'damage'.</color>");
                break;
        }
    }
}
