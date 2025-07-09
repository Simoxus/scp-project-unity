using UnityEngine;

public static class ImpulseHelper
{
    // 1f, 5f
    private static Vector3 GenerateVelocity(float minRange, float maxRange)
    {
        Vector3 randomVelocity = Random.insideUnitSphere * Random.Range(minRange, maxRange);
        return randomVelocity;
    }
}
