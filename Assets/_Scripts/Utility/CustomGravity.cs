using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravity : MonoBehaviour
{
    [SerializeField] private float gravityMultiplier = 1.0f;
    [SerializeField] private bool useGlobalDirection = true;
    [SerializeField] private Vector3 customGravityDirection = Vector3.down;

    private Rigidbody _rigidbody; // Reference to the Rigidbody component

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>(); // Get the Rigidbody component once at the start
    }

    void FixedUpdate()
    {
        // FixedUpdate is called at a fixed interval, ideal for physics calculations

        Vector3 currentGravityDirection;

        if (useGlobalDirection)
        {
            // Use the normalized (direction-only) global gravity vector
            currentGravityDirection = Physics.gravity.normalized;
        }
        else
        {
            // Use the normalized custom direction defined in the Inspector
            currentGravityDirection = customGravityDirection.normalized;
        }

        float gravitationalStrength = Physics.gravity.magnitude * gravityMultiplier;
        _rigidbody.AddForce(currentGravityDirection * gravitationalStrength * _rigidbody.mass, ForceMode.Force);
    }

    // Optional: Draw a gizmo in the editor to visualize the custom gravity direction
    void OnDrawGizmosSelected()
    {
        if (!enabled || _rigidbody == null) return; // Only draw if script is enabled and Rigidbody exists

        Gizmos.color = Color.cyan; // Set the color of the gizmo

        // Get the current position of the Rigidbody or Transform
        Vector3 origin = _rigidbody.position;

        // Determine the direction of gravity to draw
        Vector3 directionToDraw;
        if (useGlobalDirection)
        {
            directionToDraw = Physics.gravity.normalized;
        }
        else
        {
            directionToDraw = customGravityDirection.normalized;
        }

        // Draw a ray representing the gravity direction from the object's center
        Gizmos.DrawRay(origin, directionToDraw * 2f); // Ray extends 2 units in the gravity direction
        // Draw a small sphere at the end of the ray for better visibility
        Gizmos.DrawSphere(origin + directionToDraw * 2f, 0.1f);
    }
}
