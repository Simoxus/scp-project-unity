using System.Collections.Generic;
using UnityEngine;

namespace Facility.Generation
{
    public class FG_LayoutAnalyzer
    {
        private readonly FacilityGeneratorSettings _settings;
        private GridCell[,] _grid;
        private List<GridCell> _occupiedCells;

        public FG_LayoutAnalyzer(FacilityGeneratorSettings settings)
        {
            _settings = settings;
        }

        public void SetData(GridCell[,] grid, List<GridCell> occupiedCells)
        {
            _grid = grid;
            _occupiedCells = occupiedCells;
        }

        public bool IsFullyConnected(GridCell startCell)
        {
            if (_grid == null || startCell == null) return false;

            var reachable = FloodFill(startCell.position);
            int totalActive = 0;

            foreach (var cell in _occupiedCells)
            {
                if (!cell.isBlocked)
                {
                    totalActive++;
                }
            }

            if (reachable.Count != totalActive)
            {
                return false;
            }

            Log.VerboseSuccess($"All {totalActive} cells reachable; connectivity check passed");
            return true;
        }

        public HashSet<Vector2Int> FloodFill(Vector2Int startPos)
        {
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            if (!FG_GridUtility.IsValidGridPosition(startPos, _settings.GridWidth, _settings.GridHeight))
            {
                return visited;
            }

            GridCell startCell = _grid[startPos.x, startPos.y];
            if (startCell == null || startCell.isBlocked)
            {
                return visited;
            }

            queue.Enqueue(startPos);
            visited.Add(startPos);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                GridCell currentCell = _grid[current.x, current.y];

                for (int i = 0; i < 4; i++)
                {
                    Direction dir = (Direction)i;
                    if (!currentCell.HasExit(dir)) continue;

                    Vector2Int neighborPos = FG_GridUtility.GetNeighborPosition(current, dir);
                    if (visited.Contains(neighborPos)) continue;
                    if (!FG_GridUtility.IsValidGridPosition(neighborPos, _settings.GridWidth, _settings.GridHeight)) continue;

                    GridCell neighbor = _grid[neighborPos.x, neighborPos.y];
                    if (neighbor == null || neighbor.isBlocked) continue;

                    visited.Add(neighborPos);
                    queue.Enqueue(neighborPos);
                }
            }

            return visited;
        }

        public List<GridCell> GetIsolatedCells(GridCell startCell)
        {
            if (startCell == null) return new List<GridCell>();

            var reachable = FloodFill(startCell.position);
            var isolated = new List<GridCell>();

            foreach (var cell in _occupiedCells)
            {
                if (!cell.isBlocked && !reachable.Contains(cell.position))
                {
                    isolated.Add(cell);
                }
            }

            return isolated;
        }

        public List<Vector2Int> FindShortestPath(Vector2Int from, Vector2Int to)
        {
            if (_grid == null) return null;
            if (!FG_GridUtility.IsValidGridPosition(from, _settings.GridWidth, _settings.GridHeight)) return null;
            if (!FG_GridUtility.IsValidGridPosition(to, _settings.GridWidth, _settings.GridHeight)) return null;
            if (from == to) return new List<Vector2Int> { from };

            var previous = new Dictionary<Vector2Int, Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            var queue = new Queue<Vector2Int>();

            queue.Enqueue(from);
            visited.Add(from);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                if (current == to)
                    return ReconstructPath(previous, from, to);

                GridCell currentCell = _grid[current.x, current.y];
                if (currentCell == null) continue;

                for (int i = 0; i < 4; i++)
                {
                    Direction dir = (Direction)i;
                    if (!currentCell.HasExit(dir)) continue;

                    Vector2Int neighborPos = FG_GridUtility.GetNeighborPosition(current, dir);
                    if (visited.Contains(neighborPos)) continue;
                    if (!FG_GridUtility.IsValidGridPosition(neighborPos, _settings.GridWidth, _settings.GridHeight)) continue;

                    GridCell neighbor = _grid[neighborPos.x, neighborPos.y];
                    if (neighbor == null || neighbor.isBlocked) continue;

                    visited.Add(neighborPos);
                    previous[neighborPos] = current;
                    queue.Enqueue(neighborPos);
                }
            }

