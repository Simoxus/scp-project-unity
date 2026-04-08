using Facility.Persistence.Types;
using System.Collections.Generic;
using UnityEngine;

namespace Facility.Generation
{
    public class FG_NavMeshLinker
    {
        private readonly FacilityGeneratorSettings _settings;
        private readonly float _doorwayLinkOffset;
        private List<NavLinkData> _navLinkData;

        public FG_NavMeshLinker(FacilityGeneratorSettings settings, float doorwayLinkOffset)
        {
            _settings = settings;
            _doorwayLinkOffset = doorwayLinkOffset;
            _navLinkData = new List<NavLinkData>();
        }

        public List<NavLinkData> GetNavLinkData() => _navLinkData;
        public void ClearNavLinkData() => _navLinkData.Clear();

        public void CreateNavigationLinks(GridCell[,] grid, List<GridCell> occupiedCells)
        {
            int linkCount = 0;
            HashSet<(Vector2Int, Vector2Int)> processedPairs = new HashSet<(Vector2Int, Vector2Int)>();
            _navLinkData.Clear();

            foreach (var cell in occupiedCells)
            {
                if (cell.isBlocked || cell.instantiatedRoom == null) continue;

                for (int i = 0; i < 4; i++)
                {
                    Direction dir = (Direction)i;
                    if (!cell.HasExit(dir)) continue;

                    Vector2Int neighborPos = FG_GridUtility.GetNeighborPosition(cell.position, dir);
                    if (!FG_GridUtility.IsValidGridPosition(neighborPos, _settings.GridWidth, _settings.GridHeight)) continue;

                    GridCell neighbor = grid[neighborPos.x, neighborPos.y];
                    if (neighbor == null || neighbor.isBlocked || neighbor.instantiatedRoom == null) continue;

                    var pair = cell.position.x < neighborPos.x || (cell.position.x == neighborPos.x && cell.position.y < neighborPos.y)
                        ? (cell.position, neighborPos)
                        : (neighborPos, cell.position);

                    if (processedPairs.Add(pair))
                    {
                        Vector3 thisEnd = GetDoorwayPosition(cell.position, dir);
                        Vector3 neighborEnd = GetDoorwayPosition(neighborPos, FG_GridUtility.GetOppositeDirection(dir));

                        cell.instantiatedRoom.LinkNavigationToNeighbor(
                            neighbor.instantiatedRoom,
                            thisEnd,
                            neighborEnd
                        );

                        cell.instantiatedRoom.AddNeighbor(neighbor.instantiatedRoom);
                        neighbor.instantiatedRoom.AddNeighbor(cell.instantiatedRoom);

                        _navLinkData.Add(new NavLinkData(cell.position, neighborPos, thisEnd, neighborEnd));

                        linkCount++;
                    }
                }
            }

            Log.Info($"Created {linkCount} navigation links");
        }

        public void LoadNavigationLinksFromData(List<NavLinkData> navLinks, Dictionary<Vector2Int, RoomInstance> roomInstances)
        {
            if (navLinks == null || navLinks.Count == 0)
            {
                Log.Warning("No navigation link data to load");
                return;
            }

            int linkCount = 0;

            foreach (var linkData in navLinks)
            {
                if (!roomInstances.TryGetValue(linkData.cell1, out RoomInstance room1) ||
                    !roomInstances.TryGetValue(linkData.cell2, out RoomInstance room2))
                {
                    Log.Warning($"Could not find rooms for nav link between {linkData.cell1} and {linkData.cell2}");
                    continue;
                }

                if (room1 == null || room2 == null) continue;

                room1.LinkNavigationToNeighbor(room2, linkData.startPoint, linkData.endPoint);
                room1.AddNeighbor(room2);
                room2.AddNeighbor(room1);

                linkCount++;
            }

            _navLinkData = new List<NavLinkData>(navLinks);
            Log.Info($"Loaded {linkCount} navigation links from data");
        }

        public void SetupCullingSystem(CullingSystem cullingSystem, int roomCount)
        {
            if (cullingSystem == null) return;
            cullingSystem.IsActive = true;
        }

        private Vector3 GetDoorwayPosition(Vector2Int gridPos, Direction direction)
        {
            Vector3 roomCenter = FG_GridUtility.GridToWorldPosition(gridPos, _settings.CellSize);
            float offset = -_doorwayLinkOffset; // Always use entry offset

            return direction switch
            {
                Direction.North => roomCenter + new Vector3(0, 0, _settings.CellSize / 2f + offset),
                Direction.East => roomCenter + new Vector3(_settings.CellSize / 2f + offset, 0, 0),
                Direction.South => roomCenter + new Vector3(0, 0, -_settings.CellSize / 2f - offset),
                Direction.West => roomCenter + new Vector3(-_settings.CellSize / 2f - offset, 0, 0),
                _ => roomCenter
            };
        }
    }
}