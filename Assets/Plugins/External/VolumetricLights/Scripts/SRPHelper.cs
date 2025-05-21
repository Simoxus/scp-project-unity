#if UNITY_2018_1_OR_NEWER
#define VLB_SRP_SUPPORT // Comment this to disable SRP support
#endif

#if VLB_SRP_SUPPORT
#if UNITY_2019_1_OR_NEWER
using AliasCurrentPipeline = UnityEngine.Rendering.RenderPipelineManager;
using AliasCameraEvents = UnityEngine.Rendering.RenderPipelineManager;
using CallbackType = System.Action<UnityEngine.Rendering.ScriptableRenderContext, UnityEngine.Camera>;
#else
using AliasCurrentPipeline = UnityEngine.Experimental.Rendering.RenderPipelineManager;
using AliasCameraEvents = UnityEngine.Experimental.Rendering.RenderPipeline;
using CallbackType = System.Action<UnityEngine.Camera>;
#endif // UNITY_2019_1_OR_NEWER
#endif // VLB_SRP_SUPPORT

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VLB
{
    public static class SRPHelper
    {

        public static string renderPipelineScriptingDefineSymbolAsString
        {
            get
            {
            #if VLB_BUILTIN
                return "VLB_BUILTIN";
            #elif VLB_URP
                return "VLB_URP";
            #elif VLB_HDRP
                return "VLB_HDRP";
            #else
                return "NO VLB Symbols";
            #endif
            }
        }

        public static RenderPipeline projectRenderPipeline
        {
            get
            {
                // cache the value to prevent from comparing strings (in ComputeRenderPipeline) each frame when SRPBatcher is enabled
                if (!m_IsRenderPipelineCached)
                {
                    m_RenderPipelineCached = ComputeRenderPipeline();
                    m_IsRenderPipelineCached = true;
                }
                return m_RenderPipelineCached;
            }
        }

        static bool m_IsRenderPipelineCached = false;
        static RenderPipeline m_RenderPipelineCached;

        static RenderPipeline ComputeRenderPipeline()
        {
#if VLB_SRP_SUPPORT
        var rp = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
        if (rp)
        {
            var name = rp.GetType().ToString();
            if (name.Contains("Universal"))     return RenderPipeline.URP;
            if (name.Contains("Lightweight"))   return RenderPipeline.URP;
            if (name.Contains("HD"))            return RenderPipeline.HDRP;
        }
#endif
            return RenderPipeline.BuiltIn;
        }

#if VLB_SRP_SUPPORT
    public static bool IsUsingCustomRenderPipeline()
    {
        // TODO: optimize and use renderPipelineType
        return AliasCurrentPipeline.currentPipeline != null || UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null;
    }

    public static void RegisterOnBeginCameraRendering(CallbackType cb)
    {
        if (IsUsingCustomRenderPipeline())
        {
            AliasCameraEvents.beginCameraRendering -= cb;
            AliasCameraEvents.beginCameraRendering += cb;
        }
    }

    public static void UnregisterOnBeginCameraRendering(CallbackType cb)
    {
        if (IsUsingCustomRenderPipeline())
        {
            AliasCameraEvents.beginCameraRendering -= cb;
        }
    }

    #if UNITY_EDITOR
        static void AppendScriptingDefineSymbols(string[] symbolsToRemove, string symbolToAdd)
        {
        #if UNITY_2021_2_OR_NEWER
            var namedBuildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            string[] scriptingDefineSymbolsArray = null;
            PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out scriptingDefineSymbolsArray);

            var scriptingDefineSymbolsList = new List<string>(scriptingDefineSymbolsArray);

            bool hasChanged = false;
            foreach (var toRem in symbolsToRemove)
            {
                hasChanged |= scriptingDefineSymbolsList.Remove(toRem);
            }

            if (!scriptingDefineSymbolsList.Contains(symbolToAdd))
            {
                scriptingDefineSymbolsList.Add(symbolToAdd);
                hasChanged = true;
            }

            if (hasChanged)
            {
                PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, scriptingDefineSymbolsList.ToArray());
            }
        #else
            var scriptingDefineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            if (scriptingDefineSymbols == null)
                scriptingDefineSymbols = "";

            bool hasChanged = false;
            foreach (var toRem in symbolsToRemove)
            {
                if (scriptingDefineSymbols.Contains(toRem))
                {
                    scriptingDefineSymbols = scriptingDefineSymbols.Replace(toRem, "");
                    hasChanged = true;
                }
            }

            if (!scriptingDefineSymbols.Contains(symbolToAdd))
            {
                if (scriptingDefineSymbols.Length > 0)
                    scriptingDefineSymbols += ";";
                scriptingDefineSymbols += symbolToAdd;
                hasChanged = true;
            }

            if (hasChanged)
            {
                scriptingDefineSymbols = scriptingDefineSymbols.Replace(";;", ";");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, scriptingDefineSymbols);
            }
        #endif // UNITY_2021_2_OR_NEWER

            if(hasChanged)
            {
                Debug.LogWarningFormat("Volumetric Light Beam: '{0}' symbol has been added to the Scripting Define Symbols. Please save your project!", symbolToAdd);
            }
        }

        public static void SetScriptingDefineSymbolsForRenderPipeline(RenderPipeline renderPipeline)
        {
            var allSymbols = new List<string> { "VLB_BUILTIN", "VLB_URP", "VLB_HDRP" };
            int enumValueCount = System.Enum.GetNames(typeof(RenderPipeline)).Length;
            Debug.Assert(allSymbols.Count == enumValueCount);
            string defineSymbol = allSymbols[(int)renderPipeline];      

            allSymbols.Remove(defineSymbol);

            AppendScriptingDefineSymbols(allSymbols.ToArray(), defineSymbol);
        }
    #endif // UNITY_EDITOR
#else
        public static bool IsUsingCustomRenderPipeline() { return false; }
#endif
    }
}