            return null;
        }

        public int GetGridDistance(Vector2Int from, Vector2Int to)
        {
            var path = FindShortestPath(from, to);
            return path != null ? path.Count - 1 : -1;
        }

        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> previous, Vector2Int from, Vector2Int to)
        {
            var path = new List<Vector2Int>();
            Vector2Int current = to;

            while (current != from)
            {
                path.Add(current);
                if (!previous.TryGetValue(current, out current))
                {
                    return null;
                }
            }

            path.Add(from);
            path.Reverse();
            return path;
        }

        public List<GridCell> GetCellsWithinSteps(Vector2Int origin, int maxSteps)
        {
            var result = new List<GridCell>();
            var visited = new Dictionary<Vector2Int, int>();
            var queue = new Queue<(Vector2Int pos, int steps)>();

            if (!FG_GridUtility.IsValidGridPosition(origin, _settings.GridWidth, _settings.GridHeight))
                return result;

            queue.Enqueue((origin, 0));
            visited[origin] = 0;

            while (queue.Count > 0)
            {
                var (current, steps) = queue.Dequeue();
                GridCell currentCell = _grid[current.x, current.y];

                if (currentCell != null && !currentCell.isBlocked && current != origin)
                {
                    result.Add(currentCell);
                }

                if (steps >= maxSteps) continue;
                if (currentCell == null) continue;

                for (int i = 0; i < 4; i++)
                {
                    Direction dir = (Direction)i;
                    if (!currentCell.HasExit(dir)) continue;

                    Vector2Int neighborPos = FG_GridUtility.GetNeighborPosition(current, dir);
                    if (visited.ContainsKey(neighborPos)) continue;
                    if (!FG_GridUtility.IsValidGridPosition(neighborPos, _settings.GridWidth, _settings.GridHeight)) continue;

                    GridCell neighbor = _grid[neighborPos.x, neighborPos.y];
                    if (neighbor == null || neighbor.isBlocked) continue;

                    visited[neighborPos] = steps + 1;
                    queue.Enqueue((neighborPos, steps + 1));
                }
            }

            return result;
        }

        public List<GridCell> GetCellsByLayout(RoomLayout layout)
        {
            var result = new List<GridCell>();
            foreach (var cell in _occupiedCells)
            {
                if (!cell.isBlocked && cell.layout == layout)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        public List<GridCell> GetCellsByZone(ZoneLocation zone)
        {
            var result = new List<GridCell>();
            foreach (var cell in _occupiedCells)
            {
                if (!cell.isBlocked && cell.zone == zone)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        public List<GridCell> GetCellsByZoneAndLayout(ZoneLocation zone, RoomLayout layout)
        {
            var result = new List<GridCell>();
            foreach (var cell in _occupiedCells)
            {
                if (!cell.isBlocked && cell.zone == zone && cell.layout == layout)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        public GridCell GetFurthestCell(Vector2Int origin)
        {
            GridCell furthest = null;
            int maxDistance = -1;

            foreach (var cell in _occupiedCells)
            {
                if (cell.isBlocked) continue;
                if (cell.position == origin) continue;

                int dist = GetGridDistance(origin, cell.position);
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    furthest = cell;
                }
            }

            return furthest;
        }

        public GridCell GetFurthestCellInZone(Vector2Int origin, ZoneLocation zone)
        {
            GridCell furthest = null;
            int maxDistance = -1;

            foreach (var cell in _occupiedCells)
            {
                if (cell.isBlocked || cell.zone != zone) continue;
                if (cell.position == origin) continue;

                int dist = GetGridDistance(origin, cell.position);
                if (dist > maxDistance)
                {
                    maxDistance = dist;
                    furthest = cell;
                }
            }

            return furthest;
        }

        public GridCell FindNearestCellWithLayout(Vector2Int origin, RoomLayout layout, ZoneLocation? zone = null)
        {
            GridCell nearest = null;
            int minDistance = int.MaxValue;

            foreach (var cell in _occupiedCells)
            {
                if (cell.isBlocked || cell.layout != layout) continue;
                if (zone.HasValue && cell.zone != zone.Value) continue;
                if (cell.position == origin) continue;

                int dist = GetGridDistance(origin, cell.position);
                if (dist >= 0 && dist < minDistance)
                {
                    minDistance = dist;
                    nearest = cell;
                }
            }

            return nearest;
        }

        public List<GridCell> GetCheckpointCells()
        {
            var result = new List<GridCell>();
            foreach (var cell in _occupiedCells)
            {
                if (!cell.isBlocked && cell.layout == RoomLayout.Checkpoint)
                {
                    result.Add(cell);
                }
            }
            return result;
        }

        public List<RoomData> GetMissingRequiredRooms()
        {
            var placedRoomIDs = new HashSet<string>();
            foreach (var cell in _occupiedCells)
            {
                if (!cell.isBlocked && cell.assignedRoom != null) placedRoomIDs.Add(cell.assignedRoom.RoomID);
            }

            var missing = new List<RoomData>();
            foreach (var zone in _settings.Zones)
            {
                if (zone.roomPool == null) continue;
                foreach (var room in zone.roomPool.NormalRooms)
                {
                    if (room != null && room.IsRequired && !placedRoomIDs.Contains(room.RoomID))
                    {
                        missing.Add(room);
                    }
                }
            }

            return missing;
        }

        public LayoutStats GetLayoutStats()
        {
            var stats = new LayoutStats();

            foreach (var cell in _occupiedCells)
            {
                if (cell.isBlocked) continue;

                stats.totalCells++;

                switch (cell.layout)
                {
                    case RoomLayout.DeadEnd: stats.deadEnds++; break;
                    case RoomLayout.Hallway: stats.hallways++; break;
                    case RoomLayout.Corner: stats.corners++; break;
                    case RoomLayout.Junction: stats.junctions++; break;
                    case RoomLayout.Crossroads: stats.crossroads++; break;
                    case RoomLayout.Checkpoint: stats.checkpoints++; break;
                }
            }

            return stats;
        }

        public void LogLayoutStats()
        {
            var stats = GetLayoutStats();
            Log.Info($"Layout stats — Total: {stats.totalCells} | DeadEnds: {stats.deadEnds} | Hallways: {stats.hallways} | Corners: {stats.corners} | Junctions: {stats.junctions} | Crossroads: {stats.crossroads} | Checkpoints: {stats.checkpoints}");
        }
    }

    public class LayoutStats
    {
        public int totalCells;
        public int deadEnds;
        public int hallways;
        public int corners;
        public int junctions;
        public int crossroads;
        public int checkpoints;
    }
}