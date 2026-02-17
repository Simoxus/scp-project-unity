using Facility.Generation;
using UnityEngine;

namespace Console.Commands
{
    public class TeleportCommand : BaseConsole
    {
        public override string CommandWord => "teleport";
        public override string Description => "Teleports the player to a room using name or ID.";
        public override string[] Aliases => new string[] { "tp" };
        protected override string RawUsage => "teleport <roomID|roomName>";

        public override void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ConsoleManager.LogToConsole(Usage.AsError());
                return;
            }

            if (Core.FacilityManager == null) return;

            string searchTerm = string.Join(" ", args);
            RoomInstance targetRoom = Core.FacilityManager.FindRoom(searchTerm);

            if (targetRoom == null)
            {
                ConsoleManager.LogToConsole($"'{searchTerm}' not found.".AsError());
                return;
            }

            // Teleport the player
            TeleportPlayer(targetRoom);

            if (Core.CullingSystem != null)
            {
                Core.CullingSystem.ForceUpdate();
            }

            ConsoleManager.LogToConsole($"Teleported to room '{targetRoom.RoomData.RoomName}' (ID: {targetRoom.RoomData.RoomID})".AsSuccess());
        }

        private void TeleportPlayer(RoomInstance room)
        {
            if (Core.Player.Controller == null) return;

            bool wasNoclipping = Core.Player.Controller.IsNoclipping;
            if (wasNoclipping)
            {
                Core.Player.Controller.DisableNoclip();
            }

            SpawnPoint playerSpawn = room.GetSpawnPoint(SpawnType.Player);
            Vector3 teleportPosition;

            if (playerSpawn != null)
            {
                teleportPosition = playerSpawn.Position;
            }
            else
            {
                Bounds roomBounds = room.GetRoomBounds();
                teleportPosition = roomBounds.center;
                teleportPosition.y = roomBounds.min.y + 2f;
            }

            if (wasNoclipping)
            {
                Core.Player.Controller.EnableNoclip();
            }

            if (Core.Player.CharacterController != null)
            {
                Core.Player.CharacterController.enabled = false;
                Core.Player.Controller.transform.position = teleportPosition;
                Core.Player.CharacterController.enabled = true;
            }

            room.EnterRoom();
        }
    }
}