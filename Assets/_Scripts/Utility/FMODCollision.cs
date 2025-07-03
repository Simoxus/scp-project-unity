using UnityEngine;
using FMODUnity;

public class FMODCollision : MonoBehaviour
{
    [SerializeField] private PlayerAccess player;

    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private EventReference fmodEvent;

    [Header("Camera Shake Settings")]
    [SerializeField] private bool shakePlayerCamera;
    [SerializeField] private float shakeMaxDistance = 10f;
    [SerializeField] private float shakeIntensityMultiplier = 0.5f;

    private void Awake()
    {
        // Get the Rigidbody component from the GameObject this script is attached to
        rigidBody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only proceed if this GameObject has a Rigidbody
        if (rigidBody == null)
        {
            return;
        }

        float collisionMagnitude = collision.relativeVelocity.magnitude * rigidBody.mass / 10f;

        if (shakePlayerCamera)
        {
            // Calculate distance to player
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            // Only shake if within the specified maximum distance
            if (distanceToPlayer <= shakeMaxDistance)
            {
                float distanceFactor = 1f - (distanceToPlayer / shakeMaxDistance);
                distanceFactor = Mathf.Clamp01(distanceFactor);

                // Apply a final impulse velocity that scales with collision magnitude and distance
                Vector3 finalImpulseVelocity = new Vector3(
                    collisionMagnitude, collisionMagnitude) * distanceFactor * shakeIntensityMultiplier;

                player.cameraImpulseSource.GenerateImpulseWithVelocity(finalImpulseVelocity);
            }
        }

        FMODHelper.PlayOneShot3D(fmodEvent, gameObject.transform.position);
    }
}