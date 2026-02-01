Shader "Hidden/GaussianBlur"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        
        #define E 2.71828f
        
        CBUFFER_START(UnityPerMaterial)
            uint _GridSize;
            float _Spread;
        CBUFFER_END
        
        float gaussian(int x)
        {
            float sigmaSqu = _Spread * _Spread;
            return (1 / sqrt(TWO_PI * sigmaSqu)) * pow(E, -(x * x) / (2 * sigmaSqu));
        }
        ENDHLSL

        Pass
        {
            Name "Horizontal"
            ZTest Always
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_horizontal
            
            half4 frag_horizontal(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float3 col = float3(0.0f, 0.0f, 0.0f);
                float gridSum = 0.0f;
                
                int upper = ((_GridSize - 1) / 2);
                int lower = -upper;
                
                float2 texelSize = _BlitTexture_TexelSize.xy;
                
                for (int x = lower; x <= upper; ++x)
                {
                    float gauss = gaussian(x);
                    gridSum += gauss;
                    
                    float2 sampleUV = uv + float2(texelSize.x * x, 0.0f);
                    col += gauss * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, sampleUV).xyz;
                }
                
                col /= gridSum;
                return float4(col, 1.0f);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Vertical"
            ZTest Always
            ZWrite Off
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag_vertical
            
            half4 frag_vertical(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float3 col = float3(0.0f, 0.0f, 0.0f);
                float gridSum = 0.0f;
                
                int upper = ((_GridSize - 1) / 2);
                int lower = -upper;
                
                float2 texelSize = _BlitTexture_TexelSize.xy;
                
                for (int y = lower; y <= upper; ++y)
                {
                    float gauss = gaussian(y);
                    gridSum += gauss;
                    
                    float2 sampleUV = uv + float2(0.0f, texelSize.y * y);
                    col += gauss * SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, sampleUV).xyz;
                }
                
                col /= gridSum;
                return float4(col, 1.0f);
            }
            ENDHLSL
        }
    }
}