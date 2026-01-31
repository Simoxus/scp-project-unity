using UnityEngine;
using UnityEngine.AI;

public abstract class BaseNPC : MonoBehaviour
{
    [Space]
    [SerializeField] protected NavMeshAgent agent;

    [Header("Door Interaction")]
    [SerializeField] private float doorDetectionRadius = 2f;
    [SerializeField] private float doorCheckInterval = 1f;
    [SerializeField] private LayerMask doorLayer;

    private float _doorCheckTimer;
    private BaseDoorController _currentTargetDoor;

    protected virtual void Awake()
    {
        OnNPCAwake();
    }

    protected virtual void Update()
    {
        if (agent == null || !agent.enabled) return;

        UpdateDoorDetection();
    }

    private void UpdateDoorDetection()
    {
        if (!agent.hasPath) return;

        _doorCheckTimer -= Time.deltaTime;
        if (_doorCheckTimer <= 0f)
        {
            _doorCheckTimer = doorCheckInterval;
            CheckForDoorsInPath();
        }
    }

    private void CheckForDoorsInPath()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, doorDetectionRadius, doorLayer);

        foreach (var col in nearbyColliders)
        {
            var doorController = col.GetComponent<BaseDoorController>();
            if (doorController == null) continue;

            // Skip if door is already open, broken, or locked
            if (doorController.currentState == BaseDoorController.DoorState.Opened) continue;
            if (doorController.currentState == BaseDoorController.DoorState.Broken) continue;
            if (doorController.locked) continue;

            // Check if door is blocking our path
            if (IsDoorBlockingPath(doorController))
            {
                OpenDoor(doorController);
                break;
            }
        }
    }

    private bool IsDoorBlockingPath(BaseDoorController door)
    {
        if (agent.path == null || agent.path.corners.Length < 2) return false;

        Vector3 nextWaypoint = agent.path.corners[1];
        float distanceToDoor = Vector3.Distance(transform.position, door.transform.position);
        float distanceToWaypoint = Vector3.Distance(transform.position, nextWaypoint);

        return distanceToDoor < distanceToWaypoint * 0.8f;
    }

    private void OpenDoor(BaseDoorController door)
    {
        if (_currentTargetDoor == door) return;

        _currentTargetDoor = door;
        door.OpenDoor();
    }

    protected virtual void OnNPCAwake() { }
}