using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Facility.Generation
{
    public class FG_GizmoDrawer
    {
        private readonly FacilityGeneratorSettings _settings;
        private GridCell[,] _grid;
        private List<GridCell> _occupiedCells;
        private GridCell _startRoomCell;

        public FG_GizmoDrawer(FacilityGeneratorSettings settings)
        {
            _settings = settings;
        }

        public void SetData(GridCell[,] grid, List<GridCell> occupiedCells, GridCell startRoomCell)
        {
            _grid = grid;
            _occupiedCells = occupiedCells;
            _startRoomCell = startRoomCell;
        }

        public void DrawGizmos()
        {
            if (_grid == null || _settings == null) return;

            DrawFullGrid();

            if (_occupiedCells != null)
            {
                foreach (var cell in _occupiedCells)
                {
                    DrawOccupiedCell(cell);
                }
            }
        }

        private void DrawFullGrid()
        {
            for (int y = 0; y < _settings.GridHeight; y++)
            {
                for (int x = 0; x < _settings.GridWidth; x++)
                {
                    Vector3 worldPos = GridToWorldPosition(new Vector2Int(x, y));
                    DrawGridCell(x, y, worldPos);
                }
            }
        }

        private void DrawGridCell(int x, int y, Vector3 worldPos)
        {
            ZoneSettings zoneSettings = GetZoneForRow(y);
            bool isPaddingRow = IsPaddingRow(y);

            Color outlineColor;
            if (isPaddingRow)
            {
                outlineColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
            }
            else if (zoneSettings != null)
            {
                outlineColor = zoneSettings.debugColor;
                outlineColor.a = 0.6f;
            }
            else
            {
                outlineColor = new Color(1f, 1f, 1f, 0.3f);
            }

            Gizmos.color = outlineColor;
            Vector3 flatSize = new Vector3(_settings.CellSize, 0.01f, _settings.CellSize);
            Gizmos.DrawWireCube(worldPos, flatSize);

            if (isPaddingRow)
            {
                Gizmos.color = new Color(0.3f, 0.3f, 0.3f, 0.15f);
                Gizmos.DrawCube(worldPos, new Vector3(_settings.CellSize * 0.95f, 0.01f, _settings.CellSize * 0.95f));
            }
        }

        private void DrawOccupiedCell(GridCell cell)
        {
            Vector3 worldPos = GridToWorldPosition(cell.position);

            if (cell.isBlocked)
            {
                DrawBlockedCell(cell, worldPos);
                return;
            }

            DrawActiveCell(cell, worldPos);
        }

        private void DrawBlockedCell(GridCell cell, Vector3 worldPos)
        {
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.25f);
            Gizmos.DrawCube(worldPos, new Vector3(_settings.CellSize * 0.65f, 0.01f, _settings.CellSize * 0.65f));

            Vector3 blockerPos = GridToWorldPosition(cell.blockedByRoomAt);
            Gizmos.color = new Color(0.6f, 0.6f, 0.6f, 0.4f);
            Gizmos.DrawLine(worldPos + Vector3.up * 0.01f, blockerPos + Vector3.up * 0.01f);
        }

        private void DrawActiveCell(GridCell cell, Vector3 worldPos)
        {
            Color cellColor = GetRoomTypeColor(cell);

            Gizmos.color = cellColor;
            Gizmos.DrawCube(worldPos, new Vector3(_settings.CellSize * 0.85f, 0.01f, _settings.CellSize * 0.85f));

            DrawExitGizmos(cell, worldPos);

            DrawLayoutIndicator(cell, worldPos);

            if (cell.assignedRoom != null && cell.assignedRoom.IsLarge)
            {
                DrawLargeRoomBoundary(cell, worldPos);
            }
        }

        private Color GetRoomTypeColor(GridCell cell)
        {
            if (cell == _startRoomCell)
            {
                return new Color(0, 1, 1, 0.7f);
            }

            Color baseColor = cell.layout switch
            {
                RoomLayout.DeadEnd => new Color(1f, 0.3f, 0.3f),
                RoomLayout.Hallway => new Color(0.3f, 0.5f, 1f),
                RoomLayout.Corner => new Color(1f, 1f, 0.3f),
                RoomLayout.Junction => new Color(1f, 0.3f, 1f),
                RoomLayout.Crossroads => new Color(0.9f, 0.9f, 0.9f),
                RoomLayout.Checkpoint => new Color(0.3f, 1f, 1f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };

            baseColor.a = 0.5f;
            return baseColor;
        }

        private void DrawExitGizmos(GridCell cell, Vector3 worldPos)
        {
            Gizmos.color = new Color(0, 1, 0, 0.9f);
            float exitSize = _settings.CellSize * 0.15f;
            float offset = _settings.CellSize * 0.42f;

            if (cell.HasExit(Direction.North))
            {
                Vector3 exitPos = worldPos + Vector3.forward * offset + Vector3.up * 0.02f;
                Gizmos.DrawCube(exitPos, new Vector3(exitSize, 0.02f, exitSize));
            }

            if (cell.HasExit(Direction.East))
            {
                Vector3 exitPos = worldPos + Vector3.right * offset + Vector3.up * 0.02f;
                Gizmos.DrawCube(exitPos, new Vector3(exitSize, 0.02f, exitSize));
            }

            if (cell.HasExit(Direction.South))
            {
                Vector3 exitPos = worldPos + Vector3.back * offset + Vector3.up * 0.02f;
                Gizmos.DrawCube(exitPos, new Vector3(exitSize, 0.02f, exitSize));
            }

            if (cell.HasExit(Direction.West))
            {
                Vector3 exitPos = worldPos + Vector3.left * offset + Vector3.up * 0.02f;
                Gizmos.DrawCube(exitPos, new Vector3(exitSize, 0.02f, exitSize));
            }
        }

        private void DrawLayoutIndicator(GridCell cell, Vector3 worldPos)
        {
            Color layoutColor = cell.layout switch
            {
                RoomLayout.DeadEnd => Color.red,
                RoomLayout.Hallway => Color.blue,
                RoomLayout.Corner => Color.yellow,
                RoomLayout.Junction => Color.magenta,
                RoomLayout.Crossroads => Color.white,
                RoomLayout.Checkpoint => Color.cyan,
                _ => Color.gray
            };

            layoutColor.a = 0.8f;
            Gizmos.color = layoutColor;

            float indicatorSize = _settings.CellSize * 0.2f;
            Gizmos.DrawCube(worldPos + Vector3.up * 0.03f, new Vector3(indicatorSize, 0.02f, indicatorSize));
        }

        private void DrawLargeRoomBoundary(GridCell cell, Vector3 worldPos)
        {
            Gizmos.color = new Color(1, 0, 1, 0.9f);

            RoomData roomData = cell.assignedRoom;

            // Use the stored rotation from the cell
            int rotation = cell.rotation;
            Vector2Int[] occupiedCells = FG_GridUtility.GetRotatedOccupiedCells(roomData, rotation);

            // Calculate bounds from ROTATED occupied cells
            int minX = occupiedCells.Min(c => c.x);
            int maxX = occupiedCells.Max(c => c.x);
            int minZ = occupiedCells.Min(c => c.y);
            int maxZ = occupiedCells.Max(c => c.y);

            // Calculate the boundary size
            Vector3 size = new Vector3(
                (maxX - minX + 1) * _settings.CellSize,
                0.02f,
                (maxZ - minZ + 1) * _settings.CellSize
            );

            // Center is at the middle of all occupied cells relative to anchor
            Vector3 center = worldPos + new Vector3(
                (minX + maxX) * _settings.CellSize * 0.5f,
                0.04f,
                (minZ + maxZ) * _settings.CellSize * 0.5f
            );

            Gizmos.DrawWireCube(center, size);
            DrawLargeRoomCorners(center, size);
            DrawAnchorPoint(worldPos);

            // Draw all ROTATED occupied cell positions
            foreach (var cellOffset in occupiedCells)
            {
                Vector3 cellWorldPos = worldPos + new Vector3(
                    cellOffset.x * _settings.CellSize,
                    0.02f,
                    cellOffset.y * _settings.CellSize
                );

                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawCube(cellWorldPos, new Vector3(_settings.CellSize * 0.9f, 0.02f, _settings.CellSize * 0.9f));
            }
        }

        private void DrawAnchorPoint(Vector3 worldPos)
        {
            Gizmos.color = new Color(1, 1, 0, 1f); // Yellow for anchor
            float anchorSize = _settings.CellSize * 0.15f;
            Gizmos.DrawSphere(worldPos + Vector3.up * 0.05f, anchorSize);
        }

        private void DrawLargeRoomCorners(Vector3 center, Vector3 size)
        {
            Gizmos.color = new Color(1, 0, 1, 1f);
            float cornerSize = _settings.CellSize * 0.12f;
            Vector3 halfSize = size / 2f;

            Vector3 flatCornerSize = new Vector3(cornerSize, 0.02f, cornerSize);
            Gizmos.DrawCube(center + new Vector3(halfSize.x, 0, halfSize.z), flatCornerSize);
            Gizmos.DrawCube(center + new Vector3(-halfSize.x, 0, halfSize.z), flatCornerSize);
            Gizmos.DrawCube(center + new Vector3(halfSize.x, 0, -halfSize.z), flatCornerSize);
            Gizmos.DrawCube(center + new Vector3(-halfSize.x, 0, -halfSize.z), flatCornerSize);
        }

        private ZoneSettings GetZoneForRow(int row)
        {
            if (_settings.Zones == null) return null;

            foreach (var zone in _settings.Zones)
            {
                if (zone.ContainsRow(row))
                {
                    return zone;
                }
            }

            return null;
        }

        private bool IsPaddingRow(int row)
        {
            if (_settings.Zones == null || _settings.Zones.Count == 0) return false;

            for (int i = 0; i < _settings.Zones.Count - 1; i++)
            {
                ZoneSettings currentZone = _settings.Zones[i];
                ZoneSettings nextZone = _settings.Zones[i + 1];

                if (row > currentZone.endRow && row < nextZone.startRow)
                {
                    return true;
                }
            }

            return false;
        }

        private Vector3 GridToWorldPosition(Vector2Int gridPos)
        {
            return new Vector3(
                gridPos.x * _settings.CellSize,
                0,
                gridPos.y * _settings.CellSize
            );
        }

        public void DrawZoneBoundaries()
        {
            if (_settings.Zones == null) return;

            foreach (var zone in _settings.Zones)
            {
                DrawZoneBoundary(zone);
            }
        }

        private void DrawZoneBoundary(ZoneSettings zone)
        {
            float minX = 0;
            float maxX = _settings.GridWidth * _settings.CellSize;
            float minZ = zone.startRow * _settings.CellSize;
            float maxZ = (zone.endRow + 1) * _settings.CellSize;

            Vector3 center = new Vector3(
                (minX + maxX) / 2f,
                0.05f,
                (minZ + maxZ) / 2f
            );

            Vector3 size = new Vector3(
                maxX - minX,
                0.02f,
                maxZ - minZ
            );

            Color boundaryColor = zone.debugColor;
            boundaryColor.a = 0.9f;
            Gizmos.color = boundaryColor;
            Gizmos.DrawWireCube(center, size);

            Gizmos.color = boundaryColor;
            float cornerSize = _settings.CellSize * 0.15f;
            Vector3 flatCornerSize = new Vector3(cornerSize, 0.02f, cornerSize);

            Gizmos.DrawCube(new Vector3(minX, 0.05f, minZ), flatCornerSize);
            Gizmos.DrawCube(new Vector3(maxX, 0.05f, minZ), flatCornerSize);
            Gizmos.DrawCube(new Vector3(minX, 0.05f, maxZ), flatCornerSize);
            Gizmos.DrawCube(new Vector3(maxX, 0.05f, maxZ), flatCornerSize);
        }
    }
}