// Force isDepthBlendEnabled at true when GPU Instancing is enabled, to prevent from breaking the batch if 1 beam has it at 0 and 1 has it at > 0
#define FORCE_ENABLE_DEPTHBLEND_FOR_BATCHING

using UnityEngine;

namespace VLB
{
    public static class BatchingHelper
    {
        public static bool IsGpuInstancingEnabled(Material material)
        {
            Debug.Assert(material != null);
            return material.enableInstancing;
        }

        public static void SetMaterialProperties(Material material, bool enableGpuInstancing)
        {
            Debug.Assert(material != null);
            material.enableInstancing = enableGpuInstancing;
        }

        // SD
#if FORCE_ENABLE_DEPTHBLEND_FOR_BATCHING
        public static bool forceEnableDepthBlend { get {
                var renderingMode = Config.Instance.GetActualRenderingMode(ShaderMode.SD);
                return renderingMode == RenderingMode.GPUInstancing || renderingMode == RenderingMode.SRPBatcher;
            } }
#else
        public const bool forceEnableDepthBlend = false;
#endif

        static bool DoesRenderingModePreventBatching(ShaderMode shaderMode, ref string reasons)
        {
            var renderingMode = Config.Instance.GetActualRenderingMode(shaderMode);
            if (renderingMode != RenderingMode.GPUInstancing && renderingMode != RenderingMode.SRPBatcher)
            {
                reasons = string.Format("Current Rendering Mode is '{0}'. To enable batching, use '{1}'", renderingMode, RenderingMode.GPUInstancing);
                if (Config.Instance.renderPipeline != RenderPipeline.BuiltIn)
                    reasons += string.Format(" or '{0}'", RenderingMode.SRPBatcher);
                return true;
            }
            return false;
        }

#if UNITY_EDITOR
        static void CheckMaterialID(VolumetricLightBeamAbstractBase beamA, VolumetricLightBeamAbstractBase beamB, ref bool ret, ref string reasons)
        {
            var matIdA = beamA._EDITOR_GetInstancedMaterialID();
            var matIdB = beamB._EDITOR_GetInstancedMaterialID();
            if (matIdA >= 0 && matIdB >= 0)
            {
                bool haveSameMatID = matIdA == matIdB;
                if (haveSameMatID != ret)
                {
                    AppendErrorMessage(ref reasons, "UNKNOWN REASON: beams have not the same material ID");
                    Debug.LogErrorFormat("Beams {0} and {1} have not the same material ID while the reason is unknown", beamA.name, beamB.name);
                }

                if (!haveSameMatID)
                    ret = false;
            }
        }
#endif // UNITY_EDITOR

        public static bool CanBeBatched(VolumetricLightBeamSD beamA, VolumetricLightBeamSD beamB, ref string reasons)
        {
            if(DoesRenderingModePreventBatching(ShaderMode.SD, ref reasons))
                return false;

            bool ret = true;
            ret &= CanBeBatched(beamA, ref reasons);
            ret &= CanBeBatched(beamB, ref reasons);

#if UNITY_EDITOR
            bool shouldCheckMaterialID = ret;
#endif // UNITY_EDITOR

            if (Config.Instance.featureEnabledDynamicOcclusion)
            {
                if ((beamA.GetComponent<DynamicOcclusionAbstractBase>() == null) != (beamB.GetComponent<DynamicOcclusionAbstractBase>() == null))
                {
                    AppendErrorMessage(ref reasons, string.Format("{0}/{1}: dynamically occluded and non occluded beams cannot be batched together", beamA.name, beamB.name));
                    ret = false;
                }
            }

            if (Config.Instance.featureEnabledColorGradient != FeatureEnabledColorGradient.Off && beamA.colorMode != beamB.colorMode)
            {
                AppendErrorMessage(ref reasons, string.Format("'Color Mode' mismatch: {0} / {1}", beamA.colorMode, beamB.colorMode));
                ret = false;
            }

            if (beamA.blendingMode != beamB.blendingMode)
            {
                AppendErrorMessage(ref reasons, string.Format("'Blending Mode' mismatch: {0} / {1}", beamA.blendingMode, beamB.blendingMode));
                ret = false;
            }

            if (Config.Instance.featureEnabledNoise3D && beamA.isNoiseEnabled != beamB.isNoiseEnabled)
            {
                AppendErrorMessage(ref reasons, string.Format("'3D Noise' enabled mismatch: {0} / {1}", beamA.noiseMode, beamB.noiseMode));
                ret = false;
            }

            if (Config.Instance.featureEnabledDepthBlend && !forceEnableDepthBlend)
            {
#pragma warning disable 0162
                if ((beamA.depthBlendDistance > 0) != (beamB.depthBlendDistance > 0))
                {
                    AppendErrorMessage(ref reasons, string.Format("'Opaque Geometry Blending' mismatch: {0} / {1}", beamA.depthBlendDistance, beamB.depthBlendDistance));
                    ret = false;
                }
#pragma warning restore 0162
            }

            if (Config.Instance.featureEnabledShaderAccuracyHigh && beamA.shaderAccuracy != beamB.shaderAccuracy)
            {
                AppendErrorMessage(ref reasons, string.Format("'Shader Accuracy' mismatch: {0} / {1}", beamA.shaderAccuracy, beamB.shaderAccuracy));
                ret = false;
            }

#if UNITY_EDITOR
            if (shouldCheckMaterialID)
            {
                CheckMaterialID(beamA, beamB, ref ret, ref reasons);
            }
#endif // UNITY_EDITOR
            return ret;
        }

