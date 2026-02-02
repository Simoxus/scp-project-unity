namespace Console.Commands
{
    public class RoomListCommand : BaseConsole
    {
        public override string CommandWord => "roomlist";
        public override string Description => "Lists all registered rooms with their names and IDs. ";
        public override string[] Aliases => new string[] { "rooms", "listrooms" };
        protected override string RawUsage => "roomlist";

        public override void Execute(string[] args)
        {
            if (Core.FacilityManager == null) return;

            var rooms = Core.FacilityManager.GetAllRooms();

            if (rooms.Count == 0)
            {
                ConsoleManager.LogToConsole("No rooms have been registered.".AsWarning());
                return;
            }

            ConsoleManager.LogToConsole($"--- Room List ({rooms.Count} rooms registered) ---".AsHeader());

            foreach (var room in rooms)
            {
                if (room == null || room.RoomData == null) continue;

                string gridCoordinates = $"<size=80%>[{room.GridCoordinate.x},{room.GridCoordinate.y}]</size>";
                string roomName = $"<b>{room.RoomData.RoomName}</b>".AsInfo();
                string roomID = $"(ID: {room.RoomData.RoomID})".AsInfo();

                ConsoleManager.LogToConsole($"{gridCoordinates} {roomName} {roomID}");
            }

            ConsoleManager.LogToConsole("--------------------------".AsHeader());
        }
    }
}