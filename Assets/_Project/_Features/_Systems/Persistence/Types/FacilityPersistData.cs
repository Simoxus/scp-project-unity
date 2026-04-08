using Facility.Generation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facility.Persistence.Types
{
    [Serializable]
    public class FacilityPersistData : IPersistData
    {
        public string PersistDataType => "facility";
        public string FileName => "facility.json";
        public bool SavePerSlot => false;

        public string saveName;
        public DateTime saveTime;
        public string seedString;
        public int seed;
        public int gridWidth;
        public int gridHeight;
        public List<GridCellData> cells;
        public Vector2Int startCellPosition;
        public string gameVersion;

        public FacilityPersistData()
        {
            cells = new List<GridCellData>();
            saveTime = DateTime.Now;
            gameVersion = Application.version;
        }

        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }

        public static FacilityPersistData FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<FacilityPersistData>(json);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }
    }

    [Serializable]
    public class GridCellData
    {
        public Vector2Int position;
        public int rotation;
        public ZoneLocation zone;
        public RoomLayout layout;
        public string assignedRoomID;
        public bool isBlocked;
        public bool isCheckpoint;
        public int exitMask;

        public GridCellData() { }

        public GridCellData(GridCell cell)
        {
            position = cell.position;
            rotation = cell.rotation;
            zone = cell.zone;
            layout = cell.layout;
            isBlocked = cell.isBlocked;
            isCheckpoint = cell.isCheckpoint;

            exitMask = 0;
            if (cell.HasExit(Direction.North)) exitMask |= 1 << 0;
            if (cell.HasExit(Direction.East)) exitMask |= 1 << 1;
            if (cell.HasExit(Direction.South)) exitMask |= 1 << 2;
            if (cell.HasExit(Direction.West)) exitMask |= 1 << 3;

            if (cell.assignedRoom != null)
            {
                assignedRoomID = cell.assignedRoom.RoomName;
                if (string.IsNullOrEmpty(assignedRoomID))
                {
                    Log.Warning($"Room at {cell.position} has no room name");
                }
            }
            else
            {
                assignedRoomID = "";
            }
        }

        public void ApplyToCell(GridCell cell, FacilityGeneratorSettings settings)
        {
            cell.rotation = rotation;
            cell.zone = zone;
            cell.layout = layout;
            cell.isBlocked = isBlocked;
            cell.isCheckpoint = isCheckpoint;

            cell.SetExit(Direction.North, (exitMask & (1 << 0)) != 0);
            cell.SetExit(Direction.East, (exitMask & (1 << 1)) != 0);
            cell.SetExit(Direction.South, (exitMask & (1 << 2)) != 0);
            cell.SetExit(Direction.West, (exitMask & (1 << 3)) != 0);

            if (!string.IsNullOrEmpty(assignedRoomID))
            {
                // Check if this is the starting room (not in a pool)
                if (settings.StartingRoom != null && assignedRoomID == settings.StartingRoom.RoomName)
                {
                    cell.assignedRoom = settings.StartingRoom;
                    Log.VerboseInfo($"Assigned starting room '{assignedRoomID}' to cell at {position}");
                    return;
                }

                // Otherwise, look in the zone's room pool
                var zoneSettings = settings.GetZoneSettings(zone);
                if (zoneSettings != null)
                {
                    if (zoneSettings.roomPool == null)
                    {
                        Log.Error($"Zone {zone} has no room pool configured");
                        return;
                    }

                    cell.assignedRoom = zoneSettings.roomPool.GetRoomByName(assignedRoomID);
                    if (cell.assignedRoom == null)
                    {
                        Log.Error($"Could not find room '{assignedRoomID}' in zone {zone} pool. Cell at {position} will have no room.");
                    }
                    else
                    {
                        Log.VerboseInfo($"Assigned room '{assignedRoomID}' to cell at {position}");
                    }
                }
            }
        }
    }
}