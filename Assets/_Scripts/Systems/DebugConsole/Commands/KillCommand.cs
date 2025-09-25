using System.Collections.Generic; // Required for IEnumerable
using System.Linq; // Required for Enumerable.Empty
using UnityEngine;

public class KillCommand : ConsoleBase
{
    public override string CommandWord => "kill";
    public override string Description => "Kills the player.";

    public override void Execute(string[] args)
    {
        if (args.Length > 0)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Usage: kill (no arguments)</color>");
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

        if (playerHealth.GetHealth() <= 0) // Changed to <= 0 for robustness
        {
            ConsoleManager.LogToConsole($"Player is already dead.");
            return;
        }

        playerHealth.Set(0f);
        ConsoleManager.LogToConsole($"Player has been killed.");
    }
}
