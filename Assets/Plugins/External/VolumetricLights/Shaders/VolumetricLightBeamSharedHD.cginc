// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility
// UNITY_SHADER_NO_UPGRADE

#ifndef _VOLUMETRIC_LIGHT_BEAM_SHARED_INCLUDED_
#define _VOLUMETRIC_LIGHT_BEAM_SHARED_INCLUDED_

/// ****************************************
/// SHADER INPUT / OUTPUT STRUCT
/// ****************************************
struct vlb_appdata
{
    float4 vertex : POSITION;
    float4 texcoord : TEXCOORD0;

#if VLB_INSTANCING_API_AVAILABLE && (VLB_STEREO_INSTANCING || VLB_GPU_INSTANCING)
    UNITY_VERTEX_INPUT_INSTANCE_ID // for GPU Instancing and Single Pass Instanced rendering
#endif
};

struct v2f
{
    float4 posClipSpace : SV_POSITION;
    float3 posObjectSpace : TEXCOORD0;
    float4 posWorldSpace : TEXCOORD1;
    float3 posViewSpace : TEXCOORD2;
    float3 cameraPosObjectSpace : TEXCOORD3;

    float4 projPos : TEXCOORD6;

#ifdef VLB_FOG_UNITY_BUILTIN_COORDS
    UNITY_FOG_COORDS(7)
#endif

#if VLB_INSTANCING_API_AVAILABLE
#if VLB_GPU_INSTANCING
    UNITY_VERTEX_INPUT_INSTANCE_ID // not sure this one is useful
#endif

#if VLB_STEREO_INSTANCING
    UNITY_VERTEX_OUTPUT_STEREO // for Single Pass Instanced rendering
#endif
#endif // VLB_INSTANCING_API_AVAILABLE
};


#include "ShaderUtils.cginc"

inline float ComputeFadeWithCamera(float3 posViewSpace, float enabled)
{
    float distCamToPixWS = abs(posViewSpace.z); // only check Z axis (instead of length(posViewSpace.xyz)) to have smoother transition with near plane (which is not curved)
    float camFadeDistStart = _ProjectionParams.y; // cam near place
    float camFadeDistEnd = camFadeDistStart + _VLB_CameraBlendingDistance;
    float fadeWhenTooClose = smoothstep(0, 1, invLerpClamped(camFadeDistStart, camFadeDistEnd, distCamToPixWS));

    // fade out according to camera's near plane
    return lerp(1, fadeWhenTooClose, enabled);
}

// Vector Camera to current Pixel, in object space and normalized
inline float3 ComputeVectorCamToPixOSN(float3 pixPosOS, float3 cameraPosOS)
{
    float3 vecCamToPixOSN = normalize(pixPosOS - cameraPosOS);

    // Deal with ortho camera:
    // With ortho camera, we don't want to change the fresnel according to camera position.
    // So instead of computing the proper vector "Camera to Pixel", we take account of the "Camera Forward" vector (which is not dependant on the pixel position)
    float3 vecCamForwardOSN = VLB_GET_PROP(_CameraForwardOS);

    return lerp(vecCamToPixOSN, vecCamForwardOSN, VLB_CAMERA_ORTHO);
}

