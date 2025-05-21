Shader "Hidden/VLB_HD_URP_GPUInstancing"
{
    Properties
    {
        _ConeRadius("Cone Radius", Vector) = (0,0,0,0)
        _ConeGeomProps("Cone Geom Props", Vector) = (0,0,0,0)

        _ColorFlat("Color", Color) = (1,1,1,1)

        _HDRPExposureWeight("HDRP Exposure Weight", Range(0,1)) = 0

        _DistanceFallOff("Distance Fall Off", Vector) = (0,1,1,0)

        _NoiseVelocityAndScale("Noise Velocity And Scale", Vector) = (0,0,0,0)
        _NoiseParam("Noise Param", Vector) = (0,0,0,0)

        _BlendSrcFactor("BlendSrcFactor", Int) = 1 // One
        _BlendDstFactor("BlendDstFactor", Int) = 1 // One
        _ZTest("ZTest", Int) = 4 // LEqual


        // SD
        _ConeSlopeCosSin("Cone Slope Cos Sin", Vector) = (0,0,0,0)

        _AlphaInside("Alpha Inside", Range(0,1)) = 1
        _AlphaOutside("Alpha Outside", Range(0,1)) = 1

        _DistanceCamClipping("Camera Clipping Distance", Float) = 0.5
        _FadeOutFactor("FadeOutFactor", Float) = 1

        _AttenuationLerpLinearQuad("Lerp between attenuation linear and quad", Float) = 0.5
        _DepthBlendDistance("Depth Blend Distance", Float) = 2

        _FresnelPow("Fresnel Pow", Range(0,15)) = 1

        _GlareFrontal("Glare Frontal", Range(0,1)) = 0.5
        _GlareBehind("Glare from Behind", Range(0,1)) = 0.5
        _DrawCap("Draw Cap", Float) = 1

        _CameraParams("Camera Params", Vector) = (0,0,0,0)

        _DynamicOcclusionClippingPlaneWS("Dynamic Occlusion Clipping Plane WS", Vector) = (0,0,0,0)
        _DynamicOcclusionClippingPlaneProps("Dynamic Occlusion Clipping Plane Props", Float) = 0.25

        _DynamicOcclusionDepthTexture("DynamicOcclusionDepthTexture", 2D) = "white" {}
        _DynamicOcclusionDepthProps("DynamicOcclusionDepthProps", Vector) = (1, 1, 0.25, 1)

        _LocalForwardDirection("LocalForwardDirection", Vector) = (0,0,1)
        _TiltVector("TiltVector", Vector) = (0,0,0,0)
        _AdditionalClippingPlaneWS("AdditionalClippingPlaneWS", Vector) = (0,0,0,0)

        // HD
        _Intensity("Intensity", Range(0,8)) = 1
        _SideSoftness("SideSoftness", Range(0,15)) = 1
        _Jittering("Jittering", Vector) = (0,0,0,0)

        _CameraForwardOS("Camera Forward OS", Vector) = (0,0,0)
        _CameraForwardWS("Camera Forward WS", Vector) = (0,0,0)

        _TransformScale("Transform Scale", Vector) = (0,0,0)

        _CookieTexture("CookieTexture", 2D) = "white" {}
        _CookieProperties("CookieProperties", Vector) = (0,0,0,0)
        _CookiePosAndScale("CookiePosAndScale", Vector) = (0,0,1,1)

        _ShadowDepthTexture("ShadowDepthTexture", 2D) = "white" {}
        _ShadowProps("ShadowProps", Vector) = (1,1,1,1)
    }

    Category
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "DisableBatching" = "True" // disable dynamic batching which doesn't work neither with multiple materials nor material property blocks
        }

        Blend[_BlendSrcFactor][_BlendDstFactor]
        ZWrite Off
        ZTest[_ZTest]

        SubShader
        {
            Pass
            {
                Cull Front

                HLSLPROGRAM
                #if !defined(SHADER_API_METAL) // Removed shader model spec for Metal support https://github.com/keijiro/Cloner/commit/1120493ca2df265d450de3ec1b38a1d388468964
                #pragma target 3.0
                #endif
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog
                #pragma multi_compile_instancing
                #pragma multi_compile_local __ VLB_ALPHA_AS_BLACK
                #pragma multi_compile_local __ VLB_NOISE_3D
                #pragma multi_compile_local __ VLB_COLOR_GRADIENT_MATRIX_HIGH
                #pragma multi_compile_local VLB_ATTENUATION_LINEAR VLB_ATTENUATION_QUAD
                #pragma multi_compile_local __ VLB_SHADOW
                #pragma multi_compile_local __ VLB_COOKIE_1CHANNEL VLB_COOKIE_RGBA
                #pragma multi_compile_local VLB_RAYMARCHING_QUALITY_0 VLB_RAYMARCHING_QUALITY_1 VLB_RAYMARCHING_QUALITY_2


                #define VLB_SHADER_HD 1
                #define VLB_SRP_API 1
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

                #if VLB_RAYMARCHING_QUALITY_0
                #define VLB_RAYMARCHING_STEP_COUNT 5
                #endif
                #if VLB_RAYMARCHING_QUALITY_1
                #define VLB_RAYMARCHING_STEP_COUNT 10
                #endif
                #if VLB_RAYMARCHING_QUALITY_2
                #define VLB_RAYMARCHING_STEP_COUNT 20
                #endif


                #include "ShaderDefines.cginc"
                #include "ShaderProperties.cginc"
                #include "ShaderSpecificURP.cginc"
                #include "VolumetricLightBeamSharedHD.cginc"


                v2f vert(vlb_appdata v)         { return vertShared(v ); }
                half4 frag(v2f i) : SV_Target   { return fragShared(i ); }

                ENDHLSL
            }

        }
    }
}
