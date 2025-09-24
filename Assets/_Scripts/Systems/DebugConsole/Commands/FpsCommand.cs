using System;
using UnityEngine;

public class FpsCommand : ConsoleBase
{
    public override string CommandWord => "fps";
    public override string Description => "Sets the target frame rate. Usage: fps <rate>";

    public override void Execute(string[] args)
    {
        if (args.Length < 1)
        {
            ConsoleManager.LogToConsole("<color=red>Usage: fps <rate></color>");
            return;
        }

        if (int.TryParse(args[0], out int targetFPS))
        {
            if (targetFPS < -1)
            {
                ConsoleManager.LogToConsole("<color=red>Invalid FPS value. Must be -1 or greater.</color>");
                return;
            }

            Application.targetFrameRate = targetFPS;

            if (targetFPS == -1)
            {
                ConsoleManager.LogToConsole($"<color=lime>Frame rate has been unlocked.</color>");
            }
            else
            {
                ConsoleManager.LogToConsole($"<color=lime>Frame rate set to {targetFPS} FPS.</color>");
            }
        }
        else
        {
            ConsoleManager.LogToConsole("<color=red>Invalid integer. Please provide a valid number.</color>");
        }
    }
}