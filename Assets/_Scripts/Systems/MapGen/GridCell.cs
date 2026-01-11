using UnityEngine;

namespace Facility.Generation
{
    public class GridCell
    {
        public Vector2Int GridPosition { get; set; }
        public bool Exists { get; set; }
        public bool North { get; set; }
        public bool East { get; set; }
        public bool South { get; set; }
        public bool West { get; set; }
        public RoomLayout RoomLayout { get; set; }
        public float Angle { get; set; }
        public bool IsCheckpoint { get; set; }
        public bool IsOnCriticalPath { get; set; }
        public RoomData RoomData { get; set; }
        public RoomInstance RoomInstance { get; set; }

        public int ConnectionCount
        {
            get
            {
                int count = 0;
                if (North) count++;
                if (East) count++;
                if (South) count++;
                if (West) count++;
                return count;
            }
        }

        public int GScore { get; set; }
        public int HScore { get; set; }
        public int FScore => GScore + HScore;
        public GridCell Parent { get; set; }

        public GridCell(Vector2Int position)
        {
            GridPosition = position;
            Exists = false;
            North = false;
            East = false;
            South = false;
            West = false;
            RoomLayout = RoomLayout.DeadEnd;
            Angle = -1f;
            IsCheckpoint = false;
            IsOnCriticalPath = false;
            RoomData = null;
            RoomInstance = null;
            GScore = int.MaxValue;
            HScore = 0;
            Parent = null;
        }

        public void DetermineRoomLayout()
        {
            int connections = ConnectionCount;

            if (connections == 1)
            {
                RoomLayout = RoomLayout.DeadEnd;
            }
            else if (connections == 2)
            {
                if ((North && South) || (East && West))
                {
                    RoomLayout = RoomLayout.Hallway;
                }
                else
                {
                    RoomLayout = RoomLayout.Corner;
                }
            }
            else if (connections == 3)
            {
                RoomLayout = RoomLayout.Intersection;
            }
            else if (connections == 4)
            {
                RoomLayout = RoomLayout.Crossroads;
            }
        }

        public void DetermineAngle()
        {
            switch (RoomLayout)
            {
                case RoomLayout.DeadEnd:
                    if (North) Angle = 180f;
                    else if (East) Angle = 270f;
                    else if (South) Angle = 0f;
                    else if (West) Angle = 90f;
                    break;

                case RoomLayout.Hallway:
                    if (North && South) Angle = 0f;
                    else if (East && West) Angle = 90f;
                    break;

                case RoomLayout.Corner:
                    if (North && East) Angle = 270f;
                    else if (East && South) Angle = 0f;
                    else if (South && West) Angle = 90f;
                    else if (West && North) Angle = 180f;
                    break;

                case RoomLayout.Intersection:
                    if (!South) Angle = 0f;
                    else if (!West) Angle = 90f;
                    else if (!North) Angle = 180f;
                    else if (!East) Angle = 270f;
                    break;

                case RoomLayout.Crossroads:
                    Angle = 0f;
                    break;

                case RoomLayout.Checkpoint:
                    if (North && South) Angle = 0f;
                    else if (East && West) Angle = 90f;
                    break;

                default:
                    Angle = 0f;
                    break;
            }
        }

        public bool HasConnection(Direction direction)
        {
            return direction switch
            {
                Direction.North => North,
                Direction.East => East,
                Direction.South => South,
                Direction.West => West,
                _ => false
            };
        }

        public void SetConnection(Direction direction, bool value)
        {
            switch (direction)
            {
                case Direction.North: North = value; break;
                case Direction.East: East = value; break;
                case Direction.South: South = value; break;
                case Direction.West: West = value; break;
            }
        }

        public Direction GetOppositeDirection(Direction direction)
        {
            return direction switch
            {
                Direction.North => Direction.South,
                Direction.East => Direction.West,
                Direction.South => Direction.North,
                Direction.West => Direction.East,
                _ => Direction.North
            };
        }

        public Vector2Int GetNeighborPosition(Direction direction)
        {
            return direction switch
            {
                Direction.North => GridPosition + new Vector2Int(0, 1),
                Direction.East => GridPosition + new Vector2Int(1, 0),
                Direction.South => GridPosition + new Vector2Int(0, -1),
                Direction.West => GridPosition + new Vector2Int(-1, 0),
                _ => GridPosition
            };
        }
    }
}