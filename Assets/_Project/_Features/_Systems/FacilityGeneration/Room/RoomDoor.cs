using Cysharp.Threading.Tasks;
using Facility.Persistence.Types;
using System;
using UnityEngine;

namespace Facility.Generation
{
    public class RoomDoor : MonoBehaviour
    {
        private Vector2Int _room1Position;
        private Vector2Int _room2Position;
        private bool _isOpen;
        private bool _isInitialized;
        private BaseDoorController _doorController;
        private DoorPersistence _doorPersistence;

        public event Action<bool> OnDoorStateChanged;

        public Vector2Int Room1Position => _room1Position;
        public Vector2Int Room2Position => _room2Position;
        public bool IsOpen => _isOpen;
        public string DoorID => GetDoorID(_room1Position, _room2Position);
        public BaseDoorController DoorController => _doorController;

        private void Awake()
        {
            _doorController = GetComponent<BaseDoorController>();
        }

        public void Initialize(Vector2Int room1, Vector2Int room2, bool startsOpen)
        {
            _room1Position = room1;
            _room2Position = room2;
            _isOpen = startsOpen;
            _isInitialized = true;

            if (_doorController != null)
            {
                string doorID = GetDoorID(room1, room2);
                _doorPersistence = new DoorPersistence(_doorController, doorID, room1, room2);
            }

            if (_doorController != null)
            {
                _doorController.startOpened = startsOpen;

                if (startsOpen && _doorController.currentState == BaseDoorController.DoorState.Closed)
                {
                    _doorController.OpenDoor();
                }
            }
        }

        public void SetDoorState(bool open, bool notify = true)
        {
            if (!_isInitialized)
            {
                Log.VerboseWarning("Attempted to set state on uninitialized door");
                return;
            }

            if (_isOpen && !open)
            {
                Log.VerboseInfo($"Door {DoorID} is already open and cannot be closed");
                return;
            }

            if (_isOpen == open) return;

            _isOpen = open;

            if (_doorController != null)
            {
                if (open && _doorController.currentState == BaseDoorController.DoorState.Closed)
                {
                    _doorController.OpenDoor();
                }
            }

            if (notify)
            {
                OnDoorStateChanged?.Invoke(_isOpen);
            }
        }

        public void Open()
        {
            SetDoorState(true);
        }

        public void Toggle()
        {
            if (!_isOpen)
            {
                SetDoorState(true);
            }
            else
            {
                Log.VerboseInfo($"Door {DoorID} is already open");
            }
        }

        public void Break()
        {
            if (_doorController != null)
            {
                SetDoorState(true, notify: false);
                _doorController.BreakDoor().Forget();
                OnDoorStateChanged?.Invoke(true);
            }
        }

        public DoorStateData GetDoorStateData()
        {
            if (_doorPersistence != null)
            {
                return _doorPersistence.GetDoorStateData();
            }

            Log.Warning($"Door {DoorID} has no DoorPersistence instance; cannot save state");
            return null;
        }

        public void LoadDoorState(DoorStateData stateData)
        {
            if (stateData == null) return;

            _room1Position = new Vector2Int(stateData.room1X, stateData.room1Y);
            _room2Position = new Vector2Int(stateData.room2X, stateData.room2Y);
            _isInitialized = true;

            if (_doorPersistence == null && _doorController != null)
            {
                string doorID = GetDoorID(_room1Position, _room2Position);
                _doorPersistence = new DoorPersistence(_doorController, doorID, _room1Position, _room2Position);
            }

            if (_doorPersistence != null)
            {
                _doorPersistence.LoadDoorState(stateData);
                _isOpen = stateData.isOpen || stateData.isBroken;
            }
            else
            {
                Log.Warning($"Door {DoorID} has no DoorPersistence instance; cannot load state");
            }

            Log.VerboseInfo($"RoomDoor loaded: {DoorID}, Open={stateData.isOpen}, Broken={stateData.isBroken}");
        }

        public static string GetDoorID(Vector2Int pos1, Vector2Int pos2)
        {
            int minX = Mathf.Min(pos1.x, pos2.x);
            int minY = Mathf.Min(pos1.y, pos2.y);
            int maxX = Mathf.Max(pos1.x, pos2.x);
            int maxY = Mathf.Max(pos1.y, pos2.y);
            return $"{minX},{minY}_{maxX},{maxY}";
        }

        private void OnDestroy()
        {
            OnDoorStateChanged = null;
        }
    }
}