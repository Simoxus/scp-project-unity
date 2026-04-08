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

        private HashSet<string> _placedUniqueRooms = new HashSet<string>();

        private class CachedRoomList
        {
            public List<RoomData> all = new List<RoomData>();
            public List<RoomData> uniqueOrRequired = new List<RoomData>();
        }

        private Dictionary<ZoneLocation, Dictionary<RoomLayout, CachedRoomList>> _roomCache
            = new Dictionary<ZoneLocation, Dictionary<RoomLayout, CachedRoomList>>();

        public FG_RoomAssigner(FacilityGeneratorSettings settings, int seed, System.Random random)
        {
            _settings = settings;
            _seed = seed;
            _random = random;
        }

        public UniTask AssignRooms(GridCell[,] grid, List<GridCell> occupiedCells, GridCell startCell)
        {
            _grid = grid;
            _occupiedCells = occupiedCells;
            _placedUniqueRooms.Clear();

            InitializeRoomPools();
            BuildRoomCache();

            var sortedCells = occupiedCells
                .Where(c => !c.isBlocked)
                .OrderBy(c => c.position.y)
                .ThenByDescending(c => c.position.x)
                .ToList();

            if (startCell != null)
            {
                AssignStartRoom(startCell);
            }

            foreach (var cell in sortedCells)
            {
                if (cell == startCell) continue;
                if (cell.assignedRoom != null) continue;
                AssignUniqueRoomToCell(cell);
            }

            foreach (var cell in sortedCells)
            {
                if (cell.assignedRoom == null)
                {
                    AssignGenericRoomToCell(cell);
                }
            }

            return UniTask.CompletedTask;
        }

        private void BuildRoomCache()
        {
            _roomCache.Clear();

            foreach (var zoneSettings in _settings.Zones)
            {
                if (zoneSettings.roomPool == null) continue;

                var layoutMap = new Dictionary<RoomLayout, CachedRoomList>();

                foreach (var room in zoneSettings.roomPool.NormalRooms)
                {
                    if (room == null) continue;

                    if (!layoutMap.TryGetValue(room.Layout, out var cached))
                    {
                        cached = new CachedRoomList();
                        layoutMap[room.Layout] = cached;
                    }

                    cached.all.Add(room);

                    if (room.IsUnique || room.IsRequired)
                        cached.uniqueOrRequired.Add(room);
                }

                _roomCache[zoneSettings.zoneLocation] = layoutMap;
            }
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

        private void AssignStartRoom(GridCell cell)
        {
            var zoneSettings = _settings.GetZoneSettings(cell.zone);
            if (zoneSettings?.roomPool == null) return;

            int cellSeed = _seed + cell.position.x * 1000 + cell.position.y;

            if (_settings.StartingRoom != null)
            {
                if (CanPlaceRoom(cell, _settings.StartingRoom))
                {
                    PlaceRoom(cell, _settings.StartingRoom);
                    return;
                }
            }

            var fallback = GetMatchingRoom(cell, cellSeed, false);
            if (fallback != null) PlaceRoom(cell, fallback);
        }

        private void AssignUniqueRoomToCell(GridCell cell)
        {
            int cellSeed = _seed + cell.position.x * 1000 + cell.position.y;
            var uniqueRoom = GetMatchingRoom(cell, cellSeed, true);
            if (uniqueRoom != null)
                PlaceRoom(cell, uniqueRoom);
        }

        private void AssignGenericRoomToCell(GridCell cell)
        {
            var zoneSettings = _settings.GetZoneSettings(cell.zone);
            if (zoneSettings?.roomPool == null)
            {
                Log.Warning($"No room pool for zone '{cell.zone}' at {cell.position}");
                return;
            }

            int cellSeed = _seed + cell.position.x * 1000 + cell.position.y;
            var room = GetMatchingRoom(cell, cellSeed, false);
            if (room != null)
                PlaceRoom(cell, room);
        }

        private RoomData GetMatchingRoom(GridCell cell, int cellSeed, bool uniqueOnly)
        {
            if (!_roomCache.TryGetValue(cell.zone, out var layoutMap)) return null;
            if (!layoutMap.TryGetValue(cell.layout, out var cached)) return null;

            var source = uniqueOnly ? cached.uniqueOrRequired : cached.all;
            if (source.Count == 0) return null;

            var localRandom = new System.Random(cellSeed);

            float totalWeight = 0f;
            for (int i = 0; i < source.Count; i++)
            {
                var r = source[i];
                if (r.IsUnique && _placedUniqueRooms.Contains(r.RoomID)) continue;
                if (!CanPlaceRoom(cell, r)) continue;
                totalWeight += r.SpawnWeight;
            }

            if (totalWeight <= 0f) return null;

            float randomValue = (float)(localRandom.NextDouble() * totalWeight);
            float currentWeight = 0f;

            for (int i = 0; i < source.Count; i++)
            {
                var r = source[i];
                if (r.IsUnique && _placedUniqueRooms.Contains(r.RoomID)) continue;
                if (!CanPlaceRoom(cell, r)) continue;

                currentWeight += r.SpawnWeight;
                if (randomValue <= currentWeight)
                    return r;
            }

            return source[source.Count - 1];
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

                if (checkCol < 0 || checkCol >= _settings.GridWidth ||
                    checkRow < 0 || checkRow >= _settings.GridHeight)
                    return false;

                var checkCell = _grid[checkCol, checkRow];
                if (checkCell != null && checkCell != cell)
                    return false;
            }

            return true;
        }

        private void PlaceRoom(GridCell cell, RoomData roomData)
        {
            cell.rotation = FG_RotationCalculator.CalculateRotation(cell, roomData);
            cell.assignedRoom = roomData;

            if (roomData.IsUnique)
                _placedUniqueRooms.Add(roomData.RoomID);

            if (roomData.IsLarge)
                MarkLargeRoomCells(cell.position, roomData, cell.zone, cell.rotation);
        }

        private void MarkLargeRoomCells(Vector2Int anchorPosition, RoomData roomData, ZoneLocation zone, int rotation)
        {
            if (!roomData.IsLarge) return;

            Vector2Int[] rotatedCells = FG_GridUtility.GetRotatedOccupiedCells(roomData, rotation);
            foreach (var offset in rotatedCells)
            {
                if (offset == Vector2Int.zero) continue;

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
                    var existingCell = _grid[blockCol, blockRow];
                    existingCell.isCheckpoint = false;
                    existingCell.assignedRoom = null;

                    for (int d = 0; d < 4; d++)
                        existingCell.SetExit((Direction)d, false);

                    existingCell.MarkAsBlocked(anchorPosition);
                }
            }
        }
    }
}