using System.Linq;

namespace Console.Commands
{
    public class HelpCommand : BaseConsole
    {
        public override string CommandWord => "help";
        public override string Description => "Lists all available commands or shows usage for a specific command.";
        protected override string RawUsage => "help [optional: command]";

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                string commandName = args[0].ToLower();
                if (ConsoleManager.Instance.GetCommands().TryGetValue(commandName, out var command))
                {
                    ConsoleManager.LogToConsole($"<color=#ADD8E6FF>Description: {command.Description}</color>");
                    ConsoleManager.LogToConsole($"<color=#ADD8E6FF>{command.Usage}</color>");
                }
                else
                {
                    ConsoleManager.LogToConsole($"<color=#FF0000FF>Unknown command '{commandName}'</color>");
                }
                return;
            }

            ConsoleManager.LogToConsole("<color=#00FFFFFF>--- Available Commands ---</color>");

            foreach (var command in ConsoleManager.Instance.GetCommands().Values.OrderBy(c => c.CommandWord))
            {
                ConsoleManager.LogToConsole($"<b>{command.CommandWord}</b>: {command.Description}");
            }

            ConsoleManager.LogToConsole("<color=#00FFFFFF>--------------------------</color>");
        }
    }
}