        public static bool CanBeBatched(VolumetricLightBeamSD beam, ref string reasons)
        {
            bool ret = true;

            if (Config.Instance.GetActualRenderingMode(ShaderMode.SD) == RenderingMode.GPUInstancing)
            {
                if (beam.geomMeshType != MeshType.Shared)
                {
                    AppendErrorMessage(ref reasons, string.Format("{0} is not using shared mesh", beam.name));
                    ret = false;
                }
            }

            if (Config.Instance.featureEnabledDynamicOcclusion && beam.GetComponent<DynamicOcclusionDepthBuffer>() != null)
            {
                AppendErrorMessage(ref reasons, string.Format("{0} is using the DynamicOcclusion DepthBuffer feature", beam.name));
                ret = false;
            }
            return ret;
        }

        // HD
        public static bool CanBeBatched(VolumetricLightBeamHD beamA, VolumetricLightBeamHD beamB, ref string reasons)
        {
            if (DoesRenderingModePreventBatching(ShaderMode.HD, ref reasons))
                return false;

            bool ret = true;
            ret &= CanBeBatched(beamA, ref reasons);
            ret &= CanBeBatched(beamB, ref reasons);

#if UNITY_EDITOR
            bool shouldCheckMaterialID = ret;
#endif // UNITY_EDITOR

            if (Config.Instance.featureEnabledColorGradient != FeatureEnabledColorGradient.Off && beamA.colorMode != beamB.colorMode)
            {
                AppendErrorMessage(ref reasons, string.Format("'Color Mode' mismatch: {0} / {1}", beamA.colorMode, beamB.colorMode));
                ret = false;
            }

            if (beamA.blendingMode != beamB.blendingMode)
            {
                AppendErrorMessage(ref reasons, string.Format("'Blending Mode' mismatch: {0} / {1}", beamA.blendingMode, beamB.blendingMode));
                ret = false;
            }

            if (beamA.attenuationEquation != beamB.attenuationEquation)
            {
                AppendErrorMessage(ref reasons, string.Format("'Attenuation Equation' mismatch: {0} / {1}", beamA.attenuationEquation, beamB.attenuationEquation));
                ret = false;
            }

            if (Config.Instance.featureEnabledNoise3D && beamA.isNoiseEnabled != beamB.isNoiseEnabled)
            {
                AppendErrorMessage(ref reasons, string.Format("'3D Noise' enabled mismatch: {0} / {1}", beamA.noiseMode, beamB.noiseMode));
                ret = false;
            }

            if (beamA.raymarchingQualityID != beamB.raymarchingQualityID)
            {
                AppendErrorMessage(ref reasons, string.Format("'Raymarching Quality' mismatch: {0} / {1}"
                    , Config.Instance.GetRaymarchingQualityForUniqueID(beamA.raymarchingQualityID).name
                    , Config.Instance.GetRaymarchingQualityForUniqueID(beamB.raymarchingQualityID).name));
                ret = false;
            }

#if UNITY_EDITOR
            if (shouldCheckMaterialID)
            {
                CheckMaterialID(beamA, beamB, ref ret, ref reasons);
            }
#endif // UNITY_EDITOR
            return ret;
        }

        public static bool CanBeBatched(VolumetricLightBeamHD beam, ref string reasons)
        {
            bool ret = true;

            if (Config.Instance.featureEnabledShadow && beam.GetAdditionalComponentShadow() != null)
            {
                AppendErrorMessage(ref reasons, string.Format("{0} is using the Shadow feature", beam.name));
                ret = false;
            }

            if (Config.Instance.featureEnabledCookie && beam.GetAdditionalComponentCookie() != null)
            {
                AppendErrorMessage(ref reasons, string.Format("{0} is using the Cookie feature", beam.name));
                ret = false;
            }

            return ret;
        }

        public static bool CanBeBatched(VolumetricLightBeamAbstractBase beamA, VolumetricLightBeamAbstractBase beamB, ref string reasons)
        {
            if (beamA is VolumetricLightBeamSD aSD && beamB is VolumetricLightBeamSD bSD)   return CanBeBatched(aSD, bSD, ref reasons);
            if (beamA is VolumetricLightBeamHD aHD && beamB is VolumetricLightBeamHD bHD)   return CanBeBatched(aHD, bHD, ref reasons);
            return false;
        }

        static void AppendErrorMessage(ref string message, string toAppend)
        {
            if (message != "") message += "\n";
            message += "- " + toAppend;
        }
    }
}
