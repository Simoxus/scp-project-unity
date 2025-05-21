using UnityEngine;

namespace VLB
{
    public static class SpotLightHelper
    {
        public static float GetIntensity(Light light) { return light != null ? light.intensity : 0.0f; }
        public static float GetSpotAngle(Light light) { return light != null ? light.spotAngle : 0.0f; }
        public static float GetFallOffEnd(Light light) { return light != null ? light.range : 0.0f; }
    }
}
