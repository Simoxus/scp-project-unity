using UnityEngine;

namespace Console.Commands
{
    public class LocaleCommand : ConsoleBase
    {
        public override string CommandWord => "locale";
        public override string Description => "Displays the system language being used.";
        protected override string RawUsage => "locale";

        public override void Execute(string[] args)
        {
            ConsoleManager.LogToConsole($"<color=#ADD8E6FF>System Language: {Application.systemLanguage}");
        }
    }
}