using UnityEngine;

namespace Console.Commands
{
    public class TimeCommand : BaseConsole
    {
        public override string CommandWord => "time";
        public override string Description => "Modifies the scene's timeScale.";
        protected override string RawUsage => "time <get|set> [value]";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string method = args[0].ToLower();
            float value = 0f;

            if (method == "get")
            {
                ConsoleManager.LogToConsole($"Current timeScale: {Time.timeScale}".AsInfo());
            }
            else if (method == "set")
            {
                if (args.Length < 2)
                {
                    ConsoleManager.LogToConsole("Usage: time set <value>".AsError());
                    return;
                }

                if (!float.TryParse(args[1], out value))
                {
                    ConsoleManager.LogToConsole("Invalid value for 'set'. Please enter a number.".AsError());
                    return;
                }

                // Set the timeScale
                Time.timeScale = value;
                ConsoleManager.LogToConsole($"TimeScale set to {value}.".AsSuccess());
            }
            else
            {
                ConsoleManager.LogToConsole("Unknown time method. Use 'get' or 'set'.".AsError());
                return;
            }
        }
    }
}
