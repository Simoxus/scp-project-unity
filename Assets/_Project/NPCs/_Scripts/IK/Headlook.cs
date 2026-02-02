using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Headlook : MonoBehaviour
{
    [Space]
    public Transform headIKTarget;
    public Rig headRig;

    [Header("Settings")]
    public float radius = 10f;
    public float retargetSpeed = 5f;
    public float maxAngle = 90f;
    public float weightTransitionSpeed = 2f;

    public List<LookTarget> lookTargets = new List<LookTarget>();

    private float _radiusSqr;
    private Transform _currentTarget;
    private bool _isEnabled = true;

    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    private void Start()
    {
        _radiusSqr = radius * radius;
    }

    private void Update()
    {
        if (!_isEnabled || lookTargets == null || lookTargets.Count == 0)
        {
            DisableTracking();
            return;
        }

        Transform tracking = FindBestTarget();
        UpdateHeadTarget(tracking);
    }

    private Transform FindBestTarget()
    {
        Transform bestTarget = null;
        float closestAngle = maxAngle;
        int highestPriority = int.MinValue;

        foreach (LookTarget target in lookTargets)
        {
            if (target == null || !target.IsActive) continue;

            Vector3 delta = target.transform.position - transform.position;

            // Check if within radius
            if (delta.sqrMagnitude > _radiusSqr) continue;

            // Check angle
            float angle = Vector3.Angle(transform.forward, delta);
            if (angle > maxAngle) continue;

            // Prioritize by priority value, then by angle
            if (target.Priority > highestPriority ||
                (target.Priority == highestPriority && angle < closestAngle))
            {
                closestAngle = angle;
                highestPriority = target.Priority;
                bestTarget = target.transform;
            }
        }

        return bestTarget;
    }

    private void UpdateHeadTarget(Transform tracking)
    {
        float targetWeight = 0f;
        Vector3 targetPos = transform.position + (transform.forward * 2f);

        if (tracking != null)
        {
            targetPos = tracking.position;
            targetWeight = 1f;
            _currentTarget = tracking;
        }
        else
        {
            _currentTarget = null;
        }

        headIKTarget.position = Vector3.Lerp(headIKTarget.position, targetPos, Time.deltaTime * retargetSpeed);
        headRig.weight = Mathf.Lerp(headRig.weight, targetWeight, Time.deltaTime * weightTransitionSpeed);
    }

    private void DisableTracking()
    {
        Vector3 defaultPos = transform.position + (transform.forward * 2f);
        headIKTarget.position = Vector3.Lerp(headIKTarget.position, defaultPos, Time.deltaTime * retargetSpeed);
        headRig.weight = Mathf.Lerp(headRig.weight, 0f, Time.deltaTime * weightTransitionSpeed);
    }

    public void AddLookTarget(LookTarget target)
    {
        if (target != null && !lookTargets.Contains(target))
        {
            lookTargets.Add(target);
        }
    }

    public void RemoveLookTarget(LookTarget target)
    {
        if (lookTargets.Contains(target))
        {
            lookTargets.Remove(target);
        }
    }

    public void ClearLookTargets()
    {
        lookTargets.Clear();
    }
}