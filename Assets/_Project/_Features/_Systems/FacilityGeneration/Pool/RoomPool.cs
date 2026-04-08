using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Facility.Generation
{
    [CreateAssetMenu(fileName = "_RoomPool_", menuName = "Custom/Map Gen/Room Pool")]
    public class RoomPool : ScriptableObject
    {
        [SerializeField] private List<RoomData> normalRooms = new List<RoomData>();

        private Dictionary<RoomLayout, List<RoomData>> _roomsByLayout = new Dictionary<RoomLayout, List<RoomData>>();
        private Dictionary<string, RoomData> _roomsByName = new Dictionary<string, RoomData>();
        private bool _isInitialized = false;

        public IReadOnlyList<RoomData> NormalRooms => normalRooms;

        public void Initialize()
        {
            _roomsByLayout.Clear();
            _roomsByName.Clear();

            foreach (var room in normalRooms)
            {
                if (room == null) continue;

                if (!_roomsByLayout.ContainsKey(room.Layout))
                {
                    _roomsByLayout[room.Layout] = new List<RoomData>();
                }
                _roomsByLayout[room.Layout].Add(room);

                if (!string.IsNullOrEmpty(room.RoomName))
                {
                    if (_roomsByName.ContainsKey(room.RoomName))
                    {
                        Log.VerboseWarning($"Duplicate room name found '{room.RoomName}' in '{name}' RoomPool");
                    }
                    else
                    {
                        _roomsByName[room.RoomName] = room;
                    }
                }
                else
                {
                    Log.Warning($"Room in pool '{name}' has no name set");
                }
            }

            _isInitialized = true;
        }

        public List<RoomData> GetRoomsForLayout(RoomLayout layout)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (!_roomsByLayout.ContainsKey(layout))
            {
                return new List<RoomData>();
            }

            return new List<RoomData>(_roomsByLayout[layout]);
        }

        public RoomData GetRandomRoom(RoomLayout layout, int seed)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (!_roomsByLayout.ContainsKey(layout) || _roomsByLayout[layout].Count == 0)
            {
                return null;
            }

            Random.InitState(seed);
            var rooms = _roomsByLayout[layout];

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

        public RoomData GetRoomByName(string roomName)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (string.IsNullOrEmpty(roomName))
            {
                return null;
            }

            if (_roomsByName.TryGetValue(roomName, out RoomData room))
            {
                return room;
            }

            Log.Warning($"Room '{roomName}' not found in pool");
            return null;
        }

        public bool HasRoom(string roomName)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            return !string.IsNullOrEmpty(roomName) && _roomsByName.ContainsKey(roomName);
        }

        public int GetRoomCount()
        {
            return normalRooms.Count;
        }
    }
}