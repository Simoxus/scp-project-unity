Shader "Custom/Outline Fill" 
{
    Properties 
    {
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth("Outline Width", Range(0, 10)) = 2
        _MinPixelWidth("Min Pixel Width", Range(1, 10)) = 2
    }
     
    SubShader 
    {
        Tags 
        {
            "Queue" = "Transparent+110"
            "RenderType" = "Transparent"
            "DisableBatching" = "True"
            "IgnoreProjector" = "True"
        }
        
        Pass 
        {
            Name "Fill"
            Cull Off
            ZTest [_ZTest]
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask RGB
            Offset -1, -1
            
            Stencil 
            {
                Ref 1
                Comp NotEqual
            }
            
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            
            struct appdata 
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 smoothNormal : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f 
            {
                float4 position : SV_POSITION;
                fixed4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            uniform fixed4 _OutlineColor;
            uniform float _OutlineWidth;
            uniform float _MinPixelWidth;
            
            v2f vert(appdata input) 
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float3 normal = any(input.smoothNormal) ? input.smoothNormal : input.normal;
                float3 viewPos = UnityObjectToViewPos(input.vertex);
                float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));
                
                float outlineSize = _OutlineWidth * 0.001;
                
                float4 clipPos = UnityViewToClipPos(viewPos);
                float4 clipPosOffset = UnityViewToClipPos(viewPos + viewNormal * outlineSize);
                float2 screenPos = clipPos.xy / clipPos.w;
                float2 screenPosOffset = clipPosOffset.xy / clipPosOffset.w;
                float2 screenDiff = (screenPosOffset - screenPos) * _ScreenParams.xy * 0.5;
                float pixelWidth = length(screenDiff);
                
                float scale = max(1.0, _MinPixelWidth / max(pixelWidth, 0.001));
                outlineSize *= scale;
                
                output.position = UnityViewToClipPos(viewPos + viewNormal * outlineSize);
                output.color = _OutlineColor;
                
                return output;
            }
            
            fixed4 frag(v2f input) : SV_Target 
            {
                fixed4 color = input.color;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}