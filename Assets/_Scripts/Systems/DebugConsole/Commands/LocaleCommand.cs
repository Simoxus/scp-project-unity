namespace Console.Commands
{
    public class LocaleCommand : BaseConsole
    {
        public override string CommandWord => "locale";
        public override string Description => "Displays the game language currently being used.";
        protected override string RawUsage => "locale";

        public override void Execute(string[] args)
        {
            ConsoleManager.LogToConsole($"Game Language: {LocalizationHelper.GetCurrentLanguage()}".AsSuccess());
        }
    }
}