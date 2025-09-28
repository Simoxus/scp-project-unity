using UnityEngine;

namespace Console.Commands
{
    public class FpsCapCommand : ConsoleBase
    {
        public override string CommandWord => "fpscap";
        public override string Description => "Sets the target frame rate limit.";
        protected override string RawUsage => "fpscap <rate>";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole($"<color=#FF0000FF>{Usage}</color>");
                return;
            }

            if (int.TryParse(args[0], out int targetFPS))
            {
                if (targetFPS < -1)
                {
                    ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid FPS value was provided. Must be -1 or greater.</color>");
                    return;
                }

                Application.targetFrameRate = targetFPS;

                if (targetFPS == -1)
                {
                    ConsoleManager.LogToConsole($"<color=#33CC33>Frame rate limit has been unlocked.</color>");
                }
                else
                {
                    ConsoleManager.LogToConsole($"<color=#33CC33>Frame rate limit set to {targetFPS} FPS.</color>");
                }
            }
            else
            {
                ConsoleManager.LogToConsole("<color=#FF0000FF>Invalid integer. Please provide a valid number.</color>");
            }
        }
    }
}