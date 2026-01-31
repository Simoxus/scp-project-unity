namespace Console.Commands
{
    public class KillCommand : BaseConsole
    {
        public override string CommandWord => "kill";
        public override string Description => "Kills the player.";
        protected override string RawUsage => "kill";

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            PlayerHealth playerHealth = Core.Player.Health;
            if (playerHealth.GetHealth() <= 0)
            {
                ConsoleManager.LogToConsole("Player is already dead.".AsSuccess());
                return;
            }

            playerHealth.Set(0f);
            ConsoleManager.LogToConsole("Player has been killed.".AsSuccess());
        }
    }
}
