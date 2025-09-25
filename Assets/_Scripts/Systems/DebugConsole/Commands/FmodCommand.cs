using FMODUnity;
using UnityEngine;

public class FmodCommand : ConsoleBase
{
    public override string CommandWord => "fmod";
    public override string Description => "Debug FMOD events. Usage: fmod <play|stop> <eventName>";

    public override void Execute(string[] args)
    {
        if (args.Length != 2)
        {
            ConsoleManager.LogToConsole("<color=#FF0000FF>Usage: fmod <play|stop> <eventName></color>");
            return; 
        }

        string action = args[0].ToLowerInvariant();
        string eventName = args[1];

        // Use a switch statement to handle different actions.
        switch (action)
        {
            case "play":
                RuntimeManager.PlayOneShot($":/{eventName}");
                ConsoleManager.LogToConsole($"<color=#33CC33>Attempted to play FMOD event '{eventName}'</color>");
                break;

            case "stop":
                ConsoleManager.LogToConsole($"<color=#33CC33>Attempted to stop FMOD event '{eventName}'</color>");
                break;

            default:
                // If the action is not recognized, log an error.
                ConsoleManager.LogToConsole($"<color=#FF0000FF>Unknown FMOD method '{action}'. Use 'play' or 'stop'.</color>");
                break;
        }
    }
}
