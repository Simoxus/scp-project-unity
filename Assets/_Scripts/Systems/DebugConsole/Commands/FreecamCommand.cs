namespace Console.Commands
{
    public class FreecamCommand : BaseConsole
    {
        public override string CommandWord => "freecam";
        public override string Description => "Detaches the camera from the player.";
        protected override string RawUsage => "freecam";

        public override void Execute(string[] args)
        {
            bool isActive = Core.Player.Freecam.ToggleFreecam();

            string status = isActive ? "enabled" : "disabled";
            ConsoleManager.LogToConsole($"Freecam has been {status}.".AsSuccess());
        }
    }
}