using FMODUnity;
using UnityEngine;

public class FMODReverbZone : MonoBehaviour
{
    [Space]
    [SerializeField] private EventReference reverbSnapshot;
    [Range(0f, 50f)] public float blendDistance = 5f;

    [Header("Zone Shape")]
    public bool useBoxCollider = true;
    public Vector3 boxSize = new Vector3(10f, 10f, 10f);
    public float sphereRadius = 10f;

    private FMOD.Studio.EventInstance snapshotInstance;
    private Transform listenerTransform;
    private float currentIntensity = 0f;

    private void Start()
    {
        // Get the audio listener (usually on the camera)
        var listener = FindAnyObjectByType<StudioListener>();
        if (listener != null)
        {
            listenerTransform = listener.transform;
        }
        else
        {
            listenerTransform = Camera.main?.transform;
        }

        // Create the snapshot instance
        if (!reverbSnapshot.IsNull)
        {
            snapshotInstance = RuntimeManager.CreateInstance(reverbSnapshot);
        }
    }

    private void Update()
    {
        if (listenerTransform == null || snapshotInstance.isValid() == false)
            return;

        // Calculate distance and intensity
        float distance = CalculateDistanceToZone(listenerTransform.position);
        float targetIntensity = CalculateIntensity(distance);

        // Smooth transition
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * 5f);

        // Set snapshot intensity (0 = off, 1 = full effect)
        snapshotInstance.setParameterByName("intensity", currentIntensity);

        // Start/stop snapshot based on intensity
        if (currentIntensity > 0.01f)
        {
            FMOD.Studio.PLAYBACK_STATE state;
            snapshotInstance.getPlaybackState(out state);
            if (state != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                snapshotInstance.start();
            }
        }
        else
        {
            snapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    private float CalculateDistanceToZone(Vector3 listenerPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(listenerPos);

        if (useBoxCollider)
        {
            // Calculate the distance to box bounds
            Vector3 halfSize = boxSize * 0.5f;
            Vector3 closest = new Vector3(
                Mathf.Clamp(localPos.x, -halfSize.x, halfSize.x),
                Mathf.Clamp(localPos.y, -halfSize.y, halfSize.y),
                Mathf.Clamp(localPos.z, -halfSize.z, halfSize.z)
            );

            return Vector3.Distance(localPos, closest);
        }
        else
        {
            // Calculate the distance to sphere
            float distanceFromCenter = localPos.magnitude;
            return Mathf.Max(0f, distanceFromCenter - sphereRadius);
        }
    }

    private float CalculateIntensity(float distance)
    {
        if (distance <= 0f)
        {
            // Inside zone
            return 1f;
        }
        else if (distance >= blendDistance)
        {
            // Outside blend range
            return 0f;
        }
        else
        {
            // Linear falloff within blend distance
            return 1f - (distance / blendDistance);
        }
    }

    private void OnDestroy()
    {
        if (snapshotInstance.isValid())
        {
            snapshotInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            snapshotInstance.release();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (useBoxCollider)
        {
            Gizmos.DrawCube(Vector3.zero, boxSize);

            Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
        }
        else
        {
            Gizmos.DrawSphere(Vector3.zero, sphereRadius);
            Gizmos.DrawSphere(Vector3.zero, sphereRadius + blendDistance);
            Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
        }
    }
}