using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Facility.Generation
{
    [CreateAssetMenu(fileName = "RoomPool", menuName = "Custom/Map Gen/Room Pool")]
    public class RoomPool : ScriptableObject
    {
        [SerializeField] private List<RoomData> normalRooms = new List<RoomData>();

        private Dictionary<RoomLayout, List<RoomData>> roomsByLayout = new Dictionary<RoomLayout, List<RoomData>>();
        private bool isInitialized = false;

        public IReadOnlyList<RoomData> NormalRooms => normalRooms;

        public void Initialize()
        {
            roomsByLayout.Clear();

            foreach (var room in normalRooms)
            {
                if (room == null) continue;

                if (!roomsByLayout.ContainsKey(room.Layout))
                {
                    roomsByLayout[room.Layout] = new List<RoomData>();
                }
                roomsByLayout[room.Layout].Add(room);
            }

            isInitialized = true;

            Log.VerboseInfo($"Initialized with {normalRooms.Count} rooms");
        }

        public RoomData GetRandomRoom(RoomLayout layout, int seed)
        {
            if (!isInitialized)
            {
                Initialize();
            }

            if (!roomsByLayout.ContainsKey(layout) || roomsByLayout[layout].Count == 0)
            {
                return null;
            }

            Random.InitState(seed);
            var rooms = roomsByLayout[layout];

            float totalWeight = rooms.Sum(r => r.SpawnWeight);
            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var room in rooms)
            {
                currentWeight += room.SpawnWeight;
                if (randomValue <= currentWeight)
                {
                    return room;
                }
            }

            return rooms[rooms.Count - 1];
        }

        public int GetRoomCount()
        {
            return normalRooms.Count;
        }
    }
}