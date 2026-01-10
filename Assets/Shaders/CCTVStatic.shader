Shader "Custom/CCTVStatic"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)

        _StaticIntensity ("Static Intensity", Range(0, 1)) = 0.5
        _StaticSpeed ("Static Speed", Range(0, 10)) = 8
        _StaticScale ("Static Scale", Range(1, 200)) = 100
        _StaticContrast ("Static Contrast", Range(1, 5)) = 2

        _Scanlines ("Scanlines Intensity", Range(0, 1)) = 0.4
        _ScanlineCount ("Scanline Count", Range(200, 2000)) = 800
        _ScanlineSpeed ("Scanline Scroll Speed", Range(-5, 5)) = 0.5
 
        _Brightness ("Brightness", Range(0, 2)) = 0.8
        _Contrast ("Contrast", Range(0, 3)) = 1.2

        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.01)) = 0.002
        _Distortion ("Horizontal Distortion", Range(0, 0.05)) = 0.01
        _DistortionSpeed ("Distortion Speed", Range(0, 5)) = 1

        _Vignette ("Vignette", Range(0, 1)) = 0.3
        _Alpha ("Alpha", Range(0, 1)) = 1
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _StaticIntensity)
                UNITY_DEFINE_INSTANCED_PROP(float, _StaticSpeed)
                UNITY_DEFINE_INSTANCED_PROP(float, _StaticScale)
                UNITY_DEFINE_INSTANCED_PROP(float, _StaticContrast)
                UNITY_DEFINE_INSTANCED_PROP(float, _Scanlines)
                UNITY_DEFINE_INSTANCED_PROP(float, _ScanlineCount)
                UNITY_DEFINE_INSTANCED_PROP(float, _ScanlineSpeed)
                UNITY_DEFINE_INSTANCED_PROP(float, _Brightness)
                UNITY_DEFINE_INSTANCED_PROP(float, _Contrast)
                UNITY_DEFINE_INSTANCED_PROP(float, _ChromaticAberration)
                UNITY_DEFINE_INSTANCED_PROP(float, _Distortion)
                UNITY_DEFINE_INSTANCED_PROP(float, _DistortionSpeed)
                UNITY_DEFINE_INSTANCED_PROP(float, _Vignette)
                UNITY_DEFINE_INSTANCED_PROP(float, _Alpha)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // Noise
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.13);
                p3 += dot(p3, p3.yzx + 3.333);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                float staticIntensity = UNITY_ACCESS_INSTANCED_PROP(Props, _StaticIntensity);
                float staticSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _StaticSpeed);
                float staticScale = UNITY_ACCESS_INSTANCED_PROP(Props, _StaticScale);
                float staticContrast = UNITY_ACCESS_INSTANCED_PROP(Props, _StaticContrast);
                float scanlines = UNITY_ACCESS_INSTANCED_PROP(Props, _Scanlines);
                float scanlineCount = UNITY_ACCESS_INSTANCED_PROP(Props, _ScanlineCount);
                float scanlineSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _ScanlineSpeed);
                float brightness = UNITY_ACCESS_INSTANCED_PROP(Props, _Brightness);
                float contrast = UNITY_ACCESS_INSTANCED_PROP(Props, _Contrast);
                float chromaticAberration = UNITY_ACCESS_INSTANCED_PROP(Props, _ChromaticAberration);
                float distortion = UNITY_ACCESS_INSTANCED_PROP(Props, _Distortion);
                float distortionSpeed = UNITY_ACCESS_INSTANCED_PROP(Props, _DistortionSpeed);
                float vignette = UNITY_ACCESS_INSTANCED_PROP(Props, _Vignette);
                float alpha = UNITY_ACCESS_INSTANCED_PROP(Props, _Alpha);
                float4 tintColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                
                float2 uv = i.uv;
                
                // Wobble
                float wobble = sin(uv.y * 10 + _Time.y * distortionSpeed) * distortion;
                wobble += sin(uv.y * 23.4 + _Time.y * distortionSpeed * 0.7) * distortion * 0.5;
                uv.x += wobble;
                
                // Chromatic aberration
                float r = tex2D(_MainTex, uv + float2(chromaticAberration, 0)).r;
                float g = tex2D(_MainTex, uv).g;
                float b = tex2D(_MainTex, uv - float2(chromaticAberration, 0)).b;
                fixed4 col = fixed4(r, g, b, 1);
                
                // Layered static noise
                float2 noiseUV = uv * staticScale;
                float time = _Time.y * staticSpeed;
                
                float n = 0;
                n += noise(noiseUV + float2(time, time * 0.7)) * 0.5;
                n += noise(noiseUV * 2.3 + float2(time * 1.3, time)) * 0.3;
                n += noise(noiseUV * 4.7 + float2(time * 0.8, time * 1.5)) * 0.2;

                float scanlinePos = (uv.y + _Time.y * scanlineSpeed * 0.1) * scanlineCount;
                float scanline = sin(scanlinePos * 3.14159) * 0.5 + 0.5;
                scanline = pow(scanline, 3);
                col.rgb *= lerp(1.0, scanline, scanlines);
                
                // Random interference
                float randomLine = hash(float2(floor(uv.y * 200), floor(_Time.y * 10)));
                if (randomLine > 0.98)
                {
                    col.rgb = lerp(col.rgb, float3(1, 1, 1), 0.3);
                }

                col.rgb = ((col.rgb - 0.5) * contrast + 0.5) * brightness;
                
                // Vignette
                float2 vignetteUV = uv * 2 - 1;
                float vignetteAmount = 1.0 - dot(vignetteUV, vignetteUV) * vignette;
                col.rgb *= vignetteAmount;

                n = pow(n, staticContrast);

                col.rgb = lerp(col.rgb, float3(n, n, n), staticIntensity);
                col *= tintColor;
                col.a *= alpha;
                
                return col;
            }
            ENDCG
        }
    }
}