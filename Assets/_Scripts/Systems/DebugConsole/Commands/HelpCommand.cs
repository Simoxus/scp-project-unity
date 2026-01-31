using System.Linq;

namespace Console.Commands
{
    public class HelpCommand : BaseConsole
    {
        public override string CommandWord => "help";
        public override string Description => "Lists all available commands or shows usage for a specific command.";
        protected override string RawUsage => "help [command]";

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                string commandName = args[0].ToLower();
                if (Core.ConsoleManager.GetCommands().TryGetValue(commandName, out var command))
                {
                    ConsoleManager.LogToConsole($"Description: {command.Description}".AsInfo());
                    ConsoleManager.LogToConsole($"{command.Usage}".AsInfo());
                }
                else
                {
                    ConsoleManager.LogToConsole($"Unknown command '{commandName}'".AsError());
                }
                return;
            }

            var distinctCommands = Core.ConsoleManager.GetCommands().Values
                .Distinct()
                .OrderBy(c => c.CommandWord)
                .ToList();

            ConsoleManager.LogToConsole($"--- Available Commands ({distinctCommands.Count} commands) ---".AsHeader());

            foreach (var command in distinctCommands)
            {
                string commandName = command.CommandWord.AsInfo();
                if (command is LuaConsoleCommand)
                {
                    commandName += " <size=70%>(Modded)</size>".AsWarning();
                }
                ConsoleManager.LogToConsole($"<b>{commandName}</b>: {command.Description}");
            }

            ConsoleManager.LogToConsole("--------------------------".AsHeader());
        }
    }
}