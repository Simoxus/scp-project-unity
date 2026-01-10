namespace Console.Commands
{
    public class NoclipCommand : BaseConsole
    {
        public override string CommandWord => "noclip";
        public override string Description => "Toggles noclip mode (fly through walls).";
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

            player.PlayerController.ToggleNoclip();

            string status = player.IsInState(PlayerState.Noclip) ? "enabled" : "disabled";
            ConsoleManager.LogToConsole($"Noclip has been {status}.".AsSuccess());
        }
    }
}