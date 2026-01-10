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
                    ConsoleManager.LogToConsole($"Description: {command.Description}".AsInfo());
                    ConsoleManager.LogToConsole($"{command.Usage}".AsInfo());
                }
                else
                {
                    ConsoleManager.LogToConsole($"Unknown command '{commandName}'".AsError());
                }
                return;
            }
            ConsoleManager.LogToConsole("--- Available Commands ---".AsHeader());
            foreach (var command in ConsoleManager.Instance.GetCommands().Values.OrderBy(c => c.CommandWord))
            {
                string commandName = command.CommandWord.AsInfo();

                // Add tag for modded commands
                if (command is LuaConsoleCommand)
                {
                    commandName += " <size=60%>(Modded)</size>".AsWarning();
                }

                ConsoleManager.LogToConsole($"<b>{commandName}</b>: {command.Description}");
            }
            ConsoleManager.LogToConsole("--------------------------".AsHeader());
        }
    }
}