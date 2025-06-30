using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Assuming these interfaces/classes exist in your project
// public interface IStatusEffect { string Name { get; } string EffectID { get; } GameObject Target { get; } }
// public class PlayerEffects : MonoBehaviour { public List<IStatusEffect> GetActiveEffects() { return new List<IStatusEffect>(); } public void ApplyEffect(string effectId, object[] args) { } public bool RemoveEffect(string effectId) { return false; } public void ClearAllEffects() { } }
// public class BurnEffect : IStatusEffect { public BurnEffect(float duration, float damagePerTick, float tickInterval) { } public string Name => "Burn"; public string EffectID => "BurnEffect"; public GameObject Target => null; }
// public class ShieldEffect : IStatusEffect { public ShieldEffect(float damageReductionPercentage) { } public string Name => "Shield"; public string EffectID => "ShieldEffect"; public GameObject Target => null; }
// public class DebugLogEffect : IStatusEffect { public DebugLogEffect(string message) { } public string Name => "Debug Log"; public string EffectID => "DebugLogEffect"; public GameObject Target => null; }


public class EffectCommand : ConsoleBase
{
    public override string CommandWord => "effect";
    public override string Description => "Manages player status effects. Usage: effect <list|add|remove|clearall> [effectName] [args]";

    // Define known effect IDs for autocompletion.
    // In a real project, you might get this list dynamically from a registry of available effects.
    private static readonly List<string> _availableEffectIDs = new List<string>
    {
        "BurnEffect",
        "ShieldEffect",
        "DebugLogEffect"
    };

    private void HandleListCommand(PlayerEffects playerEffectsManager)
    {
        ConsoleManager.LogToConsole("<color=lime>--- Active Effects ---</color>");
        List<IStatusEffect> activeEffects = playerEffectsManager.GetActiveEffects();
        if (activeEffects.Count == 0)
        {
            ConsoleManager.LogToConsole("No active effects.");
        }
        else
        {
            foreach (var effect in activeEffects)
            {
                // You might want to extend IStatusEffect with a 'GetStatusString()' for more detailed info
                string effectInfo = effect.Name;
                ConsoleManager.LogToConsole($"- {effectInfo} (ID: {effect.EffectID}) on {effect.Target.name}");
            }
        }

        ConsoleManager.LogToConsole("<b>--- Available Effect Types (by ID) ---</b>");
        foreach (var id in _availableEffectIDs.OrderBy(id => id))
        {
            ConsoleManager.LogToConsole($"- {id}");
        }
        ConsoleManager.LogToConsole("<i>Note: Arguments for 'add' command must match the effect's constructor types and order.</i>");
    }

    private void HandleAddCommand(PlayerEffects playerEffectsManager, string[] args)
    {
        if (args.Length < 1)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: effect add <effectID> [args]</color>");
            ConsoleManager.LogToConsole("<color=red>Example: effect add BurnEffect 5 10 1</color>");
            ConsoleManager.LogToConsole("<color=red>Example: effect add DebugLogEffect \"Hello World\"</color>");
            return;
        }

        string effectID = args[0]; // Use EffectID directly for command
        object[] constructorArgs = null; // Arguments to pass to the effect's constructor

        // Parse arguments based on the effectID
        switch (effectID)
        {
            case "BurnEffect":
                if (args.Length < 4 || !float.TryParse(args[1], out float burnDuration) ||
                    !float.TryParse(args[2], out float burnDamagePerTick) ||
                    !float.TryParse(args[3], out float burnTickInterval))
                {
                    ConsoleManager.LogToConsole("<color=red>Usage: effect add BurnEffect &lt;duration&gt; &lt;damagePerTick&gt; &lt;tickInterval&gt;</color>");
                    return;
                }
                constructorArgs = new object[] { burnDuration, burnDamagePerTick, burnTickInterval };
                break;

            case "ShieldEffect":
                if (args.Length < 2 || !float.TryParse(args[1], out float shieldReduction))
                {
                    ConsoleManager.LogToConsole("<color=red>Usage: effect add ShieldEffect &lt;damageReductionPercentage&gt;</color>");
                    return;
                }
                constructorArgs = new object[] { shieldReduction };
                break;

            case "DebugLogEffect":
                // For DebugLogEffect, the rest of the arguments are the message
                if (args.Length < 2)
                {
                    ConsoleManager.LogToConsole("<color=red>Usage: effect add DebugLogEffect &lt;message&gt;</color>");
                    return;
                }
                // Join the rest of the arguments to form the message string
                string debugMessage = string.Join(" ", args.Skip(1).ToArray());
                constructorArgs = new object[] { debugMessage };
                break;

            default:
                ConsoleManager.LogToConsole($"<color=red>Unknown effect ID: '{effectID}'. See 'effect list' for available types.</color>");
                return;
        }

        // Attempt to apply the effect using the PlayerEffects manager
        playerEffectsManager.ApplyEffect(effectID, constructorArgs);
    }

    /// <summary>
    /// Handles the 'remove' sub-command.
    /// </summary>
    private void HandleRemoveCommand(PlayerEffects playerEffectsManager, string[] args)
    {
        if (args.Length < 1)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: effect remove &lt;effectID&gt;</color>");
            return;
        }

        string effectIDToRemove = args[0];
        if (playerEffectsManager.RemoveEffect(effectIDToRemove))
        {
            ConsoleManager.LogToConsole($"Successfully removed '{effectIDToRemove}' from player.");
        }
        else
        {
            ConsoleManager.LogToConsole($"Could not find active effect '{effectIDToRemove}' on player to remove.");
        }
    }

    public override void Execute(string[] args)
    {
        if (args.Length < 1)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: effect <list|add|remove|clearall> [effectName] [args]</color>");
            return;
        }

        string subCommand = args[0].ToLower();

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            ConsoleManager.LogToConsole("<color=red>Error: Player GameObject not found (ensure it has the 'Player' tag).</color>");
            return;
        }

        PlayerEffects playerEffectsManager = player.GetComponent<PlayerEffects>();
        if (playerEffectsManager == null)
        {
            ConsoleManager.LogToConsole("<color=red>Error: PlayerEffects component not found on Player GameObject.</color>");
            return;
        }

        switch (subCommand)
        {
            case "list":
                HandleListCommand(playerEffectsManager);
                break;
            case "add":
                HandleAddCommand(playerEffectsManager, args.Skip(1).ToArray());
                break;
            case "remove":
                HandleRemoveCommand(playerEffectsManager, args.Skip(1).ToArray());
                break;
            case "clearall": // New command to clear all effects
                playerEffectsManager.ClearAllEffects();
                ConsoleManager.LogToConsole("<color=yellow>All effects cleared from player.</color>");
                break;
            default:
                ConsoleManager.LogToConsole("<color=red>Unknown effect sub-command. Use 'list', 'add', 'remove', or 'clearall'.</color>");
                break;
        }
    }
}
