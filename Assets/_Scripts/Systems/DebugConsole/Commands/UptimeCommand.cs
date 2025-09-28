using UnityEngine;

namespace Console.Commands
{
    public class UptimeCommand : ConsoleBase
    {
        public override string CommandWord => "uptime";
        public override string Description => "Displays how long the game has been running.";
        protected override string RawUsage => "uptime";

        public override void Execute(string[] args)
        {
            float seconds = Time.realtimeSinceStartup;
            System.TimeSpan span = System.TimeSpan.FromSeconds(seconds);
            ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Uptime: {span:hh\\:mm\\:ss}</color>");
        }
    }
}
