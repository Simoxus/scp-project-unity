using System;
using UnityEngine;

namespace Facility.Persistence
{
    [Serializable]
    public class PersistMetadata
    {
        public string saveName;
        public DateTime saveTime;
        public string seedString;
        public int seed;
        public int roomCount;
        public float playtime;
        public string playerPosition;

        public PersistMetadata()
        {
            saveTime = DateTime.Now;
        }

        public Vector3 GetPlayerPosition()
        {
            if (string.IsNullOrEmpty(playerPosition))
                return Vector3.zero;

            try
            {
                string cleaned = playerPosition.Trim('(', ')');
                string[] parts = cleaned.Split(',');
                if (parts.Length == 3)
                {
                    float x = float.Parse(parts[0].Trim());
                    float y = float.Parse(parts[1].Trim());
                    float z = float.Parse(parts[2].Trim());
                    return new Vector3(x, y, z);
                }
            }
            catch (Exception e)
            {
                Log.Warning($"Failed to parse player position: {e.Message}");
            }
            return Vector3.zero;
        }

        public string GetFormattedPlaytime()
        {
            TimeSpan time = TimeSpan.FromSeconds(playtime);
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes}m";
            else if (time.TotalMinutes >= 1)
                return $"{(int)time.TotalMinutes}m {time.Seconds}s";
            else
                return $"{time.Seconds}s";
        }

        public string GetFormattedSaveTime()
        {
            TimeSpan timeSince = DateTime.Now - saveTime;
            if (timeSince.TotalDays >= 1)
                return $"{(int)timeSince.TotalDays} day{((int)timeSince.TotalDays != 1 ? "s" : "")} ago";
            else if (timeSince.TotalHours >= 1)
                return $"{(int)timeSince.TotalHours} hour{((int)timeSince.TotalHours != 1 ? "s" : "")} ago";
            else if (timeSince.TotalMinutes >= 1)
                return $"{(int)timeSince.TotalMinutes} minute{((int)timeSince.TotalMinutes != 1 ? "s" : "")} ago";
            else
                return "Just now";
        }
    }
}