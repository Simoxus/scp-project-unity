using Facility.Persistence.Types;
using UnityEngine;

public class DoorPersistence
{
    private readonly BaseDoorController _doorController;
    private readonly string _doorID;
    private readonly Vector2Int _room1;
    private readonly Vector2Int _room2;

    public string DoorID => _doorID;

    public DoorPersistence(BaseDoorController controller, string doorID, Vector2Int room1, Vector2Int room2)
    {
        _doorController = controller;
        _doorID = doorID;
        _room1 = room1;
        _room2 = room2;
    }

    public DoorStateData GetDoorStateData()
    {
        if (_doorController == null) return null;

        var stateData = new DoorStateData(
            _doorID,
            _room1,
            _room2,
            _doorController.currentState == BaseDoorController.DoorState.Opened,
            _doorController.currentState == BaseDoorController.DoorState.Broken,
            _doorController.locked
        );

        // Save physics data for broken sliding doors
        if (_doorController.currentState == BaseDoorController.DoorState.Broken &&
            _doorController.doorFront != null &&
            _doorController.doorBack != null)
        {
            Rigidbody frontRb = _doorController.doorFront.GetComponent<Rigidbody>();
            Rigidbody backRb = _doorController.doorBack.GetComponent<Rigidbody>();

            stateData.brokenPhysics = new BrokenDoorPhysicsData(
                _doorController.doorFront,
                _doorController.doorBack,
                frontRb,
                backRb
            );
        }

        return stateData;
    }

    public void LoadDoorState(DoorStateData stateData)
    {
        if (_doorController == null) return;
        if (stateData == null) return;

        bool isRotatingDoor = _doorController.doorFront == null && _doorController.doorBack == null;

        _doorController.locked = stateData.isLocked;

        if (stateData.isBroken && !isRotatingDoor)
        {
            RestoreBrokenState(stateData.brokenPhysics);
        }
        else if (stateData.isOpen)
        {
            _doorController.OpenDoorImmediate();
        }
        else
        {
            _doorController.CloseDoorImmediate();
        }

        // Apply locked visuals for sliding doors
        if (stateData.isLocked && !isRotatingDoor)
        {
            _doorController.ApplyLockedStateFromPersistence();
        }
    }

    private void RestoreBrokenState(BrokenDoorPhysicsData physicsData)
    {
        if (_doorController == null) return;
        if (_doorController.doorFront == null || _doorController.doorBack == null) return;
        if (_doorController.currentState == BaseDoorController.DoorState.Broken) return;

        int debrisLayer = LayerMask.NameToLayer("Debris");
        _doorController.SetBrokenStateWithoutPhysics();

        // Restore front door physics
        if (physicsData != null && _doorController.doorFront != null)
        {
            _doorController.doorFront.transform.position = physicsData.frontPosition;
            _doorController.doorFront.transform.rotation = physicsData.frontRotation;
            _doorController.doorFront.layer = debrisLayer;

            Rigidbody frontRb = _doorController.doorFront.GetComponent<Rigidbody>();
            if (frontRb != null)
            {
                frontRb.isKinematic = false;
                frontRb.linearVelocity = physicsData.frontVelocity;
                frontRb.angularVelocity = physicsData.frontAngularVelocity;
            }
        }

        // Restore back door physics
        if (physicsData != null && _doorController.doorBack != null)
        {
            _doorController.doorBack.transform.position = physicsData.backPosition;
            _doorController.doorBack.transform.rotation = physicsData.backRotation;
            _doorController.doorBack.layer = debrisLayer;

            Rigidbody backRb = _doorController.doorBack.GetComponent<Rigidbody>();
            if (backRb != null)
            {
                backRb.isKinematic = false;
                backRb.linearVelocity = physicsData.backVelocity;
                backRb.angularVelocity = physicsData.backAngularVelocity;
            }
        }

        // Ignore collision between door pieces
        Collider frontCollider = _doorController.doorFront.GetComponent<Collider>();
        Collider backCollider = _doorController.doorBack.GetComponent<Collider>();
        if (frontCollider != null && backCollider != null)
        {
            Physics.IgnoreCollision(frontCollider, backCollider, true);
        }
    }
}