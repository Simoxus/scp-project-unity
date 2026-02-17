using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Facility.Generation
{
    public class FG_PathBuilder
    {
        private readonly FacilityGeneratorSettings _settings;
        private GridCell[,] _grid;
        private List<GridCell> _occupiedCells;

        public FG_PathBuilder(FacilityGeneratorSettings settings)
        {
            _settings = settings;
        }

        public void ConnectCells(GridCell[,] grid, List<GridCell> occupiedCells)
        {
            _grid = grid;
            _occupiedCells = occupiedCells;

            foreach (var cell in occupiedCells)
            {
                if (cell.isBlocked) continue;

                Vector2Int pos = cell.position;

                CheckAndConnectNeighbor(cell, Direction.North, pos + new Vector2Int(0, 1));
                CheckAndConnectNeighbor(cell, Direction.East, pos + new Vector2Int(1, 0));
                CheckAndConnectNeighbor(cell, Direction.South, pos + new Vector2Int(0, -1));
                CheckAndConnectNeighbor(cell, Direction.West, pos + new Vector2Int(-1, 0));
            }

            ConvertCheckpointCells();
        }

        private void CheckAndConnectNeighbor(GridCell cell, Direction direction, Vector2Int neighborPos)
        {
            if (neighborPos.x < 0 || neighborPos.x >= _settings.GridWidth ||
                neighborPos.y < 0 || neighborPos.y >= _settings.GridHeight)
                return;

            GridCell neighbor = _grid[neighborPos.x, neighborPos.y];

            if (neighbor != null && !neighbor.isBlocked)
            {
                cell.SetExit(direction, true);

                Direction oppositeDir = GetOppositeDirection(direction);
                neighbor.SetExit(oppositeDir, true);
            }
        }

        private void ConvertCheckpointCells()
        {
            int convertedCount = 0;

            foreach (var cell in _occupiedCells)
            {
                if (cell.isBlocked || !cell.isCheckpoint) continue;

                // Check if this cell has any neighbor in a different zone
                bool connectsDifferentZones = false;

                Vector2Int[] neighbors = new Vector2Int[]
                {
                    cell.position + new Vector2Int(0, 1),  // North
                    cell.position + new Vector2Int(1, 0),  // East
                    cell.position + new Vector2Int(0, -1), // South
                    cell.position + new Vector2Int(-1, 0)  // West
                };

                foreach (var neighborPos in neighbors)
                {
                    if (neighborPos.x < 0 || neighborPos.x >= _settings.GridWidth ||
                        neighborPos.y < 0 || neighborPos.y >= _settings.GridHeight)
                        continue;

                    GridCell neighbor = _grid[neighborPos.x, neighborPos.y];
                    if (neighbor != null && !neighbor.isBlocked && neighbor.zone != cell.zone)
                    {
                        connectsDifferentZones = true;
                        break;
                    }
                }

                if (connectsDifferentZones)
                {
                    cell.layout = RoomLayout.Checkpoint;
                    convertedCount++;
                    Log.Info($"Converted cell at ({cell.position.x}, {cell.position.y}) to Checkpoint layout");
                }
                else
                {
                    cell.isCheckpoint = false;
                }
            }

            Log.Info($"Converted {convertedCount} cells to Checkpoint layout");
        }

        private Direction GetOppositeDirection(Direction dir)
        {
            return dir switch
            {
                Direction.North => Direction.South,
                Direction.South => Direction.North,
                Direction.East => Direction.West,
                Direction.West => Direction.East,
                _ => Direction.North
            };
        }

        public void EnsureMinimumRequirements()
        {
            foreach (var zoneSettings in _settings.Zones)
            {
                var zoneCells = _occupiedCells.Where(c => c.zone == zoneSettings.zoneLocation && !c.isBlocked).ToList();

                int deadEnds = zoneCells.Count(c => c.layout == RoomLayout.DeadEnd);
                int corners = zoneCells.Count(c => c.layout == RoomLayout.Corner);
                int crossroads = zoneCells.Count(c => c.layout == RoomLayout.Crossroads);

                Log.VerboseInfo($"Zone {zoneSettings.zoneName}: DeadEnds={deadEnds} Corners={corners} Crossroads={crossroads}");
            }
        }
    }
}