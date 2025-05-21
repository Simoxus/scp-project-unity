using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VLB
{
    public static class UtilsBeamProps
    {
        public static bool CanChangeDuringPlaytime(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.trackChangesDuringPlaytime;
            return true;
        }

        public static Quaternion GetInternalLocalRotation(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.beamInternalLocalRotation;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.beamInternalLocalRotation;

            return Quaternion.identity;
        }

        public static void SetIntensityFromLight(VolumetricLightBeamAbstractBase self, bool fromLight)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) sd.intensityFromLight = fromLight;

            var hd = self as VolumetricLightBeamHD;
            if (hd) hd.useIntensityFromAttachedLightSpot = fromLight;
        }

        public static float GetThickness(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return Mathf.Clamp01(1 - (sd.fresnelPow / Consts.Beam.SD.FresnelPowMaxValue));

            var hd = self as VolumetricLightBeamHD;
            if (hd) return Mathf.Clamp01(1 - (hd.sideSoftness / Consts.Beam.HD.SideSoftnessMax));

            return 0f;
        }

        public static void SetThickness(VolumetricLightBeamAbstractBase self, float value)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd)
            {
                sd.fresnelPow = (1 - value) * Consts.Beam.SD.FresnelPowMaxValue;
                return;
            }

            var hd = self as VolumetricLightBeamHD;
            if (hd)
            {
                hd.sideSoftness =(1 - value) * Consts.Beam.HD.SideSoftnessMax;
            }
        }

        public static float GetFallOffEnd(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.fallOffEnd;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.fallOffEnd;

            return 0f;
        }


        public static ColorMode GetColorMode(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.usedColorMode;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.colorMode;

            return ColorMode.Flat;
        }

        public static Color GetColorFlat(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.color;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.colorFlat;

            return Color.white;
        }

        public static Gradient GetColorGradient(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.colorGradient;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.colorGradient;

            return null;
        }

        public static void SetColorFromLight(VolumetricLightBeamAbstractBase self, bool fromLight)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) sd.colorFromLight = fromLight;

            var hd = self as VolumetricLightBeamHD;
            if (hd) hd.colorFromLight = fromLight;
        }

        public static float GetConeAngle(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.coneAngle;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.coneAngle;

            return 0f;
        }

        public static void SetSpotAngleFromLight(VolumetricLightBeamAbstractBase self, bool fromLight)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) sd.spotAngleFromLight = fromLight;

            var hd = self as VolumetricLightBeamHD;
            if (hd) hd.useSpotAngleFromAttachedLightSpot = fromLight;
        }

        public static float GetConeRadiusStart(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.coneRadiusStart;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.coneRadiusStart;

            return 0f;
        }

        public static float GetConeRadiusEnd(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.coneRadiusEnd;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.coneRadiusEnd;

            return 0f;
        }

        public static int GetSortingLayerID(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.sortingLayerID;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.GetSortingLayerID();

            return 0;
        }

        public static int GetSortingOrder(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.sortingOrder;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.GetSortingOrder();

            return 0;
        }

        public static bool GetFadeOutEnabled(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.isFadeOutEnabled;

            return false;
        }

        public static float GetFadeOutEnd(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.fadeOutEnd;

            return 0f;
        }

        public static void SetFallOffEndFromLight(VolumetricLightBeamAbstractBase self, bool fromLight)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) sd.fallOffEndFromLight = fromLight;

            var hd = self as VolumetricLightBeamHD;
            if (hd) hd.useFallOffEndFromAttachedLightSpot = fromLight;
        }

        public static Dimensions GetDimensions(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.dimensions;

            var hd = self as VolumetricLightBeamHD;
            if (hd) return hd.GetDimensions();

            return Dimensions.Dim3D;
        }

        public static int GetGeomSides(VolumetricLightBeamAbstractBase self)
        {
            var sd = self as VolumetricLightBeamSD;
            if (sd) return sd.geomSides;

            return Config.Instance.sharedMeshSides;
        }

        public static AttenuationEquation ConvertAttenuation(AttenuationEquationHD value)
        {
            return (AttenuationEquation)value;
        }

        public static AttenuationEquationHD ConvertAttenuation(AttenuationEquation value)
        {
            if (value == AttenuationEquation.Blend)
                return AttenuationEquationHD.Linear;
            return (AttenuationEquationHD)value;
        }
    }
}
