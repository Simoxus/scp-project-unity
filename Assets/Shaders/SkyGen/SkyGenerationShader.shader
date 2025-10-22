Shader "Skybox/Night generator"
{

    Properties
    {
        _Seed("Seed", float) = 68.89
        _SkyColor("Sky color", Color) = (0.0, 0.06, 0.12, 1.0)

        [Header(Single star settings)]
        _Color("Stars color", Color) = (1.0, 1.0, 1.0, 1.0)
        [MinMax(0.4, 3.0)] _StarSizeRange("Star size range", Vector) = (0.6, 0.9, 0.0, 0.0)

        [Header(Density settings)]
        _Layers("Layers", Range(1.0, 5.0)) = 5
        _Density("Density", Range(0.5, 4.0)) = 2.28
        _DensityMod("Density modulation", Range(1.1, 3.0)) = 1.95

        [Header(Brightness settings)]
        _Brightness("Contrast", Range(0.0, 3.0)) = 2.89
        _BrightnessMod("Brightness modulation", Range(1.01, 4.0)) = 3.0

        [Header(Background fog settings)]
        [Toggle(ENABLE_BACKGROUND_NOISE)] _EnableBackgroundNoise("Enable galaxy noise", Int) = 1
        [Space] _SkyFogColor("Sky fog color", Color) = (0.0, 0.33, 0.34, 1.0)
        _NoiseDensity("Noise density", Range(1.0, 30.0)) = 8.6

        //x - iterations, y - alpha mod, z - size mod
        [Space][NoiseParameters] _NoiseParams("Background noise parameters", Vector) = (0.75, 6.0, 0.795, 2.08)

        //x - scale, y - iterations, z - alpha mod, w - size mod
        [Space][NoiseParameters] _NoiseMaskParams("Background mask parameters", Vector) = (0.33, 6.0, 0.628, 2.11)

        //x, y - smoothstep, z - border, w - power
        [Space][NoiseCutParameters] _NoiseMaskParams2("Background cut parameters", Vector) = (0.07, - 0.001, 0.51, 2.5)


        [Header(Moon settings)]
        [Toggle(ENABLE_MOON)] _EnableMoon("Enable moon", Int) = 1
        _MoonTex("Moon texture", 2D) = "black" {}
        _MoonTint("Moon tint color", Color) = (1.0, 1.0, 1.0, 1.0)
        //x, y - smoothstep, w - power, z - strength
        [Space][BloomParameters] _MoonBloomParams("Moon blooming parameters", Vector) = (10.0, - 1.0, 0.3, 5.3)
        [Space] _MoonSize("Moon size", Range(0.0, 1.0)) = 0.095

        [HideInInspector] _SunDisk ("Sun", Int) = 2
    }

    SubShader {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        Pass {

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _ ENABLE_BACKGROUND_NOISE
            #pragma shader_feature _ ENABLE_MOON

            #include "Includes/Utils.cginc"

            struct vertexData
            {
                float4 vertex : POSITION;
            };

            struct fragmentData
            {
                float4 clipPos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            fragmentData vert (vertexData vd)
            {
                fragmentData fd;
                fd.clipPos = UnityObjectToClipPos(vd.vertex);
                fd.worldPos = mul(unity_ObjectToWorld, vd.vertex).xyz;

                return fd;
            }

            #define PI 3.141592653589793238462

            float4 _Color;
            float4 _SkyColor;

            float2 _StarSizeRange;
            float _Density;
            float _Layers;
            float _DensityMod;
            float _BrightnessMod;
            float _Brightness;
            float _Seed;

            #if defined(ENABLE_BACKGROUND_NOISE)
            float4 _SkyFogColor;
            float _NoiseDensity;
            float4 _NoiseParams;
            float4 _NoiseMaskParams;
            float4 _NoiseMaskParams2;
            #endif

            #if defined(ENABLE_MOON)
            float _MoonSize;
            float4 _MoonTint;
            float4 _MoonBloomParams;
            sampler2D _MoonTex;
            #endif

            float stars(float3 rayDir, float sphereRadius, float sizeMod)
            {
                float3 spherePoint = rayDir * sphereRadius;

                float upAtan = atan2(spherePoint.y, length(spherePoint.xz)) + 4.0 * PI;

                float starSpaces = 1.0 / sphereRadius;
                float starSize = (sphereRadius * 0.0015) * fwidth(upAtan) * 1000.0 * sizeMod;
                upAtan -= fmod(upAtan, starSpaces) - starSpaces * 0.5;

                float numberOfStars = floor(sqrt(pow(sphereRadius, 2.0) * (1.0 - pow(sin(upAtan), 2.0))) * 3.0);

                float planeAngle = atan2(spherePoint.z, spherePoint.x) + 4.0 * PI;
                planeAngle = planeAngle - fmod(planeAngle, PI / numberOfStars);

                float2 randomPosition = hash22(float2(planeAngle, upAtan) + _Seed);

                float starLevel = sin(upAtan + starSpaces * (randomPosition.y - 0.5) * (1.0 - starSize)) * sphereRadius;
                float starDistanceToYAxis = sqrt(sphereRadius * sphereRadius - starLevel * starLevel);
                float starAngle = planeAngle + (PI * (randomPosition.x * (1.0 - starSize) + starSize * 0.5) / numberOfStars);
                float3 starCenter = float3(cos(starAngle) * starDistanceToYAxis, starLevel, sin(starAngle) * starDistanceToYAxis);

                float star = smoothstep(starSize, 0.0, distance(starCenter, spherePoint));

                return star;
            }

            float starModFromI(float i)
            {
                return lerp(_StarSizeRange.y, _StarSizeRange.x, smoothstep(1.0, _Layers, i));
            }

            half4 frag (fragmentData fd) : SV_Target
            {
                float3 rayDir = normalize(fd.worldPos - _WorldSpaceCameraPos);

                float star = 0.0;
                for(float i = 1.0; i <= _Layers; i += 1.0)
                {
                    star += stars(rayDir, _Density * pow(_DensityMod, i), starModFromI(i)) * (1.0 / pow(_BrightnessMod, i));
                }

                half4 skyColor = _SkyColor;

                #if defined(ENABLE_BACKGROUND_NOISE)
                float3 p = rayDir * _NoiseDensity + _Seed;
                float noise = layeredNoise13(p * _NoiseParams.x, _NoiseParams.y, _NoiseParams.z, _NoiseParams.w);
                float noise2 = layeredNoise13(p * _NoiseMaskParams.x * 0.05 + 21.32, _NoiseMaskParams.y, _NoiseMaskParams.z, _NoiseMaskParams.w);
                noise2 = pow(smoothstep(_NoiseMaskParams2.x, _NoiseMaskParams2.y, abs(noise2 - _NoiseMaskParams2.z)), _NoiseMaskParams2.w);
                skyColor += _SkyFogColor * noise2 * noise;
                #endif

                float4 sky = _Color * star * _Brightness + skyColor;

                #if defined(ENABLE_MOON)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 rightLightDir = - normalize(cross(lightDir, float3(0.0, 1.0, 0.0)));
                float3 upLightDir = - normalize(cross(rightLightDir, lightDir));

                float3x3 moonMatrix = float3x3(rightLightDir, upLightDir, lightDir);

                float3 moonUV = (mul(moonMatrix, rayDir)) / _MoonSize + float3(0.5, 0.5, 0.0);

                float4 moonCol = float4(0, 0, 0, 0);
                float moonBloom = pow(smoothstep(_MoonBloomParams.x, _MoonBloomParams.y, length(moonUV.xy - 0.5)), _MoonBloomParams.w) * _MoonBloomParams.z * (dot(rayDir, lightDir) * 0.5 + 0.5);

                if (moonUV.x > 0.0 && moonUV.x < 1.0 && moonUV.y > 0.0 && moonUV.y < 1.0 && moonUV.z > 0.0)
                {
                    moonCol = tex2D(_MoonTex, moonUV.xy) * _MoonTint;
                }

                sky = lerp(sky, moonCol, moonCol.a) + moonBloom * _MoonTint;
                #endif

                return sky + _WorldSpaceLightPos0.w;
            }
            ENDCG
        }
    }

    Fallback Off
    CustomEditor "NightSkyEditor"
}
