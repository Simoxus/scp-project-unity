using UnityEngine;

public class LookAt : MonoBehaviour
{
    [Header("Main Settings")]
    public bool lookEnabled;
    public Transform target;

    [Header("Limits")]
    [Tooltip("Set to 0 to disable limit.")]
    public float maxLookDistance = 20f; // set to 0 to disable :0

    [Header("Rotation")]
    public bool lookAtTarget = true;
    public bool lockXRotation = false;
    public bool lockYRotation = false;
    public bool lockZRotation = false;
    public bool flipForwardDirection = false;
    public bool smoothRotation = true;
    public float rotationSpeed = 5f;

    [Header("Position Tracking")]
    public bool followTarget = false;
    public bool lockXPosition = false;
    public bool lockYPosition = false;
    public bool lockZPosition = false;

    private Vector3 _lastLookDirection = Vector3.zero;

    void LateUpdate()
    {
        if (!lookEnabled || target == null) return;
        // Throttle execution to every 4 frames
        if (Time.frameCount % 4 != (gameObject.GetInstanceID() & 3))
            return;

        HandleRotation();
        HandlePosition();
    }

    void HandleRotation()
    {
        if (!lookAtTarget)
            return;

        Vector3 toTarget = target.position - transform.position;
        float maxDistSqr = maxLookDistance > 0f ? maxLookDistance * maxLookDistance : float.PositiveInfinity;

        if (toTarget.sqrMagnitude > maxDistSqr)
            return;

        Vector3 lookDir = toTarget.normalized;

        if (!smoothRotation && (_lastLookDirection - lookDir).sqrMagnitude < 0.0001f)
            return;

        _lastLookDirection = lookDir;

        Quaternion lookRotation = Quaternion.LookRotation(lookDir);

        if (flipForwardDirection)
            lookRotation *= Quaternion.Euler(0, 180f, 0);

        Vector3 finalEuler = lookRotation.eulerAngles;

        if (lockXRotation) finalEuler.x = transform.eulerAngles.x;
        if (lockYRotation) finalEuler.y = transform.eulerAngles.y;
        if (lockZRotation) finalEuler.z = transform.eulerAngles.z;

        Quaternion finalRotation = Quaternion.Euler(finalEuler);

        transform.rotation = smoothRotation
            ? Quaternion.Slerp(transform.rotation, finalRotation, Time.deltaTime * rotationSpeed)
            : finalRotation;
    }

    void HandlePosition()
    {
        if (!followTarget)
            return;

        Vector3 newPos = target.position;

        if (lockXPosition) newPos.x = transform.position.x;
        if (lockYPosition) newPos.y = transform.position.y;
        if (lockZPosition) newPos.z = transform.position.z;

        transform.position = newPos;
    }

    public string GetRelativeDirection()
    {
        Vector3 toTarget = target.position - transform.position;
        float angle = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);

        if (angle < -45f && angle > -135f) return "Left";
        if (angle > 45f && angle < 135f) return "Right";
        if (Mathf.Abs(angle) <= 45f) return "Front";
        return "Back";
    }
}
