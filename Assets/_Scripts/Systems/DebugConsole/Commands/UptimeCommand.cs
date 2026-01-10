using UnityEngine;

namespace Console.Commands
{
    public class UptimeCommand : BaseConsole
    {
        public override string CommandWord => "uptime";
        public override string Description => "Displays how long the game has been running.";
        protected override string RawUsage => "uptime";

        public override void Execute(string[] args)
        {
            float seconds = Time.realtimeSinceStartup;
            System.TimeSpan span = System.TimeSpan.FromSeconds(seconds);
            ConsoleManager.LogToConsole($"Uptime: {span:hh\\:mm\\:ss}".AsInfo());
        }
    }
}
