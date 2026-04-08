using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Facility.Generation
{
    public class FG_PostProcessor
    {
        private readonly FacilityGeneratorSettings _settings;

        public FG_PostProcessor(FacilityGeneratorSettings settings)
        {
            _settings = settings;
        }

        public async UniTask RunAsync(GridCell startRoomCell)
        {
            TeleportPlayerToStartRoom(startRoomCell);
            await UniTask.Yield();
        }

        private void TeleportPlayerToStartRoom(GridCell startRoomCell)
        {
            if (startRoomCell == null) return;
            if (Core.Player == null) return;
            if (Core.FacilityManager == null) return;

            RoomInstance startRoom = Core.FacilityManager.FindRoomAtGridPosition(startRoomCell.position);
            if (startRoom == null) return;

            TeleportPlayer(startRoom);

            if (Core.CullingSystem != null)
            {
                Core.CullingSystem.ForceUpdate();
            }

            Log.VerboseSuccess($"Teleported player to start room '{startRoom.RoomData.RoomName}'");
        }

        private void TeleportPlayer(RoomInstance room)
        {
            if (Core.Player.Controller == null) return;

            bool wasNoclipping = Core.Player.Controller.IsNoclipping;
            if (wasNoclipping) Core.Player.Controller.DisableNoclip();

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

            if (wasNoclipping) Core.Player.Controller.EnableNoclip();

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