v2f vertShared(vlb_appdata v)
{
    v2f o;

#if VLB_INSTANCING_API_AVAILABLE && (VLB_STEREO_INSTANCING || VLB_GPU_INSTANCING)
    UNITY_SETUP_INSTANCE_ID(v);

    #if VLB_STEREO_INSTANCING
      #ifndef VLB_SRP_API // TODO CHECK THAT WE DON'T NEED THIS WITH SRP
        UNITY_INITIALIZE_OUTPUT(v2f, o);
      #endif
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    #endif

    #if VLB_GPU_INSTANCING
        UNITY_TRANSFER_INSTANCE_ID(v, o);
    #endif
#endif

    // compute the proper cone shape, so the whole beam fits into a 2x2x1 box
    // The model matrix (computed via the localScale from BeamGeometry.)
    float4 vertexOS = v.vertex;

    float2 coneRadius = VLB_GET_PROP(_ConeRadius);
    float maxRadius = max(coneRadius.x, coneRadius.y);
    float normalizedRadiusStart = coneRadius.x / maxRadius;
    float normalizedRadiusEnd = coneRadius.y / maxRadius;
    vertexOS.xy *= lerp(normalizedRadiusStart, normalizedRadiusEnd, vertexOS.z);

    float3 scaleObjectSpace = float3(maxRadius, maxRadius, VLB_GET_PROP(_DistanceFallOff).z); // maxGeometryDistance

    o.posWorldSpace = VLBObjectToWorldPos(vertexOS);
    o.posClipSpace = VLBObjectToClipPos(vertexOS.xyz);
    // TODO Should create and use VLBWorldToClipPos instead
    //o.posClipSpace = VLBWorldToClipPos(o.posWorldSpace.xyz);

#if defined(VLBWorldToViewPos)
    float3 posViewSpace = VLBWorldToViewPos(o.posWorldSpace.xyz);
#elif defined(VLBObjectToViewPos)
    float3 posViewSpace = VLBObjectToViewPos(vertexOS);
#else
    You_should_define_either_VLBWorldToViewPos_or_VLBObjectToViewPos
#endif

    // apply the same scaling than we do through the localScale in BeamGeometry.ComputeLocalMatrix
    // to get the proper transformed vertex position in object space
    o.posObjectSpace = vertexOS.xyz * scaleObjectSpace;


    o.projPos = Depth_VS_ComputeProjPos(posViewSpace, o.posClipSpace);

    o.cameraPosObjectSpace = VLBGetCameraPositionObjectSpace(scaleObjectSpace);

    o.posViewSpace = posViewSpace;

#ifdef VLB_FOG_UNITY_BUILTIN_COORDS
    UNITY_TRANSFER_FOG(o, o.posClipSpace);
#endif
    return o;
}

// original code from Inigo Quilez: https://www.iquilezles.org/www/articles/intersectors/intersectors.htm
float coneIntersect(float3 rayOrigin, float3 rayDir, float3 conePosEnd, float radiusStart, float radiusEnd)
{
    float3 ba = conePosEnd;
    float3 oa = rayOrigin;
    float3 ob = rayOrigin - conePosEnd;
    float m0 = dot(ba, ba);
    float m1 = dot(oa, ba);
    float m2 = dot(rayDir, ba);
    float m3 = dot(rayDir, oa);
    float m5 = dot(oa, oa);
    float m9 = dot(ob, ba);

    // caps
    if (m1 < 0.0)
    {
        if (dot2(oa*m2 - rayDir * m1) < (radiusStart*radiusStart*m2*m2)) // delayed division
            return -m1 / m2;
    }
    else if (m9 > 0.0)
    {
        float t = -m9 / m2; // NOT delayed division
        if (dot2(ob + rayDir * t) < (radiusEnd*radiusEnd))
            return t;
    }

    // body
    float rr = radiusStart - radiusEnd;
    float hy = m0 + rr * rr;
    float k2 = m0 * m0 - m2 * m2*hy;
    float k1 = m0 * m0*m3 - m1 * m2*hy + m0 * radiusStart*(rr*m2*1.0);
    float k0 = m0 * m0*m5 - m1 * m1*hy + m0 * radiusStart*(rr*m1*2.0 - m0 * radiusStart);
    float h = k1 * k1 - k2 * k0;
    if (h < 0.0) return -1.0; // no intersection

    float t = (-k1 - sqrt(h)) / k2;
    float y = m1 + t * m2;
    if (y<0.0 || y>m0) return -1.0; //no intersection
    return t;
}

half4 fragShared(v2f i)
{
#if VLB_INSTANCING_API_AVAILABLE && VLB_GPU_INSTANCING
    UNITY_SETUP_INSTANCE_ID(i);
#endif

#if VLB_INSTANCING_API_AVAILABLE && VLB_STEREO_INSTANCING
    // This fix access to depth map on the right eye when using single pass (aka Stereo Rendering Mode Multiview) on Gear VR or Oculus Go/Quest
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); // https://docs.unity3d.com/Manual/SinglePassInstancing.html
#endif

    float3 transformScale = VLB_GET_PROP(_TransformScale);
    float3 intersectOutOS = i.posObjectSpace * transformScale;

    // compute proper ray start and dir
    float3 realCamPosOS = i.cameraPosObjectSpace.xyz * transformScale;
    float3 rayDirOS = ComputeVectorCamToPixOSN(intersectOutOS, realCamPosOS); // deal with ortho cam here
    float3 realVecCamToPix = intersectOutOS - realCamPosOS;
    float rayLength = dot(rayDirOS, realVecCamToPix);
    float3 rayStartOS = intersectOutOS - rayLength * rayDirOS;

