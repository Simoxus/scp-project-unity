using UnityEngine;

namespace Facility.Generation
{
    public static class FG_GridUtility
    {
        public static Vector2Int GetNeighborPosition(Vector2Int pos, Direction dir)
        {
            return dir switch
            {
                Direction.North => pos + new Vector2Int(0, 1),
                Direction.East => pos + new Vector2Int(1, 0),
                Direction.South => pos + new Vector2Int(0, -1),
                Direction.West => pos + new Vector2Int(-1, 0),
                _ => pos
            };
        }

        public static bool IsValidGridPosition(Vector2Int pos, int gridWidth, int gridHeight)
        {
            return pos.x >= 0 && pos.x < gridWidth &&
                   pos.y >= 0 && pos.y < gridHeight;
        }

        public static Vector3 GridToWorldPosition(Vector2Int gridPos, float cellSize)
        {
            return new Vector3(
                gridPos.x * cellSize,
                0,
                gridPos.y * cellSize
            );
        }

        public static Direction GetOppositeDirection(Direction dir)
        {
            return dir switch
            {
                Direction.North => Direction.South,
                Direction.East => Direction.West,
                Direction.South => Direction.North,
                Direction.West => Direction.East,
                _ => dir
            };
        }

        public static Quaternion GetDoorRotation(Direction direction)
        {
            return direction switch
            {
                Direction.North => Quaternion.Euler(0, 0, 0),
                Direction.East => Quaternion.Euler(0, 90, 0),
                Direction.South => Quaternion.Euler(0, 180, 0),
                Direction.West => Quaternion.Euler(0, 270, 0),
                _ => Quaternion.identity
            };
        }

        public static bool ShouldCreateDoor(Vector2Int pos1, Vector2Int pos2)
        {
            return pos1.y < pos2.y || (pos1.y == pos2.y && pos1.x < pos2.x);
        }

        public static Vector2Int RotateCellOffset(Vector2Int offset, int rotation)
        {
            rotation = ((rotation % 4) + 4) % 4;
            return rotation switch
            {
                0 => offset,
                1 => new Vector2Int(offset.y, -offset.x),
                2 => new Vector2Int(-offset.x, -offset.y),
                3 => new Vector2Int(-offset.y, offset.x),
                _ => offset
            };
        }

        public static Vector2Int[] GetRotatedOccupiedCells(RoomData roomData, int rotation)
        {
            if (!roomData.IsLarge)
                return new Vector2Int[] { Vector2Int.zero };

            Vector2Int[] originalCells = roomData.GetOccupiedCells();
            Vector2Int[] rotatedCells = new Vector2Int[originalCells.Length];

            for (int i = 0; i < originalCells.Length; i++)
            {
                rotatedCells[i] = RotateCellOffset(originalCells[i], rotation);
            }

            return rotatedCells;
        }
    }
}