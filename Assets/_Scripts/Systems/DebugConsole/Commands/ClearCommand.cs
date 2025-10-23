namespace Console.Commands
{
    public class ClearCommand : BaseConsole
    {
        public override string CommandWord => "clear";
        public override string Description => "Clears the console output.";
        protected override string RawUsage => "clear";

        public override void Execute(string[] args)
        {
            ConsoleManager.LogToConsole("<CMD_CLEAR_CONSOLE>");
        }
    }
}