#if VLB_NOISE_3D
    // only useful for noise world space
    float3 intersectOutWS = i.posWorldSpace.xyz;
    float3 rayDirWS, rayStartWS;
    {
        float3 perspRayStartWS = _WorldSpaceCameraPos.xyz;
        float3 perspRayDirWS = normalize(intersectOutWS - perspRayStartWS);

        float3 orthoRayDirWS = VLB_GET_PROP(_CameraForwardWS);
        float3 orthoRayStartWS = intersectOutWS - rayLength * orthoRayDirWS;

        rayStartWS = lerp(perspRayStartWS, orthoRayStartWS, VLB_CAMERA_ORTHO);
        rayDirWS = lerp(perspRayDirWS, orthoRayDirWS, VLB_CAMERA_ORTHO);
    }
#endif // VLB_NOISE_3D

    float distanceFadeEnd = VLB_GET_PROP(_DistanceFallOff).y * transformScale.z;
    float2 coneRadius = VLB_GET_PROP(_ConeRadius) * min(transformScale.x, transformScale.y);

    float tIn = coneIntersect(rayStartOS, rayDirOS, float3(0, 0, distanceFadeEnd), coneRadius.x, coneRadius.y);

    // fix artifact in VR on geometry edges when no intersection is found
    float intensity = 1.0 - ifAnd(
        isLower(tIn, 0.0),
        isLower(realCamPosOS.z, 0.0) // is it ok to apply transformScale here?
    );

    tIn = max(tIn, 0); // when camera is inside the beam, tIn = -1: set tIn at 0

    float tOut = length(rayStartOS - intersectOutOS);
    float sceneZ = Depth_PS_GetSceneDepthFromEye(i.projPos, i.posViewSpace);

    { // DEBUG
#if DEBUG_DEPTH_MODE == DEBUG_VALUE_DEPTHBUFFER_FROMEYE
        return Depth_PS_GetSceneDepthFromEye(i.projPos, i.posViewSpace) * _ProjectionParams.w;
#elif DEBUG_DEPTH_MODE == DEBUG_VALUE_DEPTHBUFFER_FROMNEARPLANE
        return Depth_PS_GetSceneDepthFromNearPlane(i.projPos) * _ProjectionParams.w;
#elif DEBUG_DEPTH_MODE == DEBUG_VALUE_DEPTHSTEREOEYE
        float depthValue = Depth_PS_GetSceneDepthFromEye(i.projPos, i.posViewSpace) * _ProjectionParams.w;
#if defined(USING_STEREO_MATRICES) && defined(UNITY_STEREO_MULTIVIEW_ENABLED) // used with single pass / multiview on android VR (Oculus Go/Quest, Gear VR)
        return depthValue * lerp(float4(1, 0, 0, 1), float4(0, 1, 0, 1), unity_StereoEyeIndex);
#elif defined(UNITY_SINGLE_PASS_STEREO)
        return depthValue * lerp(float4(1, 0, 0, 1), float4(0, 0, 1, 1), unity_StereoEyeIndex);
#elif defined(UNITY_STEREO_INSTANCING_ENABLED)
        return depthValue * lerp(float4(0, 1, 0, 1), float4(0, 0, 1, 1), unity_StereoEyeIndex);
#elif defined(UNITY_STEREO_MULTIVIEW_ENABLED)
        return depthValue * lerp(float4(1, 1, 0, 1), float4(0, 1, 1, 1), unity_StereoEyeIndex);
#else
        return depthValue;
#endif
#elif DEBUG_DEPTH_MODE == DEBUG_VALUE_DEPTHBLEND
        return float4(1 - saturate(abs(tIn - sceneZ)), 1 - saturate(abs(tOut - sceneZ)), 0, 1);
#endif
    } // DEBUG

    tOut = min(tOut, sceneZ);
  
    float tInJittered = tIn;

    // add jittering
    {
        float2 screenPos = i.projPos.xy / i.projPos.w;

        float2 jitterCoord = screenPos * _ScreenParams.xy * _VLB_JitteringNoiseTex_TexelSize.xy;
        float jitterNoise = tex2D(_VLB_JitteringNoiseTex, jitterCoord).r;

        float4 jitterProps = VLB_GET_PROP(_Jittering);

        // Golden Ratio Animated Noise https://blog.demofox.org/2017/10/31/animating-noise-for-integration-over-time/
        float frameRate = jitterProps.y;
        const float kGoldenRatio = 1.61803398875f; // (1.0f + sqrt(5.0f)) * 0.5f;
        const float kGoldenRatioFrac = 0.61803398875f;
        
        uint frame = uint(floor(_Time.y * frameRate));
        jitterNoise = frac(jitterNoise + float(frame) * kGoldenRatio);

        float3 intersectInOS = rayStartOS + rayDirOS * tIn;
        float currentPosZNorm = max(intersectInOS.z, intersectOutOS.z) / distanceFadeEnd;

        float jitterRatio = invLerpClamped(jitterProps.z /* range start */, jitterProps.w /* range end */, currentPosZNorm);
        tInJittered += jitterNoise * jitterRatio * jitterProps.x;
    }

