using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gamepad rumble pulses for the accessibility layer (works with DualSense, Xbox and
/// any pad the Input System exposes). Haptics add a sensory channel that keyboards
/// lack — e.g. feeling the wall you just bumped into.
/// Maps to gameaccessibilityguidelines.com (Motor/Vision): "Provide vibration feedback"
/// and reinforces "Ensure no essential information is conveyed by visuals alone".
/// </summary>
public static class A11yHaptics
{
    /// <summary>Short rumble pulse; host runs the coroutine that stops the motors.</summary>
    public static void Pulse(MonoBehaviour host, float lowFrequency, float highFrequency, float duration)
    {
        var pad = Gamepad.current;
        if (pad == null || host == null || !host.isActiveAndEnabled) return;

        pad.SetMotorSpeeds(Mathf.Clamp01(lowFrequency), Mathf.Clamp01(highFrequency));
        host.StartCoroutine(StopAfter(pad, duration));
    }

    private static IEnumerator StopAfter(Gamepad pad, float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (pad != null && pad == Gamepad.current)
        {
            pad.ResetHaptics();
        }
    }

    /// <summary>
    /// Rumble parameters for taking damage, scaled by how big the hit was (0..1 of max
    /// health). Design rule from QA: strong haptics are reserved for tense moments —
    /// damage rumbles hard, everything ambient stays subtle.
    /// Returns (lowFrequency, highFrequency, duration).
    /// </summary>
    public static Vector3 DamagePulseParams(float damageFraction)
    {
        float t = Mathf.Clamp01(damageFraction);
        float low = Mathf.Clamp01(0.35f + t * 0.65f);   // grave: cuerpo del golpe
        float high = Mathf.Clamp01(0.15f + t * 0.55f);  // agudo: mordida
        float duration = 0.15f + t * 0.35f;
        return new Vector3(low, high, duration);
    }

    public static void PulseDamage(MonoBehaviour host, float damageFraction)
    {
        Vector3 p = DamagePulseParams(damageFraction);
        Pulse(host, p.x, p.y, p.z);
    }
}
