Shader "Custom/Glass"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 0.5)
        _OverlayTexture ("Overlay Texture", 2D) = "white" {}
        _OverlayIntensity ("Overlay Intensity", Range(0, 1)) = 0.5
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _NormalStrength ("Normal Strength", Range(0, 2)) = 1.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95
        _RefractionStrength ("Refraction Strength", Range(0, 0.5)) = 0.02
        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _FresnelPower ("Fresnel Power", Range(0, 5)) = 3.0
        _FresnelDistanceFade ("Fresnel Distance Fade", Range(0, 100)) = 20.0
        _BlurAmount ("Blur Amount", Range(0, 10)) = 2.0
        [Enum(Off,0,Back,2)] _Cull ("Cull Mode", Float) = 2
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        LOD 100
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float4 screenPos : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                float fogFactor : TEXCOORD5;
                float distanceToCamera : TEXCOORD6;
                float3 tangentWS : TEXCOORD7;
                float3 bitangentWS : TEXCOORD8;
            };
            
            TEXTURE2D(_OverlayTexture);
            SAMPLER(sampler_OverlayTexture);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _OverlayTexture_ST;
                float4 _NormalMap_ST;
                float _OverlayIntensity;
                float _NormalStrength;
                float _Smoothness;
                float _RefractionStrength;
                float _Metallic;
                float _FresnelPower;
                float _FresnelDistanceFade;
                float _BlurAmount;
                float _Cull;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionCS = posInputs.positionCS;
                output.positionWS = posInputs.positionWS;
                output.normalWS = normInputs.normalWS;
                output.tangentWS = normInputs.tangentWS;
                output.bitangentWS = normInputs.bitangentWS;
                output.uv = TRANSFORM_TEX(input.uv, _OverlayTexture);
                output.screenPos = ComputeScreenPos(posInputs.positionCS);
                output.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                output.fogFactor = ComputeFogFactor(posInputs.positionCS.z);
                output.distanceToCamera = length(_WorldSpaceCameraPos - posInputs.positionWS);
                
                return output;
            }
            
            // Gaussian blur for glass effect
            half3 SampleBlurredBackground(float2 uv, float blurAmount)
            {
                if (blurAmount < 0.01)
                {
                    return SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv).rgb;
                }
    
                half3 color = half3(0.0, 0.0, 0.0);
                float pixelSize = 0.001 * blurAmount;
                
                // Gaussian kernel weights (13-tap)
                const float weights[13] = {
                    0.0561, 0.1353, 0.2781, 0.4868, 0.7261,
                    0.9231, 1.0, 0.9231, 0.7261,
                    0.4868, 0.2781, 0.1353, 0.0561
                };
                
                float totalWeight = 0.0;
                
                // Horizontal and vertical passes combined
                [unroll]
                for (int i = -6; i <= 6; i++)
                {
                    float weight = weights[i + 6];
                    totalWeight += weight * 2.0; // Count for both axes
                    
                    // Horizontal samples
                    color += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, 
                        saturate(uv + float2(i * pixelSize, 0))).rgb * weight;
                    
                    // Vertical samples
                    color += SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, 
                        saturate(uv + float2(0, i * pixelSize))).rgb * weight;
                }
                
                return color / max(totalWeight, 0.0001);
            }
            
            half4 frag(Varyings input, bool isFrontFace : SV_IsFrontFace) : SV_Target
            {
                // Normalize vectors
                float3 normalWS = normalize(input.normalWS);
                float3 tangentWS = normalize(input.tangentWS);
                float3 bitangentWS = normalize(input.bitangentWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // Flip normals for back faces when double-sided
                if (!isFrontFace)
                {
                    normalWS = -normalWS;
                    tangentWS = -tangentWS;
                }
                
                // Sample and apply normal map
                half3 normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv), _NormalStrength);
                float3x3 TBN = float3x3(tangentWS, bitangentWS, normalWS);
                normalWS = normalize(mul(normalTS, TBN));
                
                // Calculate distance fade for fresnel (0 = far away, 1 = close)
                float distanceFade = saturate(1.0 - (input.distanceToCamera / _FresnelDistanceFade));
                
                // Calculate fresnel effect with distance fade
                float fresnelRaw = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                float fresnel = fresnelRaw * distanceFade;
                
                // Sample overlay texture
                half4 overlayColor = SAMPLE_TEXTURE2D(_OverlayTexture, sampler_OverlayTexture, input.uv);
                
                // Calculate refraction with proper UV clamping
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float2 refractionOffset = normalWS.xy * _RefractionStrength;
                float2 refractedUV = screenUV + refractionOffset;
                
                // Clamp UVs to prevent sampling outside screen bounds
                refractedUV = saturate(refractedUV);
                
                // Sample blurred background for glass effect
                half3 refractionColor = SampleBlurredBackground(refractedUV, _BlurAmount);
                
                // Get main light
                Light mainLight = GetMainLight();
                half3 lightColor = mainLight.color;
                float3 lightDir = normalize(mainLight.direction);
                
                // Simple lighting calculation
                float NdotL = saturate(dot(normalWS, lightDir));
                half3 lighting = lightColor * NdotL;
                
                // Specular highlight (also affected by distance)
                float3 halfDir = normalize(lightDir + viewDirWS);
                float specular = pow(saturate(dot(normalWS, halfDir)), _Smoothness * 128.0) * distanceFade;
                
                // Combine overlay with glass base color
                half3 glassColor = lerp(_BaseColor.rgb, overlayColor.rgb, _OverlayIntensity);
                
                // Mix refraction with glass color based on fresnel
                half3 finalColor = lerp(refractionColor * glassColor, glassColor, fresnel * 0.5);
                
                // Add lighting and specular
                finalColor += lighting * 0.3 + specular * fresnel;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                // Calculate final alpha (also reduced at distance)
                float finalAlpha = _BaseColor.a + fresnel * 0.3;
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    
    FallBack Off
}