#if VLB_COLOR_GRADIENT
    float4 colorGradient = 0.0;
#endif // VLB_COLOR_GRADIENT

#if VLB_COOKIE_RGBA
    float4 colorCookieSum = 0;
#endif // VLB_COOKIE_RGBA

    {
        float distanceFadeStart = VLB_GET_PROP(_DistanceFallOff).x * transformScale.z;

        float sumLerp = 0.0;
        float sumLinear = 0.0;

        float sideSoftness = VLB_GET_PROP(_SideSoftness);

        const int raymarchSteps = VLB_RAYMARCHING_STEP_COUNT;

        int stepCountLinear = 0;
        float stepLinear = 1.1 * distanceFadeEnd / raymarchSteps;
        float tLinear = tIn; // using the non jittered version of tIn for linear sampling helps a bit smoothing the noise

#if VLB_SHADOW
        // cache some values useful for shadow computing
        float4 shw_props = VLB_GET_PROP(_ShadowProps);
        const float shw_isPersp = shw_props.w;
        const float shw_apexDist = VLB_GET_PROP(_ConeGeomProps).x * shw_isPersp;
        const float kMinNearClipPlane = 0.1f * shw_isPersp; // should be the same than in VolumetricShadowHD.cs

        const float shw_nearUnscaled = max(shw_apexDist, kMinNearClipPlane / transformScale.z);
        const float shw_nearScaled = max(shw_apexDist, kMinNearClipPlane);
        const float shw_farUnscaled = shw_nearUnscaled + distanceFadeEnd / transformScale.z;
        const float shw_farScaled = shw_nearScaled + distanceFadeEnd;

        // handle scale X & Y
        float2 shw_ratioScale;
        {
            float ratioScale = transformScale.x / transformScale.y;
            if (ratioScale >= 1)    shw_ratioScale = float2(1.0 / ratioScale, 1.0);
            else                    shw_ratioScale = float2(1.0, ratioScale);
        }

        #if DEBUG_DEPTH_MODE == DEBUG_VALUE_SHADOW_DEPTH
        {
            float shadowDepthRaw = tex2D(_ShadowDepthTexture, i.projPos.xy / i.projPos.w).r;
            float shadowDepthLinearPersp = VLB_ZBufferToLinear(shadowDepthRaw, shw_nearUnscaled, shw_farUnscaled);
            shadowDepthLinearPersp = fromABtoCD_Clamped(shadowDepthLinearPersp, shw_nearUnscaled, shw_farUnscaled, shw_nearScaled, shw_farScaled);
            const float shadowDepthLinearOrtho = shadowDepthRaw * (shw_farScaled - shw_nearScaled);
            float shadowDepthLinear = lerp(shadowDepthLinearOrtho, shadowDepthLinearPersp, shw_isPersp) - shw_apexDist;
            return shadowDepthLinear;
        }
        #endif // DEBUG_DEPTH_MODE
#endif // VLB_SHADOW

#if DEBUG_DEPTH_MODE == DEBUG_VALUE_LINEAR_OVERFLOW
        bool linearStepsOverflow = false;
#endif // DEBUG_VALUE_LINEAR_OVERFLOW
        for (int i = 0; i < raymarchSteps; i++)
        {
            float t = saturate(float(i+1) / (raymarchSteps + 1));
            float tLerp = lerp(tInJittered, tOut, t); // use the jittered version of tIn to apply noise on raymarching

            float3 posOSLinear = rayStartOS + rayDirOS * tLinear;
            float3 posOSLerp = rayStartOS + rayDirOS * tLerp;

            //if (length(posOSLerp - rayStartOS) > tOut) { break; } // Fix cookie sampling artifacts when a geometry is placed between the camera and the beam. Useless when disabling mipmaps on cookie texture

            float att = ComputeAttenuationHD(posOSLerp.z, distanceFadeStart, distanceFadeEnd);
            float widthAtThisZ = fromABtoCD_Clamped(posOSLerp.z, 0.0, distanceFadeEnd, coneRadius.x, coneRadius.y);

            float fresnel = (widthAtThisZ - length(posOSLerp.xy)) / (widthAtThisZ * sideSoftness);
            fresnel = saturate(fresnel);

            float powerAtThisStepLerp = att * fresnel;
            float powerAtThisStepLinear = 1.0;

#if VLB_COLOR_GRADIENT
            colorGradient += ApplyAlphaToColor(ComputeColorGradient(posOSLerp.z / transformScale.z));
#endif // VLB_COLOR_GRADIENT

#if VLB_NOISE_3D
            {
            //#define VLB_NOISE_3D_LINEAR 1
            #if VLB_NOISE_3D_LINEAR
                float3 posWSLinear = rayStartWS + rayDirWS * tLinear;
                float3 noiseUVWLinear = Noise3D_GetUVW(posWSLinear, posOSLinear);
                float noiseFactorLinear = Noise3D_GetFactorFromUVW(noiseUVWLinear);
                powerAtThisStepLinear *= noiseFactorLinear;
            #else
                float3 posWSLerp = rayStartWS + rayDirWS * tLerp;
                float3 noiseUVWLerp = Noise3D_GetUVW(posWSLerp, posOSLerp);
                float noiseFactorLerp = Noise3D_GetFactorFromUVW(noiseUVWLerp);
                powerAtThisStepLerp *= noiseFactorLerp;
            #endif
            }
#endif // VLB_NOISE_3D

#if VLB_COOKIE_1CHANNEL || VLB_COOKIE_RGBA
            {
                float4 posAndScale = VLB_GET_PROP(_CookiePosAndScale);
                float4 props = VLB_GET_PROP(_CookieProperties); // contrib + negative, texture channel, cos(rot), sin(rot)
                float2x2 rotMatrix = float2x2(props.z, -props.w, props.w, props.z);

                float2 posOSXY = posOSLerp.xy / widthAtThisZ;   // [-0.5 ; 0.5]
                posOSXY += posAndScale.xy;                      // translate
                posOSXY = mul(posOSXY, rotMatrix);              // rotate
                posOSXY *= posAndScale.zw;                      // scale
                posOSXY = posOSXY * 0.5 + 0.5;                  // transform coord to [0.0 ; 1.0]

            #if VLB_COOKIE_1CHANNEL
                float cookie = tex2D(_CookieTexture, posOSXY)[(int)props.y];
                float negative = max(props.x, 0); // props.x also store contribution as negative value if negative
                float contrib = abs(props.x);
                powerAtThisStepLerp *= lerp(1.0, lerp(cookie, 1.0 - cookie, negative), contrib);
            #endif

            #if VLB_COOKIE_RGBA
                colorCookieSum += tex2D(_CookieTexture, posOSXY);
            #endif
            }
#endif // VLB_COOKIE_1CHANNEL || VLB_COOKIE_RGBA

#if VLB_SHADOW
            {
                float3 posOSRef = posOSLerp;

                float width = lerp(coneRadius.x, widthAtThisZ, shw_isPersp);
                float2 posOSXYNormalized = posOSRef.xy / width;

                // handle scale X & Y
                posOSXYNormalized *= shw_ratioScale;

                posOSXYNormalized = posOSXYNormalized * 0.5 + 0.5;
                posOSXYNormalized.x = flipUV(posOSXYNormalized.x, shw_props.x);
                posOSXYNormalized.y = flipUV(posOSXYNormalized.y, shw_props.y);

                float shadowDepthRaw = tex2D(_ShadowDepthTexture, posOSXYNormalized).r;
                shadowDepthRaw = lerp(shadowDepthRaw, 1.0f - shadowDepthRaw, _VLB_UsesReversedZBuffer);

                // Compte perspective linear depth and handle Z scaling
                float shadowDepthLinearPersp = VLB_ZBufferToLinear(shadowDepthRaw, shw_nearUnscaled, shw_farUnscaled); // decode depth value using unscaled near/far distance
                shadowDepthLinearPersp = fromABtoCD_Clamped(shadowDepthLinearPersp, shw_nearUnscaled, shw_farUnscaled, shw_nearScaled, shw_farScaled); // scale the linear depth value according to the scaled near/far distance

                // Compute ortho linear depth
                const float shadowDepthLinearOrtho = shadowDepthRaw * (shw_farScaled - shw_nearScaled);

                // get either the perspective or ortho depth
                float shadowDepthLinear = lerp(shadowDepthLinearOrtho, shadowDepthLinearPersp, shw_isPersp) - shw_apexDist;

                float factor = isEqualOrGreater(shadowDepthLinear, posOSRef.z);

                float dimmer = shw_props.z;
                factor = (1 - dimmer) + factor * dimmer; // lerp(1 - dimmer, 1, factor)
                powerAtThisStepLerp *= factor;
            }
#endif // VLB_SHADOW

            sumLerp += powerAtThisStepLerp;

            if (tLinear < tOut)
            {
                tLinear += stepLinear;
                sumLinear += powerAtThisStepLinear;
                stepCountLinear++;
            }
        #if DEBUG_DEPTH_MODE == DEBUG_VALUE_LINEAR_OVERFLOW
            else
            {
                linearStepsOverflow = true;
            }
        #endif // DEBUG_VALUE_LINEAR_OVERFLOW
        }
        // for loop end

#if VLB_COLOR_GRADIENT
        colorGradient /= raymarchSteps;
#endif // VLB_COLOR_GRADIENT

#if VLB_COOKIE_RGBA
        colorCookieSum /= raymarchSteps;
#endif // VLB_COOKIE_RGBA


        intensity *= (sumLerp / raymarchSteps);

        if (stepCountLinear > 0)
        {   
            float meanLinear = (sumLinear / stepCountLinear);
        #if DEBUG_DEPTH_MODE == DEBUG_VALUE_LINEAR_OVERFLOW
            {
                if (!linearStepsOverflow)
                    return float4(0, 0, 1, 1);
            }
        #endif // DEBUG_VALUE_LINEAR_OVERFLOW
            intensity *= meanLinear;
        }

        intensity *= (tOut - tIn); // prevent from having a darker circle at end cap
        intensity *= VLB_GET_PROP(_Intensity);
    }

    {
        float fadeWithCameraEnabled = 1 - VLB_CAMERA_ORTHO; // fading according to camera eye position doesn't make sense with ortho camera
        intensity *= ComputeFadeWithCamera(i.posViewSpace, fadeWithCameraEnabled);
    }

    {
        float factorInsideGeom = isEqualOrGreater(tOut, tIn);
        intensity *= factorInsideGeom;
    }

