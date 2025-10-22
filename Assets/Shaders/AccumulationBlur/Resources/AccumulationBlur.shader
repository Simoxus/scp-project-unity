Shader "Hidden/AccumulationBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurPower ("Blur Power", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100

        // Pass 1 : Blend previous frame with current
        Pass
        {
            Name "Accumulation Blend"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_AccumulationTex);
            SAMPLER(sampler_AccumulationTex);
            TEXTURE2D(_CurrentFrame);
            SAMPLER(sampler_CurrentFrame);

            float _BlurPower;
            float _Desaturation;
            float3 _TintColor;

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;

                // Sample current frame and previous accumulated frame
                half4 current = SAMPLE_TEXTURE2D(_CurrentFrame, sampler_CurrentFrame, uv);
                half4 accumulated = SAMPLE_TEXTURE2D(_AccumulationTex, sampler_AccumulationTex, uv);

                // Apply desaturation to accumulated frame (for B & W effect)
                if (_Desaturation > 0.0)
                {
                    float luminance = dot(accumulated.rgb, float3(0.299, 0.587, 0.114));
                    accumulated.rgb = lerp(accumulated.rgb, luminance.xxx, _Desaturation);
                }

                // Apply color tint to accumulated frame
                accumulated.rgb *= _TintColor;

                // Lerp between current and accumulated based on blur power
                half4 result = lerp(current, accumulated, _BlurPower);

                return result;
            }
            ENDHLSL
        }

        // Pass 2 : Copy result back to accumulation texture
        Pass
        {
            Name "Copy to Accumulation"

            ZTest Always
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half4 frag(Varyings input) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
            }
            ENDHLSL
        }
    }
}