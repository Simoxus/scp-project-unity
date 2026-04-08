using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Facility.Persistence.Types
{
    [Serializable]
    public class NavLinksPersistData : IPersistData
    {
        public string PersistDataType => "navlinks";
        public string FileName => "navlinks.json";
        public bool SavePerSlot => false;

        public List<NavLinkData> links;

        public NavLinksPersistData()
        {
            links = new List<NavLinkData>();
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

        public static NavLinksPersistData FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<NavLinksPersistData>(json);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return null;
            }
        }
    }

    [Serializable]
    public class NavLinkData
    {
        public SerializableVector2Int cell1;
        public SerializableVector2Int cell2;
        public SerializableVector3 startPoint;
        public SerializableVector3 endPoint;

        public NavLinkData() { }

        public NavLinkData(Vector2Int cell1Pos, Vector2Int cell2Pos, Vector3 start, Vector3 end)
        {
            cell1 = new SerializableVector2Int(cell1Pos);
            cell2 = new SerializableVector2Int(cell2Pos);
            startPoint = new SerializableVector3(start);
            endPoint = new SerializableVector3(end);
        }
    }

    [Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3() { }

        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }

        public static implicit operator Vector3(SerializableVector3 sv)
        {
            return sv?.ToVector3() ?? Vector3.zero;
        }

        public static implicit operator SerializableVector3(Vector3 v)
        {
            return new SerializableVector3(v);
        }
    }

    [Serializable]
    public class SerializableVector2Int
    {
        public int x;
        public int y;

        public SerializableVector2Int() { }

        public SerializableVector2Int(Vector2Int vector)
        {
            x = vector.x;
            y = vector.y;
        }

        public Vector2Int ToVector2Int()
        {
            return new Vector2Int(x, y);
        }

        public static implicit operator Vector2Int(SerializableVector2Int sv)
        {
            return sv?.ToVector2Int() ?? Vector2Int.zero;
        }

        public static implicit operator SerializableVector2Int(Vector2Int v)
        {
            return new SerializableVector2Int(v);
        }
    }
}