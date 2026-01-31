namespace Console.Commands
{
    public class HealthCommand : BaseConsole
    {
        public override string CommandWord => "health";
        public override string Description => "Modifies the player's health.";
        protected override string RawUsage => "health <set|heal|damage> <value>";

        public override void Execute(string[] args)
        {
            if (args.Length != 2)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string method = args[0].ToLower();
            float value;

            if (!float.TryParse(args[1], out value))
            {
                ConsoleManager.LogToConsole("Invalid value. Please enter a number.".AsError());
                return;
            }

            PlayerHealth playerHealth = Core.Player.Health;
            switch (method)
            {
                case "set":
                    playerHealth.Set(value);
                    ConsoleManager.LogToConsole($"Player health set to {value}.".AsSuccess());
                    break;
                case "heal":
                    playerHealth.Heal(value);
                    ConsoleManager.LogToConsole($"Player healed by {value}.".AsSuccess());
                    break;
                case "damage":
                    playerHealth.Take(value);
                    ConsoleManager.LogToConsole($"Player took {value} damage.".AsSuccess());
                    break;
                default:
                    ConsoleManager.LogToConsole("Unknown health method. Use 'set', 'heal', or 'damage'.".AsError());
                    break;
            }
        }
    }
}
