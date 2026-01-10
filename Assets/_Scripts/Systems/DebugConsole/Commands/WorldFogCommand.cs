using UnityEngine;
namespace Console.Commands
{
    public class WorldFogCommand : BaseConsole
    {
        public override string CommandWord => "worldfog";
        public override string Description => "Controls fog visibility and density settings.";
        protected override string RawUsage => "worldfog <toggle|density> [amount|reset]";

        public static float defaultFogDensity;
        public static void SetDefaultFogDensity() => defaultFogDensity = RenderSettings.fogDensity;

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            string action = args[0].ToLower();

            switch (action)
            {
                case "toggle":

                    if (args.Length > 1)
                    {
                        ConsoleManager.LogToConsole(Usage.AsError());
                        return;
                    }
                    RenderSettings.fog = !RenderSettings.fog;
                    string status = RenderSettings.fog ? "enabled" : "disabled";
                    ConsoleManager.LogToConsole($"Fog has been {status}.".AsSuccess());

                    break;

                case "density":

                    if (args.Length < 2)
                    {
                        ConsoleManager.LogToConsole(Usage.AsError());
                        return;
                    }

                    if (args[1].ToLower() == "reset")
                    {
                        RenderSettings.fogDensity = defaultFogDensity;
                        ConsoleManager.LogToConsole($"Fog density reset to default ({defaultFogDensity}).".AsSuccess());
                    }
                    else if (float.TryParse(args[1], out float density))
                    {
                        if (density < 0)
                        {
                            ConsoleManager.LogToConsole("Invalid density value. Must be 0 or greater.".AsError());
                            return;
                        }

                        RenderSettings.fogDensity = density;
                        ConsoleManager.LogToConsole($"Fog density set to {density}.".AsSuccess());
                    }
                    else
                    {
                        ConsoleManager.LogToConsole("Invalid value. Please provide a valid number or 'reset'.".AsError());
                    }

                    break;

                default:
                    ConsoleManager.LogToConsole($"Invalid action. Use 'toggle' or 'density'.".AsError());
                    break;
            }
        }
    }
}