using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Facility.Generation
{
    public class FG_Instantiator
    {
        private readonly FacilityGeneratorSettings _settings;
        private readonly int _seed;
        private readonly Transform _roomAnchor;
        private readonly Transform _doorAnchor;
        private readonly CullingSystem _cullingSystem;

        private Dictionary<Vector2Int, RoomInstance> _roomInstances;
        private List<GameObject> _doorInstances;

        public FG_Instantiator(
            FacilityGeneratorSettings settings,
            int seed,
            Transform roomAnchor,
            Transform doorAnchor,
            CullingSystem cullingSystem,
            Dictionary<Vector2Int, RoomInstance> roomInstances,
            List<GameObject> doorInstances)
        {
            _settings = settings;
            _seed = seed;
            _roomAnchor = roomAnchor;
            _doorAnchor = doorAnchor;
            _cullingSystem = cullingSystem;
            _roomInstances = roomInstances;
            _doorInstances = doorInstances;
        }

        public async UniTask InstantiateRoomsAsync(List<GridCell> occupiedCells)
        {
            _roomInstances.Clear();
            int instantiatedCount = 0;
            const int BATCH_SIZE = 10;

            for (int i = 0; i < occupiedCells.Count; i += BATCH_SIZE)
            {
                List<UniTask<(Vector2Int pos, RoomInstance instance)>> batchTasks = new List<UniTask<(Vector2Int, RoomInstance)>>();

                for (int j = i; j < Mathf.Min(i + BATCH_SIZE, occupiedCells.Count); j++)
                {
                    var cell = occupiedCells[j];
                    if (cell.isBlocked || cell.assignedRoom == null) continue;

                    Vector3 worldPos = FG_GridUtility.GridToWorldPosition(cell.position, _settings.CellSize);
                    batchTasks.Add(InstantiateRoomWithPositionAsync(cell.assignedRoom, worldPos, cell));
                }

                var batchResults = await UniTask.WhenAll(batchTasks);

                foreach (var (pos, roomInstance) in batchResults)
                {
                    if (roomInstance != null)
                    {
                        _roomInstances[pos] = roomInstance;
                        instantiatedCount++;
                    }
                }
            }

            Log.VerboseSuccess($"Instantiated {instantiatedCount} rooms");
        }

        private async UniTask<(Vector2Int pos, RoomInstance instance)> InstantiateRoomWithPositionAsync(
            RoomData roomData, Vector3 position, GridCell cell)
        {
            var roomInstance = await InstantiateRoomAsync(roomData, position, cell);
            return (cell.position, roomInstance);
        }

        private async UniTask<RoomInstance> InstantiateRoomAsync(RoomData roomData, Vector3 position, GridCell cell)
        {
            if (roomData.RoomPrefabReference == null)
            {
                Log.VerboseWarning($"Room '{roomData.RoomName}' has no prefab reference");
                return null;
            }

            int rotation = cell.rotation;

            Vector3 finalPosition = FG_PositionCalculator.CalculatePosition(position, roomData, rotation, _settings.CellSize);
            float finalRotation = FG_PositionCalculator.CalculateRotation(rotation, roomData);

            AsyncOperationHandle<GameObject> handle = roomData.RoomPrefabReference.InstantiateAsync(
                finalPosition,
                Quaternion.Euler(0, finalRotation, 0),
                _roomAnchor
            );
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject roomObj = handle.Result;
                roomObj.name = $"{roomData.RoomName}_{cell.position.x}_{cell.position.y}";

                RoomInstance roomInstance = roomObj.GetComponent<RoomInstance>();
                if (roomInstance != null)
                {
                    roomInstance.Initialize(roomData, cell.position, 0, _seed, rotation, cell.zone, _cullingSystem);
                    cell.instantiatedRoom = roomInstance;
                }
                else
                {
                    if (roomData != _settings.StartingRoom)
                    {
                        Log.VerboseWarning($"Room prefab '{roomData.RoomName}' is missing a RoomInstance component");
                    }
                }

                return roomInstance;
            }

            Log.Error($"Failed to instantiate room '{roomData.RoomName}'");
            return null;
        }

        public async UniTask CreateDoorsAsync(GridCell[,] grid, List<GridCell> occupiedCells)
        {
            _doorInstances.Clear();

            List<UniTask> doorTasks = new List<UniTask>();
            int doorCount = 0;

            foreach (var cell in occupiedCells)
            {
                if (cell.isBlocked) continue;

                for (int i = 0; i < 4; i++)
                {
                    Direction dir = (Direction)i;
                    if (!cell.HasExit(dir)) continue;

                    Vector2Int neighborPos = FG_GridUtility.GetNeighborPosition(cell.position, dir);
                    if (!FG_GridUtility.IsValidGridPosition(neighborPos, _settings.GridWidth, _settings.GridHeight)) continue;

                    GridCell neighbor = grid[neighborPos.x, neighborPos.y];
                    if (neighbor == null || neighbor.isBlocked) continue;

                    if (FG_GridUtility.ShouldCreateDoor(cell.position, neighborPos))
                    {
                        doorTasks.Add(CreateDoorBetweenCells(cell, neighbor, dir));
                        doorCount++;
                    }
                }
            }

            await UniTask.WhenAll(doorTasks);
            Log.VerboseSuccess($"Created {doorCount} doors");
        }

        private async UniTask CreateDoorBetweenCells(GridCell cell1, GridCell cell2, Direction direction)
        {
            var zoneSettings = _settings.GetZoneSettings(cell1.zone);
            if (zoneSettings?.doorPool == null) return;

            int doorSeed = _seed + cell1.position.x * 10000 + cell1.position.y * 100 + (int)direction;
            var doorReference = zoneSettings.doorPool.GetRandomDoorReference(doorSeed);

            if (doorReference == null) return;

            Vector3 pos1 = FG_GridUtility.GridToWorldPosition(cell1.position, _settings.CellSize);
            Vector3 pos2 = FG_GridUtility.GridToWorldPosition(cell2.position, _settings.CellSize);
            Vector3 doorPosition = (pos1 + pos2) / 2f;
            doorPosition.y += 1f;

            Quaternion doorRotation = FG_GridUtility.GetDoorRotation(direction);

            AsyncOperationHandle<GameObject> handle = doorReference.InstantiateAsync(
                doorPosition,
                doorRotation,
                _doorAnchor
            );
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject doorObj = handle.Result;
                doorObj.name = $"Door_{cell1.position}_{cell2.position}";

                RoomDoor roomDoor = doorObj.GetComponent<RoomDoor>();
                if (roomDoor == null)
                {
                    roomDoor = doorObj.AddComponent<RoomDoor>();
                }

                BaseDoorController doorController = doorObj.GetComponent<BaseDoorController>();
                bool startsOpen = false;

                if (doorController != null)
                {
                    if (doorController.startOpened)
                    {
                        if (doorController.chanceToStartOpened)
                        {
                            System.Random doorRandom = new System.Random(doorSeed);
                            startsOpen = doorRandom.NextDouble() < doorController.percentChanceToStartOpened;
                        }
                        else
                        {
                            startsOpen = true;
                        }
                    }
                }

                roomDoor.Initialize(cell1.position, cell2.position, startsOpen);

                _doorInstances.Add(doorObj);
            }
        }

        public async UniTask BakeAllNavigationAsync(Dictionary<Vector2Int, RoomInstance> roomInstances)
        {
            int bakedCount = 0;
            const int BAKE_BATCH_SIZE = 3; // 3 rooms a frame

            var roomList = new List<RoomInstance>(roomInstances.Values);

            for (int i = 0; i < roomList.Count; i += BAKE_BATCH_SIZE)
            {
                List<UniTask> batchTasks = new List<UniTask>();

                for (int j = i; j < Mathf.Min(i + BAKE_BATCH_SIZE, roomList.Count); j++)
                {
                    if (roomList[j] != null)
                    {
                        batchTasks.Add(roomList[j].BakeNavigationAsync());
                        bakedCount++;
                    }
                }

                await UniTask.WhenAll(batchTasks);
                await UniTask.DelayFrame(1);
            }

            Log.VerboseSuccess($"Baked navigation for {bakedCount} rooms");
        }
    }
}