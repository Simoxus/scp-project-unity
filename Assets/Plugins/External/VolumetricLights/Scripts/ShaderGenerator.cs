#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace VLB
{
    public class ShaderGenerator : ScriptableObject
    {
        [SerializeField] TextAsset m_Base = null;
        TextAsset textAssetBase { get { return m_Base; } }

        [SerializeField] TextAsset m_Pass = null;
        TextAsset textAssetPass { get { return m_Pass; } }

        [SerializeField] TextAsset m_IncludesBuiltin = null;
        TextAsset textAssetIncludesBuiltin { get { return m_IncludesBuiltin; } }

        [SerializeField] TextAsset m_IncludesURP = null;
        TextAsset textAssetIncludesURP { get { return m_IncludesURP; } }

        [SerializeField] TextAsset m_IncludesHDRP = null;
        TextAsset textAssetIncludesHDRP { get { return m_IncludesHDRP; } }

        static string GetShaderAssetName(ShaderMode shaderMode) { return string.Format("VLBGeneratedShader{0}.shader", shaderMode); }

        static string GetFolderOutputPath()
        {
            var assetPath = AssetDatabase.GetAssetPath(Instance);
            var fullPath = Path.GetFullPath(assetPath);
            var fullDir = Path.GetDirectoryName(fullPath);
            return fullDir; // full path to shader generator directory
        }

        public class ConfigProps
        {
            public RenderPipeline renderPipeline;
            public RenderingMode renderingMode;

            public bool dithering;
            public FeatureEnabledColorGradient colorGradient;
            public bool noise3D;

            public bool dynamicOcclusion;
            public bool depthBlend;
            public bool meshSkewing;
            public bool shaderAccuracyHigh;

            public bool cookie;
            public bool shadow;

            public RaymarchingQuality[] raymarchingQualities;
        }

        public static Shader Generate(ShaderMode shaderMode, ConfigProps configProps)
        {
            Debug.Assert(configProps != null);

            // The instance might not be accessible yet, when called from Config.OnEnable for instance if the Config is enabled before the ShaderGenerator.
            // In this case we don't generate the shader right away, we store the parameters instead and we'll generate the shader in ShaderGenerator.OnEnable.
            if (Instance == null)
            {
                AddGenerationParamOnEnable(shaderMode, configProps);
                return null;
            }

            return new GenShader(configProps).Generate(shaderMode);
        }

        static string LoadText(TextAsset textAsset)
        {
            Debug.Assert(textAsset != null, "Fail to load a TextAsset, please try to reinstall the VolumetricLightBeam plugin");
            return textAsset.text;
        }

        TextAsset GetTextAssetIncludes(RenderPipeline rp)
        {
            switch (rp)
            {
                case RenderPipeline.BuiltIn:
                    return textAssetIncludesBuiltin;
                case RenderPipeline.URP:
                    return textAssetIncludesURP;
                case RenderPipeline.HDRP:
                    return textAssetIncludesHDRP;
            }
            return null;
        }

        static bool IsFogSupported(RenderPipeline rp) { return rp != RenderPipeline.HDRP; }

        enum ShaderLangage { CG, HLSL }

        static ShaderLangage GetShaderLangage(RenderPipeline rp)
        {
            switch (rp)
            {
                case RenderPipeline.BuiltIn:
                    return ShaderLangage.CG;
                case RenderPipeline.URP:
                case RenderPipeline.HDRP:
                    return ShaderLangage.HLSL;
            }
            return ShaderLangage.CG;
        }

        static string GetShaderLangagePre (ShaderLangage lang) { return lang == ShaderLangage.CG ? "CGPROGRAM" : "HLSLPROGRAM"; }
        static string GetShaderLangagePost(ShaderLangage lang) { return lang == ShaderLangage.CG ? "ENDCG" : "ENDHLSL"; }

        static string NewLine(string str) { return "                " + str + System.Environment.NewLine; }
        static string NewDefine(string define, int value = 1) { return NewLine(string.Format("#define {0} {1}", define, value)); }
        static string NewInclude(string define) { return NewLine(string.Format("#include \"{0}\"", define)); }

        public class GenPass
        {
            CullMode m_CullMode;

            public GenPass(CullMode cullMode)
            {
                m_CullMode = cullMode;
            }

            static void AppendMultiCompile(ref string str, bool genDefaultVariant, params string[] options)
            {
#if UNITY_2019_1_OR_NEWER
                const string kPrefix = "#pragma multi_compile_local";
#else
                const string kPrefix = "#pragma multi_compile";
#endif
                var newLine = kPrefix;
                if(genDefaultVariant) newLine += " __";
                foreach (string opt in options) newLine += " " + opt;
                str += NewLine(newLine);
            }

            public string Generate(ShaderMode shaderMode, RenderPipeline rp, ConfigProps configProps, int passID, int passCount)
            {
                Debug.Assert(configProps != null);
                var code = LoadText(Instance.textAssetPass);

                code = code.Replace("{VLB_GEN_CULLING}", m_CullMode.ToString());
                code = code.Replace("{VLB_GEN_PRAGMA_INSTANCING}", configProps.renderingMode == RenderingMode.GPUInstancing ? "#pragma multi_compile_instancing" : "");
                code = code.Replace("{VLB_GEN_PRAGMA_FOG}", IsFogSupported(rp) ? "#pragma multi_compile_fog" : "");

                string multiCompileVariants = "";
                AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.AlphaAsBlack);
                if (configProps.noise3D) AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.Noise3D);
                switch(configProps.colorGradient)
                {
                    case FeatureEnabledColorGradient.HighOnly:      AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.ColorGradientMatrixHigh); break;
                    case FeatureEnabledColorGradient.HighAndLow:    AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.ColorGradientMatrixHigh, ShaderKeywords.ColorGradientMatrixLow); break;
                }

                if (shaderMode == ShaderMode.SD)
                {
                    if (configProps.depthBlend)             AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.SD.DepthBlend);
                    if (configProps.dynamicOcclusion)       AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.SD.OcclusionClippingPlane, ShaderKeywords.SD.OcclusionDepthTexture);
                    if (configProps.meshSkewing)            AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.SD.MeshSkewing);
                    if (configProps.shaderAccuracyHigh)     AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.SD.ShaderAccuracyHigh);
                }
                else if (shaderMode == ShaderMode.HD)
                {
                    AppendMultiCompile(ref multiCompileVariants, false, ShaderKeywords.HD.AttenuationLinear, ShaderKeywords.HD.AttenuationQuad);
                    if (configProps.shadow) AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.HD.Shadow);
                    if (configProps.cookie) AppendMultiCompile(ref multiCompileVariants, true, ShaderKeywords.HD.CookieSingleChannel, ShaderKeywords.HD.CookieRGBA);

                    if (configProps.raymarchingQualities != null)
                    {
                        Debug.Assert(configProps.raymarchingQualities.Length > 0);
                        var allParams = new string[configProps.raymarchingQualities.Length];
                        for (int i = 0; i < configProps.raymarchingQualities.Length; ++i)
                        {
                            allParams[i] = ShaderKeywords.HD.GetRaymarchingQuality(i);
                        }
                        AppendMultiCompile(ref multiCompileVariants, false, allParams);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Invalid RaymarchingQualities array during shader generation.");
                    }
                }

                code = code.Replace("{VLB_GEN_PRAGMA_MULTI_COMPILE_VARIANTS}", multiCompileVariants);

                var lang = GetShaderLangage(rp);
                code = code.Replace("{VLB_GEN_PROGRAM_PRE}", GetShaderLangagePre(lang));
                code = code.Replace("{VLB_GEN_PROGRAM_POST}", GetShaderLangagePost(lang));

                var passPre = "";
                if (shaderMode == ShaderMode.HD)
                {
                    passPre += NewDefine("VLB_SHADER_HD");

                    code = code.Replace("{VLB_GEN_INPUT_VS}", "");
                    code = code.Replace("{VLB_GEN_INPUT_FS}", "");
                }
                else
                {
                    if (passCount > 1)
                    {
                        code = code.Replace("{VLB_GEN_INPUT_VS}", string.Format(", {0}", passID));
                        code = code.Replace("{VLB_GEN_INPUT_FS}", string.Format(", {0}", passID));
                    }
                    else
                    {
                        code = code.Replace("{VLB_GEN_INPUT_VS}", ", v.texcoord.y");
                        code = code.Replace("{VLB_GEN_INPUT_FS}", ", i.cameraPosObjectSpace_outsideBeam.w");
                    }
                }

                if (rp != RenderPipeline.BuiltIn)
                {
                    passPre += NewDefine("VLB_SRP_API");

                    if (configProps.renderingMode == RenderingMode.SRPBatcher)
                    {
                        passPre += NewDefine("VLB_SRP_BATCHER");

                        if (rp == RenderPipeline.URP)
                        {
                            // force enable constant buffers to fix SRP Batcher support on Android
                            passPre += NewLine("#pragma enable_cbuffer");
                        }
                    }
                }

                if (configProps.dithering)
                {
                    passPre += NewDefine("VLB_DITHERING");
                }

                passPre += LoadText(Instance.GetTextAssetIncludes(rp));

                code = code.Replace("{VLB_GEN_PRE}", passPre);
                code = code.Replace("{VLB_GEN_RAYMARCHING_QUALITIES}", shaderMode == ShaderMode.HD ? GenerateRaymarchingQualities(configProps) : "");

                return code;
            }

            string GenerateRaymarchingQualities(ConfigProps enabledFeatures)
            {
                string str = "";
                for (int i = 0; i < enabledFeatures.raymarchingQualities.Length; ++i)
                {
                    str += NewLine(string.Format("#if {0}", ShaderKeywords.HD.GetRaymarchingQuality(i)));
                    str += NewDefine(ShaderKeywords.HD.RaymarchingStepCount, enabledFeatures.raymarchingQualities[i].stepCount);
                    str += NewLine("#endif");
                }
                return str;
            }
        }

        class GenShader
        {
            ConfigProps m_ConfigProps;
            List<GenPass> m_Passes = new List<GenPass>();

            public GenShader(ConfigProps configProps)
            {
                Debug.Assert(configProps != null);
                m_ConfigProps = configProps;

                AddPass(CullMode.Front);

                if (configProps.renderPipeline == RenderPipeline.BuiltIn && configProps.renderingMode == RenderingMode.MultiPass)
                    AddPass(CullMode.Back);
            }

            GenShader AddPass(CullMode cullMode)
            {
                m_Passes.Add(new GenPass(cullMode));
                return this;
            }

            public Shader Generate(ShaderMode shaderMode)
            {
                var shaderName = string.Format("Hidden/VLB_{0}_{1}_{2}", shaderMode, m_ConfigProps.renderPipeline, m_ConfigProps.renderingMode);
                var code = LoadText(Instance.textAssetBase);
                code = code.Replace("{VLB_GEN_SHADERNAME}", shaderName);

                {
                    var passes = "";
                    for (int i = 0; i < m_Passes.Count; ++i)
                        passes += m_Passes[i].Generate(shaderMode, m_ConfigProps.renderPipeline, m_ConfigProps, i, m_Passes.Count);
                    code = code.Replace("{VLB_GEN_PASSES}", passes);
                }

                {
                    var includes = "";
                    includes += NewInclude(GetRenderPipelineInclude(m_ConfigProps.renderPipeline));
                    includes += NewInclude(GetSharedInclude(shaderMode));
                    code = code.Replace("{VLB_GEN_SPECIFIC_INCLUDE}", includes);
                }

                // Write shader file
                var outputFolderPath = ShaderGenerator.GetFolderOutputPath();
                var outputFullPath = Path.Combine(outputFolderPath, GetShaderAssetName(shaderMode));
                try
                {
                    File.WriteAllText(outputFullPath, code);
                }
                catch (System.Exception ex)
                {
                    Debug.LogErrorFormat("Failed to generate shader {0} in folder '{1}':\n{2}", shaderName, outputFullPath, ex.Message);
                    return null;
                }
                AssetDatabase.Refresh();

                var shader = Shader.Find(shaderName);
                Debug.Assert(shader != null, string.Format("Failed to generate shader '{0}' at '{1}'", shaderName, outputFullPath));
                return shader;
            }

            string GetRenderPipelineInclude(RenderPipeline rp)
            {
                switch(rp)
                {
                    case RenderPipeline.BuiltIn:  return "ShaderSpecificBuiltin.cginc";
                    case RenderPipeline.HDRP:     return "ShaderSpecificHDRP.hlsl";
                    case RenderPipeline.URP:      return "ShaderSpecificURP.cginc";
                }
                return null;
            }

            string GetSharedInclude(ShaderMode shaderMode)
            {
                switch (shaderMode)
                {
                    case ShaderMode.SD: return "VolumetricLightBeamSharedSD.cginc";
                    case ShaderMode.HD: return "VolumetricLightBeamSharedHD.cginc";
                }
                return null;
            }
        }

        static ShaderGenerator FindInstance()
        {
            var assetGUIDs = AssetDatabase.FindAssets("ShaderGenerator");

            foreach (var guid in assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ShaderGenerator>(path);
                if (asset)
                    return asset;
            }
            return null;
        }

        // Store data to generate the shader on OnEnable
        static Hashtable ms_ConfigPropsOnEnable = null;
        static void AddGenerationParamOnEnable(ShaderMode shaderMode, ConfigProps configProps)
        {
            Debug.Assert(configProps != null);
            if(ms_ConfigPropsOnEnable == null)
                ms_ConfigPropsOnEnable = new Hashtable(2);
            ms_ConfigPropsOnEnable[shaderMode] = configProps;
        }

        void OnEnable()
        {
            if (ms_ConfigPropsOnEnable != null && Instance != null)
            {
                foreach(DictionaryEntry entry in ms_ConfigPropsOnEnable)
                {
                    var shaderMode = (ShaderMode)entry.Key;
                    var configProps = (ConfigProps)entry.Value;
                    Debug.Assert(configProps != null);
                    Generate(shaderMode, configProps);
                }

                ms_ConfigPropsOnEnable = null;
            }
        }

        // Singleton management
        static ShaderGenerator m_Instance = null;
        static ShaderGenerator Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = FindInstance();
                return m_Instance;
            }
        }
    }
}
#endif

