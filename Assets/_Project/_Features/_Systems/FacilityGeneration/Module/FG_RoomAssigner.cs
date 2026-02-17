using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Facility.Generation
{
    public class FG_RoomAssigner
    {
        private readonly FacilityGeneratorSettings _settings;
        private readonly int _seed;
        private readonly System.Random _random;
        private GridCell[,] _grid;
        private List<GridCell> _occupiedCells;

        // Track which unique rooms have been placed
        private HashSet<string> _placedUniqueRooms = new HashSet<string>();

        public FG_RoomAssigner(FacilityGeneratorSettings settings, int seed, System.Random random)
        {
            _settings = settings;
            _seed = seed;
            _random = random;
        }

        public async UniTask AssignRooms(GridCell[,] grid, List<GridCell> occupiedCells, GridCell startCell)
        {
            _grid = grid;
            _occupiedCells = occupiedCells;
            _placedUniqueRooms.Clear();

            InitializeRoomPools();

            // Sort cells: start from bottom-right, move up (like SCP-CB)
            // This ensures unique rooms get priority placement
            var sortedCells = occupiedCells
                .Where(c => !c.isBlocked)
                .OrderBy(c => c.position.y)           // Bottom to top
                .ThenByDescending(c => c.position.x)  // Right to left
                .ToList();

            // Phase 1: Assign start room
            if (startCell != null)
            {
                await AssignStartRoom(startCell);
            }

            // Phase 2: Assign unique/required rooms first (like SCP-CB)
            foreach (var cell in sortedCells)
            {
                if (cell == startCell) continue;
                if (cell.assignedRoom != null) continue;

                AssignUniqueRoomToCell(cell);
            }

            // Phase 3: Fill remaining cells with generic rooms
            foreach (var cell in sortedCells)
            {
                if (cell.assignedRoom == null)
                {
                    AssignGenericRoomToCell(cell);
                }
            }

            await UniTask.Yield();
        }

        private void InitializeRoomPools()
        {
            foreach (var zoneSettings in _settings.Zones)
            {
                if (zoneSettings.roomPool != null)
                {
                    zoneSettings.roomPool.Initialize();
                }
            }
        }

        private async UniTask AssignStartRoom(GridCell cell)
        {
            var zoneSettings = _settings.GetZoneSettings(cell.zone);
            if (zoneSettings?.roomPool == null) return;

            int cellSeed = _seed + cell.position.x * 1000 + cell.position.y;

            if (_settings.StartingRoom != null)
            {
                if (CanPlaceRoom(cell, _settings.StartingRoom))
                {
                    PlaceRoom(cell, _settings.StartingRoom);
                }
                else
                {
                    var fallback = GetMatchingRoom(zoneSettings.roomPool, cell, cellSeed, false);
                    if (fallback != null)
                    {
                        PlaceRoom(cell, fallback);
                    }
                }
            }
            else
            {
                var room = GetMatchingRoom(zoneSettings.roomPool, cell, cellSeed, false);
                if (room != null)
                {
                    PlaceRoom(cell, room);
                }
            }

            await UniTask.Yield();
        }

        private void AssignUniqueRoomToCell(GridCell cell)
        {
            var zoneSettings = _settings.GetZoneSettings(cell.zone);
            if (zoneSettings?.roomPool == null) return;

            int cellSeed = _seed + cell.position.x * 1000 + cell.position.y;

            // Try to place a unique/required room
            var uniqueRoom = GetMatchingRoom(zoneSettings.roomPool, cell, cellSeed, true);
            if (uniqueRoom != null)
            {
                PlaceRoom(cell, uniqueRoom);
            }
        }

        private void AssignGenericRoomToCell(GridCell cell)
        {
            var zoneSettings = _settings.GetZoneSettings(cell.zone);
            if (zoneSettings?.roomPool == null)
            {
                Log.Warning($"No room pool for zone {cell.zone} at {cell.position}");
                return;
            }

            int cellSeed = _seed + cell.position.x * 1000 + cell.position.y;

            var room = GetMatchingRoom(zoneSettings.roomPool, cell, cellSeed, false);
            if (room != null)
            {
                PlaceRoom(cell, room);
            }
        }

        private RoomData GetMatchingRoom(RoomPool pool, GridCell cell, int cellSeed, bool uniqueOnly)
        {
            var localRandom = new System.Random(cellSeed);

            // Get all rooms that match the layout
            var candidateRooms = pool.NormalRooms
                .Where(r => r.Layout == cell.layout)
                .ToList();

            if (uniqueOnly)
            {
                // Only consider unique/required rooms that haven't been placed
                candidateRooms = candidateRooms
                    .Where(r => (r.IsUnique || r.IsRequired) && !_placedUniqueRooms.Contains(r.RoomID))
                    .ToList();
            }
            else
            {
                // Filter out unique rooms that have already been placed
                candidateRooms = candidateRooms
                    .Where(r => !r.IsUnique || !_placedUniqueRooms.Contains(r.RoomID))
                    .ToList();
            }

            // Filter by whether they can fit
            candidateRooms = candidateRooms
                .Where(r => CanPlaceRoom(cell, r))
                .ToList();

            if (candidateRooms.Count == 0)
            {
                return null;
            }

            // Weighted random selection
            float totalWeight = candidateRooms.Sum(r => r.SpawnWeight);
            float randomValue = (float)(localRandom.NextDouble() * totalWeight);
            float currentWeight = 0f;

            foreach (var room in candidateRooms)
            {
                currentWeight += room.SpawnWeight;
                if (randomValue <= currentWeight)
                {
                    return room;
                }
            }

            return candidateRooms[candidateRooms.Count - 1];
        }

        private bool CanPlaceRoom(GridCell cell, RoomData roomData)
        {
            if (!roomData.IsLarge) return true;

            int rotation = FG_RotationCalculator.CalculateRotation(cell, roomData);
            Vector2Int[] rotatedCells = FG_GridUtility.GetRotatedOccupiedCells(roomData, rotation);

            foreach (var offset in rotatedCells)
            {
                int checkCol = cell.position.x + offset.x;
                int checkRow = cell.position.y + offset.y;

                // Check grid bounds
                if (checkCol < 0 || checkCol >= _settings.GridWidth ||
                    checkRow < 0 || checkRow >= _settings.GridHeight)
                {
                    return false;
                }

                // Check if space is available
                var checkCell = _grid[checkCol, checkRow];
                if (checkCell != null && checkCell != cell)
                {
                    // Space occupied by another room or blocked
                    if (checkCell.assignedRoom != null || checkCell.isBlocked)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void PlaceRoom(GridCell cell, RoomData roomData)
        {
            cell.rotation = FG_RotationCalculator.CalculateRotation(cell, roomData);
            cell.assignedRoom = roomData;

            // Track unique rooms
            if (roomData.IsUnique)
            {
                _placedUniqueRooms.Add(roomData.RoomID);
            }

            // Handle large rooms using ROTATED cells
            if (roomData.IsLarge)
            {
                MarkLargeRoomCells(cell.position, roomData, cell.zone, cell.rotation);
            }
        }

        private void MarkLargeRoomCells(Vector2Int anchorPosition, RoomData roomData, ZoneLocation zone, int rotation)
        {
            if (!roomData.IsLarge) return;

            Vector2Int[] rotatedCells = FG_GridUtility.GetRotatedOccupiedCells(roomData, rotation);
            foreach (var offset in rotatedCells)
            {
                if (offset == Vector2Int.zero) continue; // Skip anchor cell

                int blockCol = anchorPosition.x + offset.x;
                int blockRow = anchorPosition.y + offset.y;

                if (blockCol < 0 || blockCol >= _settings.GridWidth ||
                    blockRow < 0 || blockRow >= _settings.GridHeight)
                    continue;

                if (_grid[blockCol, blockRow] == null)
                {
                    GridCell blockedCell = new GridCell(new Vector2Int(blockCol, blockRow));
                    blockedCell.zone = zone;
                    blockedCell.MarkAsBlocked(anchorPosition);
                    _grid[blockCol, blockRow] = blockedCell;
                    _occupiedCells.Add(blockedCell);
                }
                else
                {
                    _grid[blockCol, blockRow].MarkAsBlocked(anchorPosition);
                }
            }
        }
    }
}