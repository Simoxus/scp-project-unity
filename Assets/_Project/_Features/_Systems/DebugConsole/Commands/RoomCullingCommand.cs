namespace Console.Commands
{
    public class RoomCullingCommand : BaseConsole
    {
        public override string CommandWord => "roomculling";
        public override string Description => "Toggles the room culling system.";
        public override string[] Aliases => new string[] { "culling" };
        protected override string RawUsage => "roomculling";

        public override void Execute(string[] args)
        {
            if (Core.CullingSystem == null) return;
            if (Core.FacilityGenerator == null || !Core.FacilityGenerator.IsGenerated)
            {
                ConsoleManager.LogToConsole("The facility hasn't been generated yet.".AsError());
                return;
            }

            // Toggle culling state
            if (Core.CullingSystem.IsActive)
            {
                Core.CullingSystem.IsActive = false;
                Core.CullingSystem.ShowAllRooms();
                ConsoleManager.LogToConsole($"Facility culling has been disabled. All {Core.FacilityManager.GetAllRooms().Count} rooms are now visible.".AsSuccess());
            }
            else
            {
                Core.CullingSystem.IsActive = true;
                Core.CullingSystem.ForceUpdate();
                ConsoleManager.LogToConsole($"Facility culling has been enabled.".AsSuccess());
            }
        }
    }
}