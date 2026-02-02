namespace Console.Commands
{
    public class FullbrightCommand : BaseConsole
    {
        public override string CommandWord => "fullbright";
        public override string Description => "Toggles fullbright ambient lighting.";
        protected override string RawUsage => "fullbright";

        public override void Execute(string[] args)
        {
            if (Core.FacilityManager == null)
            {
                return;
            }

            bool currentState = Core.FacilityManager.GetFullbrightEnabled();
            Core.FacilityManager.SetFullbright(!currentState, 1.4f);

            string status = currentState ? "disabled" : "enabled";
            ConsoleManager.LogToConsole($"Fullbright has been {status}.".AsSuccess());
        }
    }
}