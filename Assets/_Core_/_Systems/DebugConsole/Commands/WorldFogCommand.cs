namespace Console.Commands
{
    public class WorldFogCommand : BaseConsole
    {
        public override string CommandWord => "worldfog";
        public override string Description => "Controls fog visibility and density settings.";
        public override string[] Aliases => new string[] { "camerafog" };
        protected override string RawUsage => "worldfog <density[value]|reset>";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                bool currentState = Core.FacilityManager.GetFogEnabled();
                Core.FacilityManager.SetFogEnabled(!currentState, 0f);
                string status = !currentState ? "enabled" : "disabled";
                ConsoleManager.LogToConsole($"Fog has been {status}.".AsSuccess());
                return;
            }

            string action = args[0].ToLower();

            switch (action)
            {
                case "density":
                    if (args.Length < 2)
                    {
                        ConsoleManager.LogToConsole(Usage.AsError());
                        return;
                    }

                    if (args[1].ToLower() == "reset")
                    {
                        float defaultDensity = Core.FacilityManager.GetDefaultFogDensity();
                        Core.FacilityManager.SetFogDensity(defaultDensity, 0f);
                        ConsoleManager.LogToConsole($"Fog density reset to default ({defaultDensity}).".AsSuccess());
                    }
                    else if (float.TryParse(args[1], out float density))
                    {
                        if (density < 0)
                        {
                            ConsoleManager.LogToConsole("Invalid density value. Must be 0 or greater.".AsError());
                            return;
                        }
                        Core.FacilityManager.SetFogDensity(density, 0f);
                        ConsoleManager.LogToConsole($"Fog density set to {density}.".AsSuccess());
                    }
                    else
                    {
                        ConsoleManager.LogToConsole(Usage.AsError());
                    }
                    break;

                case "clear":
                    Core.FacilityManager.ClearFogQueue();
                    ConsoleManager.LogToConsole("Fog queue cleared.".AsSuccess());
                    break;

                default:
                    ConsoleManager.LogToConsole(Usage.AsError());
                    break;
            }
        }
    }
}