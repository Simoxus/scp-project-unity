using UnityEngine;

namespace VLB
{
    [System.Serializable]
    public class RaymarchingQuality
    {
        public int uniqueID => _UniqueID;
        public bool hasValidUniqueID => _UniqueID >= 0;

        public string name;
        public int stepCount;

        public static RaymarchingQuality defaultInstance => ms_DefaultInstance;

        [SerializeField] int _UniqueID;
        static RaymarchingQuality ms_DefaultInstance = new RaymarchingQuality(-1);
        const int kRandomUniqueIdMinRange = 4; // default qualities have fixed ID from 1 to 3
        
        private RaymarchingQuality(int uniqueID)
        {
            _UniqueID = uniqueID;
            name = "New quality";
            stepCount = 10;
        }

        static public RaymarchingQuality New()
        {
            int id = Random.Range(kRandomUniqueIdMinRange, int.MaxValue);
            return new RaymarchingQuality(id);
        }

        static public RaymarchingQuality New(string name, int forcedUniqueID, int stepCount)
        {
            var instance = new RaymarchingQuality(forcedUniqueID);
            instance.name = name;
            instance.stepCount = stepCount;
            return instance;
        }

        static bool HasRaymarchingQualityWithSameUniqueID(RaymarchingQuality[] values, int id)
        {
            foreach (var qual in values)
            {
                if (qual != null && qual.uniqueID == id)
                    return true;
            }
            return false;
        }
    }
}

