using UnityEngine;

public enum SonarTargetKind
{
    None,
    Door,
    Item
}

/// <summary>
/// Pure decision logic for the accessibility proximity sonar.
/// Kept static and side-effect free so it can be verified in batchmode without entering play mode.
/// </summary>
public static class SonarLogic
{
    public static bool TryChooseTarget(Vector3 origin, Collider[] hits, int count, out SonarTargetKind kind, out Collider chosen, out Vector3 position, out float distance, System.Func<Collider, bool> isReachable = null)
    {
        kind = SonarTargetKind.None;
        chosen = null;
        position = Vector3.zero;
        distance = float.PositiveInfinity;

        float closestSqr = float.PositiveInfinity;
        for (int i = 0; i < count; i++)
        {
            var hit = hits[i];
            if (hit == null) continue;
            if (!hit.TryGetComponent(out IInteractable interactable)) continue; // same detection rule as PlayerInteract

            float sqr = (origin - hit.transform.position).sqrMagnitude;
            if (sqr >= closestSqr) continue;
            if (isReachable != null && !isReachable(hit)) continue; // e.g. occluded by a wall (checked last: raycasts cost more than distance math)

            closestSqr = sqr;
            chosen = hit;
            position = hit.transform.position;
            kind = IsDoor(interactable) ? SonarTargetKind.Door : SonarTargetKind.Item;
        }

        if (kind == SonarTargetKind.None) return false;
        distance = Mathf.Sqrt(closestSqr);
        return true;
    }

    // All door/gate activators inherit BaseDoorActivator on dev; everything else counts as an item
    public static bool IsDoor(IInteractable interactable)
    {
        return interactable is BaseDoorActivator;
    }

    public static float IntervalForDistance(float distance, float radius, float minInterval, float maxInterval)
    {
        float t = Mathf.Clamp01(distance / Mathf.Max(radius, 0.01f));
        return Mathf.Lerp(minInterval, maxInterval, t);
    }

    /// <summary>
    /// TLOU-style height encoding for scan pings: targets above the player sound
    /// higher-pitched, below sound lower. Level ground = pitch 1.
    /// </summary>
    public static float PitchForHeightDelta(float heightDelta)
    {
        return Mathf.Clamp(1f + Mathf.Clamp(heightDelta * 0.12f, -0.4f, 0.6f), 0.6f, 1.6f);
    }

    /// <summary>
    /// A wall bump only counts when the surface is roughly vertical and the player is
    /// pushing mostly straight into it (sliding along a wall stays silent).
    /// </summary>
    public static bool ShouldBumpFeedback(Vector3 moveDirection, Vector3 surfaceNormal, float maxNormalY = 0.35f, float minFacingDot = 0.6f)
    {
        if (Mathf.Abs(surfaceNormal.y) > maxNormalY) return false; // floor or ceiling, not a wall

        Vector3 flatMove = new Vector3(moveDirection.x, 0f, moveDirection.z);
        Vector3 flatNormal = new Vector3(surfaceNormal.x, 0f, surfaceNormal.z);
        if (flatMove.sqrMagnitude < 0.0001f || flatNormal.sqrMagnitude < 0.0001f) return false;

        return Vector3.Dot(flatMove.normalized, -flatNormal.normalized) >= minFacingDot;
    }

    /// <summary>
    /// Blink warning fires once per blink cycle, only while eyes are open and the
    /// blink meter has drained past the warning threshold.
    /// </summary>
    public static bool ShouldWarnBlink(bool armed, bool isBlinking, float currentBlink, float warnThreshold)
    {
        return armed && !isBlinking && currentBlink <= warnThreshold;
    }
}
