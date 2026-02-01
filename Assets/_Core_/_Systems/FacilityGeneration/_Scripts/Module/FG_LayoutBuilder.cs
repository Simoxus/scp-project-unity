using System.Collections.Generic;
using UnityEngine;

namespace Facility.Generation
{
    public class FG_LayoutBuilder
    {
        private readonly FacilityGeneratorSettings _settings;
        private readonly System.Random _random;
        private GridCell[,] _grid;
        private List<GridCell> _occupiedCells;
        private GridCell _startCell;

        public FG_LayoutBuilder(FacilityGeneratorSettings settings, System.Random random)
        {
            _settings = settings;
            _random = random;
        }

        public (GridCell[,] grid, List<GridCell> occupiedCells, GridCell startCell) GenerateGrid()
        {
            _grid = new GridCell[_settings.GridWidth, _settings.GridHeight];
            _occupiedCells = new List<GridCell>();

            foreach (var zone in _settings.Zones)
            {
                Log.Info($"Zone '{zone.zoneName}' ({zone.zoneLocation}): Rows {zone.startRow}-{zone.endRow}");
            }

            int x = _settings.GridWidth / 2;
            int y = 1;

            int startCol = _settings.GridWidth / 2;
            _startCell = CreateCell(startCol, 0, _settings.Zones[0].zoneLocation);

            int width, height;
            int temp = 0;

            int iterations = 0;
            int maxIterations = _settings.MaxGenerationAttempts * 100;

            do
            {
                iterations++;
                if (iterations > maxIterations)
                {
                    break;
                }

                width = _random.Next(_settings.HorizontalHallwayLengthMin, _settings.HorizontalHallwayLengthMax + 1);

                if (x > _settings.GridWidth * 0.6)
                {
                    width = -width;
                }
                else if (x > _settings.GridWidth * 0.4)
                {
                    x = x - width / 2;
                }

                if (x + width > _settings.GridWidth - 3)
                {
                    width = _settings.GridWidth - 3 - x;
                }
                else if (x + width < 2)
                {
                    width = -x + 2;
                }

                x = Mathf.Min(x, x + width);
                width = Mathf.Abs(width);

                for (int i = x; i <= x + width; i++)
                {
                    var cell = CreateCell(i, y, GetZoneForRow(y));
                    if (cell == null) continue;

                    if (y < _settings.GridHeight - 1 && GetZoneForRow(y) != GetZoneForRow(y + 1))
                    {
                        cell.isCheckpoint = true;
                        Log.Info($"Marked horizontal cell at ({i}, {y}) as checkpoint (crosses from zone {GetZoneForRow(y)} to {GetZoneForRow(y + 1)})");
                    }
                }

                Log.VerboseInfo($"Built horizontal hallway at row {y} from {x} to {x + width} (Zone: {GetZoneForRow(y)})");

                height = _random.Next(_settings.VerticalHallwayLengthMin, _settings.VerticalHallwayLengthMax + 1);

                if (y + height > _settings.GridHeight - 3)
                {
                    height = _settings.GridHeight - 3 - y;
                }

                int yHallways = _random.Next(_settings.VerticalConnectionsMin, _settings.VerticalConnectionsMax + 1);

                for (int i = 1; i <= yHallways; i++)
                {
                    int x2 = Mathf.Max(2, Mathf.Min(_settings.GridWidth - 2, _random.Next(x, x + width)));

                    while (_grid[x2 - 1, y + 1] != null || _grid[x2, y + 1] != null || _grid[x2 + 1, y + 1] != null)
                    {
                        x2++;
                        if (x2 >= x + width) break;
                    }

                    if (x2 < x + width)
                    {
                        int tempHeight;

                        if (i == 1)
                        {
                            tempHeight = height;
                            if (_random.Next(1, 3) == 1)
                                x2 = x;
                            else
                                x2 = x + width;
                        }
                        else
                        {
                            tempHeight = _random.Next(1, height + 1);
                        }

                        for (int y2 = y; y2 <= y + tempHeight; y2++)
                        {
                            var cell = CreateCell(x2, y2, GetZoneForRow(y2));
                            if (cell == null) continue;

                            if (y2 < _settings.GridHeight - 1 && GetZoneForRow(y2) != GetZoneForRow(y2 + 1))
                            {
                                cell.isCheckpoint = true;
                                Log.Info($"Marked cell at ({x2}, {y2}) as checkpoint (crosses from zone {GetZoneForRow(y2)} to {GetZoneForRow(y2 + 1)})");
                            }
                        }

                        if (tempHeight == height)
                        {
                            temp = x2;
                        }

                        Log.VerboseInfo($"Built vertical hallway at col {x2} height {tempHeight}");
                    }
                }

                x = temp;
                y = y + height;

            } while (y <= _settings.GridHeight - 3);

            Log.Info($"Generated {_occupiedCells.Count} cells");
            return (_grid, _occupiedCells, _startCell);
        }

        private ZoneLocation GetZoneForRow(int row)
        {
            foreach (var zone in _settings.Zones)
            {
                if (zone.ContainsRow(row))
                    return zone.zoneLocation;
            }
            return _settings.Zones[0].zoneLocation;
        }

        private GridCell CreateCell(int col, int row, ZoneLocation zone)
        {
            if (_grid[col, row] == null)
            {
                GridCell cell = new GridCell(new Vector2Int(col, row));
                cell.zone = zone;
                _grid[col, row] = cell;
                _occupiedCells.Add(cell);
                return cell;
            }
            return _grid[col, row];
        }
    }
}