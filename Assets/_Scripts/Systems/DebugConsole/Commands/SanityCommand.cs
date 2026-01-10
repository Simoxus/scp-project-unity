namespace Console.Commands
{
    public class SanityCommand : BaseConsole
    {
        public override string CommandWord => "sanity";
        public override string Description => "Modifies the player's sanity.";
        protected override string RawUsage => "sanity <get|set|reset> [value]";

        public override void Execute(string[] args)
        {
            ConsoleManager.LogToConsole($"This command is currently being rewritten and will be temporarily unavailable.".AsInfo());
        }
    }
}
