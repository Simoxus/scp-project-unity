using System.Linq;

namespace Console.Commands
{
    public class LogCommand : BaseConsole
    {
        public override string CommandWord => "log";
        public override string Description => "Logs a custom message to the console with a specified type.";
        protected override string RawUsage => "log <type (error|warning|log|info)> <message>";

        public override void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string logType = args[0].ToLower();
            string message = string.Join(" ", args.Skip(1).ToArray());

            switch (logType)
            {
                case "error":
                    Log.Error($"[Forced Error]: {message}");
                    break;
                case "warning":
                    Log.Warning($"[Forced Warning]: {message}");
                    break;
                case "log":
                    Log.Info($"[Forced Log]: {message}");
                    break;
                case "info":
                    Log.Info($"[Forced Info]: {message}");
                    break;
                default:
                    ConsoleManager.LogToConsole(Usage.AsError());
                    break;
            }
        }
    }
}
