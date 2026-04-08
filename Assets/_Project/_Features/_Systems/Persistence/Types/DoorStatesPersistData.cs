using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facility.Persistence.Types
{
    [Serializable]
    public class DoorStatesPersistData : IPersistData
    {
        public string PersistDataType => "doorstates";
        public string FileName => "doorstates.json";
        public bool SavePerSlot => true;
        public List<DoorStateData> doorStates;

        public DoorStatesPersistData()
        {
            doorStates = new List<DoorStateData>();
        }

        public string ToJson()
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                return JsonConvert.SerializeObject(this, settings);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }

        public static DoorStatesPersistData FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<DoorStatesPersistData>(json);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }
    }

    [Serializable]
    public class DoorStateData
    {
        // Door identification
        public string doorID;
        public int room1X;
        public int room1Y;
        public int room2X;
        public int room2Y;

        // Door state
        public bool isOpen;
        public bool isBroken;
        public bool isLocked;

        // Broken door physics data (only saved if isBroken = true)
        public BrokenDoorPhysicsData brokenPhysics;

        public DoorStateData() { }

        public DoorStateData(string id, Vector2Int room1, Vector2Int room2, bool open, bool broken, bool locked)
        {
            doorID = id;
            room1X = room1.x;
            room1Y = room1.y;
            room2X = room2.x;
            room2Y = room2.y;
            isOpen = open;
            isBroken = broken;
            isLocked = locked;
        }
    }

    [Serializable]
    public class BrokenDoorPhysicsData
    {
        // Front door piece
        public Vector3 frontPosition;
        public Quaternion frontRotation;
        public Vector3 frontVelocity;
        public Vector3 frontAngularVelocity;

        // Back door piece
        public Vector3 backPosition;
        public Quaternion backRotation;
        public Vector3 backVelocity;
        public Vector3 backAngularVelocity;

        public BrokenDoorPhysicsData() { }

        public BrokenDoorPhysicsData(
            GameObject doorFront,
            GameObject doorBack,
            Rigidbody frontRb,
            Rigidbody backRb)
        {
            if (doorFront != null)
            {
                frontPosition = doorFront.transform.position;
                frontRotation = doorFront.transform.rotation;
                if (frontRb != null)
                {
                    frontVelocity = frontRb.linearVelocity;
                    frontAngularVelocity = frontRb.angularVelocity;
                }
            }

            if (doorBack != null)
            {
                backPosition = doorBack.transform.position;
                backRotation = doorBack.transform.rotation;
                if (backRb != null)
                {
                    backVelocity = backRb.linearVelocity;
                    backAngularVelocity = backRb.angularVelocity;
                }
            }
        }
    }
}