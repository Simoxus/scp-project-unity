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
            ConsoleManager.LogToConsole("<color=red>Usage: fmod <play|stop> <eventName></color>");
            return; 
        }

        string action = args[0].ToLowerInvariant();
        string eventName = args[1];

        // Use a switch statement to handle different actions.
        switch (action)
        {
            case "play":
                RuntimeManager.PlayOneShot($":/{eventName}");
                ConsoleManager.LogToConsole($"Attempted to play FMOD event {eventName}");
                break;

            case "stop":
                ConsoleManager.LogToConsole($"Attempted to stop FMOD event {eventName}");
                break;

            default:
                // If the action is not recognized, log an error.
                ConsoleManager.LogToConsole($"<color=red>Unknown FMOD method '{action}'. Use 'play' or 'stop'.</color>");
                break;
        }
    }
}
