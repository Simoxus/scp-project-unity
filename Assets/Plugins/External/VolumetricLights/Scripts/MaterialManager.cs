using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace VLB
{
    public static class MaterialManager
    {
        public static MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

        public enum BlendingMode
        {
            Additive,
            SoftAdditive,
            TraditionalTransparency,
            Count
        }

        public enum ColorGradient
        {
            Off,
            MatrixLow,
            MatrixHigh,
            Count
        }

        public enum Noise3D
        {
            Off,
            On,
            Count
        }

        public static class SD
        {
            public enum DepthBlend
            {
                Off,
                On,
                Count
            }

            public enum DynamicOcclusion
            {
                Off,
                ClippingPlane,
                DepthTexture,
                Count
            }

            public enum MeshSkewing
            {
                Off,
                On,
                Count
            }

            public enum ShaderAccuracy
            {
                Fast,
                High,
                Count
            }
        }

        public static class HD
        {
            public enum Attenuation
            {
                Linear,
                Quadratic,
                Count
            }

            public enum Shadow
            {
                Off,
                On,
                Count
            }

            public enum Cookie
            {
                Off,
                SingleChannel,
                RGBA,
                Count
            }
        }

        static readonly UnityEngine.Rendering.BlendMode[] BlendingMode_SrcFactor = new UnityEngine.Rendering.BlendMode[(int)BlendingMode.Count]
        {
            UnityEngine.Rendering.BlendMode.One,                // Additive
            UnityEngine.Rendering.BlendMode.OneMinusDstColor,   // SoftAdditive
            UnityEngine.Rendering.BlendMode.SrcAlpha,           // TraditionalTransparency
        };

        static readonly UnityEngine.Rendering.BlendMode[] BlendingMode_DstFactor = new UnityEngine.Rendering.BlendMode[(int)BlendingMode.Count]
        {
            UnityEngine.Rendering.BlendMode.One,                // Additive
            UnityEngine.Rendering.BlendMode.One,                // SoftAdditive
            UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha,   // TraditionalTransparency
        };

        static readonly bool[] BlendingMode_AlphaAsBlack = new bool[(int)BlendingMode.Count]
        {
            true,   // Additive
            true,   // SoftAdditive
            false,  // TraditionalTransparency
        };

        interface IStaticProperties
        {
            int GetPropertiesCount();
            int GetMaterialID();
            void ApplyToMaterial(Material mat);
            ShaderMode GetShaderMode();
        }

        // STATIC PROPERTIES SD
        public struct StaticPropertiesSD : IStaticProperties
        {
            public ShaderMode GetShaderMode() { return ShaderMode.SD; }
            public static int staticPropertiesCount { get { return (int)BlendingMode.Count * (int)Noise3D.Count * (int)SD.DepthBlend.Count * (int)ColorGradient.Count * (int)SD.DynamicOcclusion.Count * (int)SD.MeshSkewing.Count * (int)SD.ShaderAccuracy.Count; } }
            public int GetPropertiesCount() { return staticPropertiesCount; }

            public BlendingMode blendingMode;
            public Noise3D noise3D;
            public SD.DepthBlend depthBlend;
            public ColorGradient colorGradient;
            public SD.DynamicOcclusion dynamicOcclusion;
            public SD.MeshSkewing meshSkewing;
            public SD.ShaderAccuracy shaderAccuracy;

            int blendingModeID      { get { return (int)blendingMode; } }
            int noise3DID           { get { return Config.Instance.featureEnabledNoise3D ? (int)noise3D : 0; } }
            int depthBlendID        { get { return Config.Instance.featureEnabledDepthBlend ? (int)depthBlend : 0; } }
            int colorGradientID     { get { return Config.Instance.featureEnabledColorGradient != FeatureEnabledColorGradient.Off ? (int)colorGradient : 0; } }
            int dynamicOcclusionID  { get { return Config.Instance.featureEnabledDynamicOcclusion ? (int)dynamicOcclusion : 0; } }
            int meshSkewingID       { get { return Config.Instance.featureEnabledMeshSkewing ? (int)meshSkewing : 0; } }
            int shaderAccuracyID    { get { return Config.Instance.featureEnabledShaderAccuracyHigh ? (int)shaderAccuracy : 0; } }

            public int GetMaterialID()
            {
                return (((((((blendingModeID)
                        * (int)Noise3D.Count + noise3DID)
                        * (int)SD.DepthBlend.Count + depthBlendID)
                        * (int)ColorGradient.Count + colorGradientID)
                        * (int)SD.DynamicOcclusion.Count + dynamicOcclusionID)
                        * (int)SD.MeshSkewing.Count + meshSkewingID)
                        * (int)SD.ShaderAccuracy.Count + shaderAccuracyID)
                        ;
            }

            public void ApplyToMaterial(Material mat)
            {
                mat.SetKeywordEnabled(ShaderKeywords.AlphaAsBlack, BlendingMode_AlphaAsBlack[(int)blendingMode]);
                mat.SetKeywordEnabled(ShaderKeywords.ColorGradientMatrixLow,  colorGradient == ColorGradient.MatrixLow);
                mat.SetKeywordEnabled(ShaderKeywords.ColorGradientMatrixHigh, colorGradient == ColorGradient.MatrixHigh);
                mat.SetKeywordEnabled(ShaderKeywords.SD.DepthBlend, depthBlend == SD.DepthBlend.On);
                mat.SetKeywordEnabled(ShaderKeywords.Noise3D, noise3D == Noise3D.On);
                mat.SetKeywordEnabled(ShaderKeywords.SD.OcclusionClippingPlane, dynamicOcclusion == SD.DynamicOcclusion.ClippingPlane);
                mat.SetKeywordEnabled(ShaderKeywords.SD.OcclusionDepthTexture, dynamicOcclusion == SD.DynamicOcclusion.DepthTexture);
                mat.SetKeywordEnabled(ShaderKeywords.SD.MeshSkewing, meshSkewing == SD.MeshSkewing.On);
                mat.SetKeywordEnabled(ShaderKeywords.SD.ShaderAccuracyHigh, shaderAccuracy == SD.ShaderAccuracy.High);

                mat.SetBlendingMode(ShaderProperties.BlendSrcFactor, BlendingMode_SrcFactor[(int)blendingMode]);
                mat.SetBlendingMode(ShaderProperties.BlendDstFactor, BlendingMode_DstFactor[(int)blendingMode]);
                mat.SetZTest(ShaderProperties.ZTest, CompareFunction.LessEqual);
            }
        }

        // STATIC PROPERTIES HD
        public struct StaticPropertiesHD : IStaticProperties
        {
            public ShaderMode GetShaderMode() { return ShaderMode.HD; }
            public static int staticPropertiesCount { get { return (int)BlendingMode.Count * (int)HD.Attenuation.Count * (int)Noise3D.Count * (int)ColorGradient.Count * (int)HD.Shadow.Count * (int)HD.Cookie.Count * Config.Instance.raymarchingQualitiesCount; } }
            public int GetPropertiesCount() { return staticPropertiesCount; }

            public BlendingMode blendingMode;
            public HD.Attenuation attenuation;
            public Noise3D noise3D;
            public ColorGradient colorGradient;
            public HD.Shadow shadow;
            public HD.Cookie cookie;
            public int raymarchingQualityIndex;

            int blendingModeID { get { return (int)blendingMode; } }
            int attenuationID { get { return (int)attenuation; } }
            int noise3DID { get { return Config.Instance.featureEnabledNoise3D ? (int)noise3D : 0; } }
            int colorGradientID { get { return Config.Instance.featureEnabledColorGradient != FeatureEnabledColorGradient.Off ? (int)colorGradient : 0; } }
            int dynamicOcclusionID { get { return Config.Instance.featureEnabledShadow ? (int)shadow : 0; } }
            int cookieID { get { return Config.Instance.featureEnabledCookie ? (int)cookie : 0; } }
            int raymarchingQualityID { get { return raymarchingQualityIndex; } }

            public int GetMaterialID()
            {
                return (((((((blendingModeID)
                        * (int)HD.Attenuation.Count + attenuationID)
                        * (int)Noise3D.Count + noise3DID)
                        * (int)ColorGradient.Count + colorGradientID)
                        * (int)HD.Shadow.Count + dynamicOcclusionID)
                        * (int)HD.Cookie.Count + cookieID)
                        * (int)Config.Instance.raymarchingQualitiesCount + raymarchingQualityID)
                        ;
            }

            public void ApplyToMaterial(Material mat)
            {
                mat.SetKeywordEnabled(ShaderKeywords.AlphaAsBlack, BlendingMode_AlphaAsBlack[(int)blendingMode]);
                mat.SetKeywordEnabled(ShaderKeywords.HD.AttenuationLinear, attenuation == HD.Attenuation.Linear);
                mat.SetKeywordEnabled(ShaderKeywords.HD.AttenuationQuad, attenuation == HD.Attenuation.Quadratic);
                mat.SetKeywordEnabled(ShaderKeywords.ColorGradientMatrixLow, colorGradient == ColorGradient.MatrixLow);
                mat.SetKeywordEnabled(ShaderKeywords.ColorGradientMatrixHigh, colorGradient == ColorGradient.MatrixHigh);
                mat.SetKeywordEnabled(ShaderKeywords.Noise3D, noise3D == Noise3D.On);
                mat.SetKeywordEnabled(ShaderKeywords.HD.Shadow, shadow == HD.Shadow.On);
                mat.SetKeywordEnabled(ShaderKeywords.HD.CookieSingleChannel, cookie == HD.Cookie.SingleChannel);
                mat.SetKeywordEnabled(ShaderKeywords.HD.CookieRGBA, cookie == HD.Cookie.RGBA);

                for (int i = 0; i < Config.Instance.raymarchingQualitiesCount; ++i)
                    mat.SetKeywordEnabled(ShaderKeywords.HD.GetRaymarchingQuality(i), raymarchingQualityIndex == i);

                mat.SetBlendingMode(ShaderProperties.BlendSrcFactor, BlendingMode_SrcFactor[(int)blendingMode]);
                mat.SetBlendingMode(ShaderProperties.BlendDstFactor, BlendingMode_DstFactor[(int)blendingMode]);
                mat.SetZTest(ShaderProperties.ZTest, CompareFunction.Always);
            }
        }

        public static Material NewMaterialPersistent(Shader shader, bool gpuInstanced)
        {
            if (!shader)
            {
                Debug.LogError("Invalid VLB Shader. Please try to reset the VLB Config asset or reinstall the plugin.");
                return null;
            }

            var material = new Material(shader);
            BatchingHelper.SetMaterialProperties(material, gpuInstanced);
            return material;
        }

        class MaterialsGroup
        {
            public Material[] materials = null;

            public MaterialsGroup(int count)
            {
                Debug.Assert(count > 0);
                materials = new Material[count];
            }
        }

        // SD Instanced Material Access
        static Hashtable ms_MaterialsGroupSD = new Hashtable(1);
        public static Material GetInstancedMaterial(uint groupID, ref StaticPropertiesSD staticProps)
        {
            IStaticProperties iStaticProp = staticProps;
            return GetInstancedMaterial(ms_MaterialsGroupSD, groupID, ref iStaticProp);
        }

        // HD Instanced Material Access
        static Hashtable ms_MaterialsGroupHD = new Hashtable(1);
        public static Material GetInstancedMaterial(uint groupID, ref StaticPropertiesHD staticProps)
        {
            IStaticProperties iStaticProp = staticProps;
            return GetInstancedMaterial(ms_MaterialsGroupHD, groupID, ref iStaticProp);
        }

        static Material GetInstancedMaterial(Hashtable groups, uint groupID, ref IStaticProperties staticProps) // pass StaticProperties by ref to avoid per value arg copy
        {
            MaterialsGroup group = (MaterialsGroup)groups[groupID];
            if (group == null)
            {
                group = new MaterialsGroup(staticProps.GetPropertiesCount());
                groups[groupID] = group;
            }

            int matID = staticProps.GetMaterialID();
            Debug.Assert(matID < staticProps.GetPropertiesCount());
            Debug.Assert(group.materials != null);
            var mat = group.materials[matID];
            if (mat == null)
            {
                mat = Config.Instance.NewMaterialTransient(staticProps.GetShaderMode(), gpuInstanced:true);
                if(mat)
                {
                    group.materials[matID] = mat;
                    staticProps.ApplyToMaterial(mat);
                }
            }

            return mat;
        }

        // Enable or Disable the GPU Instancing batching optimization globally, at runtime.
        // The Config's asset must be configured with GPUInstancing RenderingMode, in order to properly generate GPUInstancing compatible shaders.
        public static bool EnableGPUInstancing(ShaderMode shaderMode, bool enabled)
        {
            if(Config.Instance.GetActualRenderingMode(shaderMode) != RenderingMode.GPUInstancing)
            {
                Debug.LogErrorFormat("To change GPU Instancing at runtime, the VLB plugin's config must be configured to use the GPUInstancing RenderingMode.");
                return false;
            }

            var materialGroup = shaderMode == ShaderMode.SD ? ms_MaterialsGroupSD : ms_MaterialsGroupHD;

            bool hasChanged = false;
            foreach (var value in materialGroup.Values)
            {
                MaterialsGroup group = (MaterialsGroup)value;
                if (group != null)
                {
                    foreach (var mat in group.materials)
                    {
                        if (mat)
                        {
                            mat.enableInstancing = enabled;
                            hasChanged = true;
                        }
                    }
                }
            }

            return hasChanged;
        }

        // Material Utils
        enum ZWrite { Off = 0, On = 1 }
        static void SetBlendingMode(this Material mat, int nameID, BlendMode value) { mat.SetInt(nameID, (int)value); }
        static void SetStencilRef(this Material mat, int nameID, int value) { mat.SetInt(nameID, value); }
        static void SetStencilComp(this Material mat, int nameID, CompareFunction value) { mat.SetInt(nameID, (int)value); }
        static void SetStencilOp(this Material mat, int nameID, StencilOp value) { mat.SetInt(nameID, (int)value); }
        static void SetCull(this Material mat, int nameID, CullMode value) { mat.SetInt(nameID, (int)value); }
        static void SetZWrite(this Material mat, int nameID, ZWrite value) { mat.SetInt(nameID, (int)value); }
        static void SetZTest(this Material mat, int nameID, CompareFunction value) { mat.SetInt(nameID, (int)value); }
    }
}
