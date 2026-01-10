Shader "PostProcessing/AnaglyphEffect"
{
    Properties
    {
        _LeftTint ("Left Eye Tint", Color) = (1, 0, 0, 1)
        _RightTint ("Right Eye Tint", Color) = (0, 1, 1, 1)
        _Separation ("Channel Separation", Range(0, 0.02)) = 0.005
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        Cull Off 
        ZWrite Off 
        ZTest Always

        Pass
        {
            Name "Anaglyph"
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                half4 _LeftTint;
                half4 _RightTint;
                half _Separation;
            CBUFFER_END
            
            half4 frag(Varyings input) : SV_Target
            {
                float2 leftUV = input.texcoord + float2(-_Separation, 0);
                float2 rightUV = input.texcoord + float2(_Separation, 0);
                
                half4 leftSample = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, leftUV);
                half4 rightSample = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, rightUV);
                
                half3 leftChannel = leftSample.rgb * _LeftTint.rgb;
                half3 rightChannel = rightSample.rgb * _RightTint.rgb;
                
                half3 finalColor = leftChannel + rightChannel;
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}