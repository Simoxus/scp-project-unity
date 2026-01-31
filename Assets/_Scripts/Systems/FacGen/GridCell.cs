using UnityEngine;

namespace Facility.Generation
{
    public class GridCell
    {
        public Vector2Int position;
        public int rotation = 0;
        public RoomLayout layout;
        public RoomData assignedRoom;
        public RoomInstance instantiatedRoom;
        public ZoneLocation zone;
        public bool isCheckpoint = false;
        public int exitCount;
        public bool[] exits = new bool[4];

        public bool isBlocked;
        public Vector2Int blockedByRoomAt;

        public GridCell(Vector2Int pos)
        {
            position = pos;
            rotation = 0;
            layout = RoomLayout.DeadEnd;
            exitCount = 0;
            zone = ZoneLocation.LightContainmentZone;
            isBlocked = false;
            blockedByRoomAt = Vector2Int.zero;
        }

        public void SetExit(Direction direction, bool hasExit)
        {
            exits[(int)direction] = hasExit;
            RecalculateExitCount();
        }

        public bool HasExit(Direction direction)
        {
            return exits[(int)direction];
        }

        public void MarkAsBlocked(Vector2Int roomPosition)
        {
            isBlocked = true;
            blockedByRoomAt = roomPosition;
        }

        public void ClearBlocked()
        {
            isBlocked = false;
            blockedByRoomAt = Vector2Int.zero;
        }

        private void RecalculateExitCount()
        {
            exitCount = 0;
            for (int i = 0; i < 4; i++)
            {
                if (exits[i]) exitCount++;
            }

            layout = exitCount switch
            {
                1 => RoomLayout.DeadEnd,
                2 => DetermineCornerOrHallway(),
                3 => RoomLayout.Junction,
                4 => RoomLayout.Crossroads,
                _ => RoomLayout.DeadEnd
            };
        }

        private RoomLayout DetermineCornerOrHallway()
        {
            bool northSouth = exits[(int)Direction.North] && exits[(int)Direction.South];
            bool eastWest = exits[(int)Direction.East] && exits[(int)Direction.West];

            if (northSouth || eastWest)
            {
                return RoomLayout.Hallway;
            }

            return RoomLayout.Corner;
        }

        public int GetRotationForLayout()
        {
            if (exitCount == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (exits[i]) return i;
                }
            }
            else if (exitCount == 2)
            {
                if (exits[(int)Direction.North] && exits[(int)Direction.South])
                    return 0;
                if (exits[(int)Direction.East] && exits[(int)Direction.West])
                    return 1;
                if (exits[(int)Direction.North] && exits[(int)Direction.East])
                    return 0;
                if (exits[(int)Direction.East] && exits[(int)Direction.South])
                    return 1;
                if (exits[(int)Direction.South] && exits[(int)Direction.West])
                    return 2;
                if (exits[(int)Direction.West] && exits[(int)Direction.North])
                    return 3;
            }

            return 0;
        }
    }
}