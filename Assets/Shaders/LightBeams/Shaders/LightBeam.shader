Shader "Custom/LightBeam"
{
    Properties
    {
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)
        _Intensity("Intensity", Range(0, 5)) = 1
        _OverallAlpha("Beam Density", Range(0, 1)) = 0.3 
        
        [Header(Texture Settings)]
        [Toggle(USE_TEXTURE)] _UseTexture("Use Texture", Float) = 0
        [MainTexture] _MainTex("Albedo (RGB)", 2D) = "white" {}

        [Header(Fading)]
        _FadeDist("Fade Distance", Range(0, 50)) = 12
        _FadePower("Fade Power", Range(0.1, 10)) = 2
        _ViewPower("View Angle Power", Range(0.1, 10)) = 1
        _ViewMin("View Fade Min", Range(-2, 2)) = -0.5
        _ViewMax("View Fade Max", Range(-2, 5)) = 2.5

        [Header(Volumetric Noise)]
        [Toggle(USE_NOISE)] _UseNoise("Enable Volumetric Noise", Float) = 1
        _NoiseScale("Noise Scale", Range(0.1, 10)) = 2
        _NoiseIntensity("Noise Intensity", Range(0, 1)) = 0.3
        _NoiseSpeed("Noise Speed", Range(0, 2)) = 0.1
        _NoiseContrast("Noise Contrast", Range(0.1, 5)) = 1.5

        [Header(Dust Particles)]
        [Toggle(USE_DUST)] _UseDust("Enable Dust Particles", Float) = 0
        _DustDensity("Dust Density", Range(1, 50)) = 15
        _DustSize("Dust Size", Range(0.001, 0.1)) = 0.01
        _DustIntensity("Dust Intensity", Range(0, 5)) = 2
        _DustSpeed("Dust Speed", Range(0, 3)) = 0.3
        _DustDrift("Dust Drift Amount", Range(0, 2)) = 0.3
        _DustHeightRange("Dust Height Range", Range(0.1, 20)) = 5
        _DustRadialSpread("Dust Radial Spread", Range(0.1, 5)) = 1
        _DustOffset("Dust Position Offset", Vector) = (0,0,0,0)

        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 0 
    }

    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal"
        }

        Tags
        {
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Transparent+10"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
        }

        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull [_Cull]

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_local __ USE_TEXTURE
            #pragma multi_compile_local __ USE_NOISE
            #pragma multi_compile_local __ USE_DUST
            
            #pragma multi_compile_instancing
            #pragma multi_compile_fog 
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normal       : NORMAL;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 posWS        : TEXCOORD0;
                float3 modelPos     : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float2 uv           : TEXCOORD3;
                float fogFactor     : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Intensity;
                float4 _MainTex_ST;
                float _OverallAlpha;
                float _FadeDist;
                float _FadePower;
                float _ViewPower;
                float _ViewMin;
                float _ViewMax;
                float _Cull;
                float _NoiseScale;
                float _NoiseIntensity;
                float _NoiseSpeed;
                float _NoiseContrast;
                float _DustDensity;
                float _DustSize;
                float _DustIntensity;
                float _DustSpeed;
                float _DustDrift;
                float _DustHeightRange;
                float _DustRadialSpread;
                float3 _DustOffset;
            CBUFFER_END

            float3 hash33(float3 p)
            {
                p = frac(p * float3(0.1031, 0.1030, 0.0973));
                p += dot(p, p.yxz + 33.33);
                return frac((p.xxy + p.yxx) * p.zyx);
            }

            // Value noise
            float noise3D(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);

                f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);

                float n000 = hash33(i).x;
                float n100 = hash33(i + float3(1, 0, 0)).x;
                float n010 = hash33(i + float3(0, 1, 0)).x;
                float n110 = hash33(i + float3(1, 1, 0)).x;
                float n001 = hash33(i + float3(0, 0, 1)).x;
                float n101 = hash33(i + float3(1, 0, 1)).x;
                float n011 = hash33(i + float3(0, 1, 1)).x;
                float n111 = hash33(i + float3(1, 1, 1)).x;

                float nx00 = lerp(n000, n100, f.x);
                float nx10 = lerp(n010, n110, f.x);
                float nx01 = lerp(n001, n101, f.x);
                float nx11 = lerp(n011, n111, f.x);
                
                float nxy0 = lerp(nx00, nx10, f.y);
                float nxy1 = lerp(nx01, nx11, f.y);
                
                return lerp(nxy0, nxy1, f.z);
            }

            // Fractal noise
            float fractalNoise(float3 p)
            {
                float value = 0.0;
                float amplitude = 1.0;
                float frequency = 1.0;

                for (int i = 0; i < 3; i++)
                {
                    value += amplitude * noise3D(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }
                
                return value;
            }

            float dustParticles(float3 worldPos, float3 beamOrigin, float time)
            {
                float dust = 0.0;

                float3 relativePos = worldPos - beamOrigin - _DustOffset;
    
                // Only show dust in the top 30% of the beam (ignores fade distance)
                float distFromOrigin = length(relativePos);
                float maxDustDist = _FadeDist * 0.3; // Dust only in first 30% of beam
                if (distFromOrigin > maxDustDist)
                {
                    return 0.0;
                }
    
                float cellSize = 10.0 / _DustDensity;
                float3 cell = floor(relativePos / cellSize);

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            float3 currentCell = cell + float3(x, y, z);

                            float3 random = hash33(currentCell);
                            float3 offset = random - 0.5;

                            float sizeVar = lerp(0.85, 1.15, random.x);
                            float particleSize = _DustSize * sizeVar;

                            float speedVar = lerp(0.5, 1.5, random.y);
                            float fallSpeed = _DustSpeed * speedVar;

                            float fallTime = time * fallSpeed;
                            float heightOffset = frac(fallTime + random.z) * _DustHeightRange;
                            offset.y += heightOffset;

                            float swirlPhase = fallTime * 2.0 + random.x * 6.28318;
                            offset.x += sin(swirlPhase) * _DustDrift * 0.1;
                            offset.x += sin(fallTime + random.x * 6.28318) * _DustDrift * 0.1;
                            offset.z += cos(swirlPhase) * _DustDrift * 0.1;
                            offset.z += cos(fallTime * 0.7 + random.y * 6.28318) * _DustDrift * 0.1;

                            offset.xz *= _DustRadialSpread;

                            float3 particlePos = (currentCell + offset) * cellSize + beamOrigin + _DustOffset;
                            float dist = length(worldPos - particlePos);
                            float particle = smoothstep(particleSize * 2.0, 0.0, dist);

                            particle *= lerp(0.7, 1.3, random.z);
                            dust += particle;
                        }
                    }
                }
    
                return dust;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.modelPos = TransformObjectToWorld(float3(0, 0, 0));
                OUT.normalWS = TransformObjectToWorldNormal(IN.normal);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.fogFactor = ComputeFogFactor(OUT.positionHCS.z);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
    
                half4 color = _BaseColor * _Intensity;

                #if USE_TEXTURE
                    half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                    color.rgb *= texColor.rgb; 
                #endif

                // Distance fade
                float fadeStart = 0;
                float fadeEnd = _FadeDist;
                float3 dir2pos = IN.posWS - IN.modelPos;
                float d = length(dir2pos);
                float fade = 1.0 - saturate((d - fadeStart) / (fadeEnd - fadeStart));
                fade = pow(fade, _FadePower);

                // View angle fade
                float3 dir2Cam = normalize(_WorldSpaceCameraPos.xyz - IN.posWS);
                float3 normal = normalize(IN.normalWS);
                float dotVal = max(0.0, dot(normal, dir2Cam));
                float val = pow(dotVal, _ViewPower);
                fade *= max(0.0, lerp(_ViewMin, _ViewMax, val));

                float dustFade = fade;

                // Volumetric noise
                #if USE_NOISE
                    float3 noisePos = IN.posWS * _NoiseScale;
                    noisePos.y += _Time.y * _NoiseSpeed;
        
                    float noise = fractalNoise(noisePos);
                    noise = pow(noise, _NoiseContrast);
        
                    // Blend
                    float noiseFactor = lerp(1.0 - _NoiseIntensity, 1.0, noise);
                    fade *= noiseFactor;
                #endif

                // Dust
                #if USE_DUST
                    float dust = dustParticles(IN.posWS, IN.modelPos, _Time.y);
                    dust = saturate(dust * _DustIntensity);
                    float dustVisibility = pow(dustFade, 0.5);
        
                    color.rgb += dust * _BaseColor.rgb * dustVisibility;
                    fade = saturate(fade + dust * _BaseColor.a * 0.3);
                #endif

                half4 finalColor = half4(color.rgb, color.a * fade * _OverallAlpha);
                finalColor.rgb = MixFog(finalColor.rgb, IN.fogFactor);

                return finalColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}