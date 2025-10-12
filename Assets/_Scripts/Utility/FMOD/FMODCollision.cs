using UnityEngine;
using FMODUnity;
using System;

public class FMODCollision : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private EventReference fmodEvent;

    [Header("Camera Shake Settings")]
    [SerializeField] private bool shakePlayerCamera;
    [SerializeField] private float shakeMaxDistance = 10f;
    [SerializeField] private float shakeIntensityMultiplier = 0.5f;

    private void Awake()
    {
        player = player != null ? player : Player.Instance;
        rigidBody = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rigidBody == null) return;
        if (transform == null || player == null || player.transform == null || player.cameraImpulseSource == null)
        {
            return;
        }

        float collisionMagnitude = collision.relativeVelocity.magnitude * rigidBody.mass / 10f;

        if (shakePlayerCamera)
        {
            try
            {
                // Calculate distance to player
                float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

                // Only shake if within the specified maximum distance
                if (distanceToPlayer <= shakeMaxDistance)
                {
                    float distanceFactor = 1f - (distanceToPlayer / shakeMaxDistance);
                    distanceFactor = Mathf.Clamp01(distanceFactor);

                    Vector3 finalImpulseVelocity = new Vector3(
                        collisionMagnitude, collisionMagnitude) * distanceFactor * shakeIntensityMultiplier;

                    player.cameraImpulseSource.GenerateImpulseWithVelocity(finalImpulseVelocity);
                }
            }
            catch (NullReferenceException ex)
            {
                Log.VerboseInfo($"Collision camera shake failed: {ex}");
            }
        }

        if (AudioManager.Instance)
        {
            FMODHelper.PlayOneShotWithDynamicOcclusion(
                fmodEvent,
                gameObject.transform.position,
                1.5f
            );
        }
        else
        {
            FMODHelper.PlayOneShot3D(fmodEvent, gameObject.transform.position);
        }
    }
}