#if VLB_COLOR_GRADIENT
    float4 color = colorGradient;
#elif VLB_COLOR_FLAT
    float4 color = ApplyAlphaToColor(ComputeColorFlat());
#endif // VLB_COLOR_GRADIENT / VLB_COLOR_FLAT

#if VLB_COOKIE_RGBA
    {
        float4 props = VLB_GET_PROP(_CookieProperties); // contrib + negative, texture channel, cos(rot), sin(rot)
        float contrib = abs(props.x);
        color *= lerp(1, colorCookieSum, contrib);
    }
#endif // VLB_COOKIE_RGBA

#if VLB_DITHERING
    {
        float2 screenPos = i.projPos.xy / i.projPos.w;
        float2 ditherCoord = screenPos * _ScreenParams.xy * _VLB_DitheringNoiseTex_TexelSize.xy;
        float dither = tex2D(_VLB_DitheringNoiseTex, ditherCoord).r - 0.5;
        color += (1 - saturate(intensity)) * _VLB_DitheringFactor * dither;
    }
#endif

    ApplyPipelineSpecificIntensityModifier(/* inout */ intensity);

    intensity *= GetLODFadeFactor();

#if VLB_ALPHA_AS_BLACK
    color *= intensity;
#else
    color.a *= intensity;
#endif

#ifdef VLB_FOG_APPLY
    VLB_FOG_APPLY(color);
#endif

    return color;
}

#endif