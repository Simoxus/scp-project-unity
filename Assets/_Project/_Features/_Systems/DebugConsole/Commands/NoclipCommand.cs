namespace Console.Commands
{
    public class NoclipCommand : BaseConsole
    {
        public override string CommandWord => "noclip";
        public override string Description => "Toggles noclip mode for the player.";
        protected override string RawUsage => "noclip";

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            Player player = Core.Player;
            if (player == null) return;

            if (player.Controller.IsNoclipping)
            {
                player.Controller.DisableNoclip();
                ConsoleManager.LogToConsole($"Noclip has been disabled.".AsSuccess());
            }
            else
            {
                player.Controller.EnableNoclip();
                ConsoleManager.LogToConsole($"Noclip has been enabled.".AsSuccess());
            }
        }
    }
}