// The following comment prevents Unity from auto upgrading the shader. Please keep it to keep backward compatibility.
// UNITY_SHADER_NO_UPGRADE

#ifndef _VLB_SHADER_UTILS_INCLUDED_
#define _VLB_SHADER_UTILS_INCLUDED_

#include "ShaderMaths.cginc"

// https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html
#define VLB_CAMERA_NEAR_PLANE _ProjectionParams.y
#define VLB_CAMERA_FAR_PLANE _ProjectionParams.z
#define VLB_CAMERA_ORTHO unity_OrthoParams.w // w is 1.0 when camera is orthographic, 0.0 when perspective

// Z buffer to 0..1 depth (0 at eye, 1 at far plane)
float VLB_ZBufferTo01(float depth, float near, float far)
{
    float x = 1 - far / near;
    float y = far / near;

    return 1.0 / (x * depth + y);
}

// Z buffer to linear depth
float VLB_ZBufferToLinear(float depth, float near, float far)
{
    float x = 1 - far / near;
    float y = far / near;

    float z = x / far;
    float w = y / far;

    return 1.0 / (z * depth + w);
}

inline float4 Depth_VS_ComputeProjPos(float3 vertexViewSpace, float4 vertexClipSpace)
{
    float4 projPos = ComputeScreenPos(vertexClipSpace);
    projPos.z = -vertexViewSpace.z; // = COMPUTE_EYEDEPTH
    return projPos;
}

inline float Depth_PS_GetLinearDepthOrtho(float rawDepth)
{
    rawDepth = lerp(rawDepth, 1.0f - rawDepth, _VLB_UsesReversedZBuffer);
    return (VLB_CAMERA_FAR_PLANE - VLB_CAMERA_NEAR_PLANE) * rawDepth + VLB_CAMERA_NEAR_PLANE;
}

inline float Depth_PS_GetSceneDepthFromNearPlane(float4 uv)
{
    float rawDepth = VLBSampleDepthTexture(uv);
    float linearDepthPersp = VLBLinearEyeDepth(rawDepth);

    float linearDepthOrtho = Depth_PS_GetLinearDepthOrtho(rawDepth);
    return lerp(linearDepthPersp, linearDepthOrtho, VLB_CAMERA_ORTHO);
}

inline float Depth_PS_GetSceneDepthFromEye(float4 uv, float3 posViewSpace)
{
    float rawDepth = VLBSampleDepthTexture(uv);
    float linearDepthPersp = VLBLinearEyeDepth(rawDepth);

    // transform perspective depth from near plane to distance based on the eye
    float acosViewDirZ = abs(normalize(posViewSpace.xyz).z); // TODO precompute that in VS?
    linearDepthPersp /= acosViewDirZ;

    float linearDepthOrtho = Depth_PS_GetLinearDepthOrtho(rawDepth);
    return lerp(linearDepthPersp, linearDepthOrtho, VLB_CAMERA_ORTHO);
}


#if VLB_DEPTH_BLEND || VLB_DITHERING
inline float DepthFade_PS_BlendDistance(float4 projPos, float3 posViewSpace, float distance)
{
    // Use FromNearPlane instead of FromEye for SD beams, it looks better specially in QA tests 'BlendWithGeom' and 'BlendWithGeomDisableAtSrc'
    float sceneDepth = Depth_PS_GetSceneDepthFromNearPlane(projPos);
    float sceneZ = max(0, sceneDepth - VLB_CAMERA_NEAR_PLANE);
    float partZ = max(0, projPos.z - VLB_CAMERA_NEAR_PLANE);
    return saturate((sceneZ - partZ) / distance);
}
#endif // VLB_DEPTH_BLEND || VLB_DITHERING


#if VLB_NOISE_3D
uniform sampler3D _VLB_NoiseTex3D;
uniform float _VLB_NoiseCustomTime;

float3 Noise3D_GetUVW(float3 posWorldSpace, float3 posLocalSpace)
{
    float4 noiseVelocityAndScale = VLB_GET_PROP(_NoiseVelocityAndScale);
    float2 noiseParam = VLB_GET_PROP(_NoiseParam);
    float3 velocity = noiseVelocityAndScale.xyz;
    float scale = noiseVelocityAndScale.w;

    float3 posRef = lerp(posWorldSpace, posLocalSpace, noiseParam.y); // 0 -> World Space ; 1 -> Local Space

    // use _VLB_NoiseCustomTime if it's equal or higher than 0.0
    float currentTime = lerp(_Time.y, _VLB_NoiseCustomTime, isEqualOrGreater(_VLB_NoiseCustomTime, 0.0f));

	//return frac(posRef.xyz * scale + (currentTime * velocity)); // frac doesn't give good results on VS
	return (posRef.xyz * scale + (currentTime * velocity));
}

float Noise3D_GetFactorFromUVW(float3 uvw)
{
    float2 noiseParam = VLB_GET_PROP(_NoiseParam);
    float intensity = noiseParam.x;
	float noise = tex3D(_VLB_NoiseTex3D, uvw).a;
    return lerp(1, noise, intensity);
}
#endif // VLB_NOISE_3D


inline float ComputeAttenuationSD(float pixDistZ, float fallOffStart, float fallOffEnd, float lerpLinearQuad)
{
    float distFromSourceNormalized = invLerpClamped(fallOffStart, fallOffEnd, pixDistZ);

    // Almost simple linear attenuation between Fade Start and Fade End: Use smoothstep for a better fall to zero rendering
    float attLinear = smoothstep(0, 1, 1 - distFromSourceNormalized);

    // Unity's custom quadratic attenuation https://forum.unity.com/threads/light-attentuation-equation.16006/
    float attQuad = 1.0 / (1.0 + 25.0 * distFromSourceNormalized * distFromSourceNormalized);

    const float kAttQuadStartToFallToZero = 0.8;
    attQuad *= saturate(smoothstep(1.0, kAttQuadStartToFallToZero, distFromSourceNormalized)); // Near the light's range (fade end) we fade to 0 (because quadratic formula never falls to 0)

    return lerp(attLinear, attQuad, lerpLinearQuad);
}

inline float ComputeAttenuationHD(float pixDistZ, float fallOffStart, float fallOffEnd)
{
    float distFromSourceNormalized = invLerpClamped(fallOffStart, fallOffEnd, pixDistZ);
    float att = -1.0f;

#if VLB_ATTENUATION_LINEAR
    // Simple linear attenuation
    att = (1 - distFromSourceNormalized);
#elif VLB_ATTENUATION_QUAD
    // Unity's custom quadratic attenuation
    // https://forum.unity.com/threads/light-attentuation-equation.16006/
    // https://forum.unity.com/threads/light-distance-in-shader.509306/#post-3326818
    att = saturate(1.0 / (1.0 + 25.0 * distFromSourceNormalized * distFromSourceNormalized) * saturate((1 - distFromSourceNormalized) * 5.0));
#endif

    return att;
}

#if VLB_COLOR_GRADIENT
#if VLB_COLOR_GRADIENT_MATRIX_HIGH || VLB_COLOR_GRADIENT_MATRIX_LOW
#if VLB_COLOR_GRADIENT_MATRIX_HIGH
#define FLOAT_PACKING_PRECISION 64
#else
#define FLOAT_PACKING_PRECISION 8
#endif
inline float4 UnpackToColor(float packedFloat)
{
    float4 color;

    color.a = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.b = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.g = packedFloat % FLOAT_PACKING_PRECISION;
    packedFloat = floor(packedFloat / FLOAT_PACKING_PRECISION);

    color.r = packedFloat;

    return color / (FLOAT_PACKING_PRECISION - 1);
}

inline float GetAtMatrixIndex(float4x4 mat, uint idx) { return mat[idx % 4][floor(idx / 4)]; }

inline float4 DecodeGradient(float t, float4x4 colorMatrix)
{
#define kColorGradientMatrixSize 16
    float sampleIndexFloat = t * (kColorGradientMatrixSize - 1);
    float ratioPerSample = sampleIndexFloat - (int)sampleIndexFloat;
    uint sampleIndexInt = min((uint)sampleIndexFloat, kColorGradientMatrixSize - 2);
    float4 colorA = UnpackToColor(GetAtMatrixIndex(colorMatrix, sampleIndexInt + 0));
    float4 colorB = UnpackToColor(GetAtMatrixIndex(colorMatrix, sampleIndexInt + 1));
    return lerp(colorA, colorB, ratioPerSample);
}
#elif VLB_COLOR_GRADIENT_ARRAY
inline half4 DecodeGradient(float t, float4 colorArray[kColorGradientArraySize])
{
    uint arraySize = kColorGradientArraySize;
    float sampleIndexFloat = t * (arraySize - 1);
    float ratioPerSample = sampleIndexFloat - (int)sampleIndexFloat;
    uint sampleIndexInt = min((uint)sampleIndexFloat, arraySize - 2);
    float4 colorA = colorArray[sampleIndexInt + 0];
    float4 colorB = colorArray[sampleIndexInt + 1];
    return lerp(colorA, colorB, ratioPerSample);
}
#endif // VLB_COLOR_GRADIENT_*

inline float4 ComputeColorGradient(float pixDistFromSource)
{
    float distanceFadeEnd = VLB_GET_PROP(_DistanceFallOff).y;
    float4x4 colorGradientMatrix = VLB_GET_PROP(_ColorGradientMatrix);
    float distFromSourceNormalized = invLerpClamped(0, distanceFadeEnd, pixDistFromSource);
    return DecodeGradient(distFromSourceNormalized, colorGradientMatrix);
}
#elif VLB_COLOR_FLAT
inline float4 ComputeColorFlat()
{
    return VLB_GET_PROP(_ColorFlat);
}
#endif // VLB_COLOR_GRADIENT / VLB_COLOR_FLAT

inline float4 ApplyAlphaToColor(float4 color)
{
#if VLB_ALPHA_AS_BLACK
    color.rgb *= color.a;
#endif
    return color;
}

inline float4 ApplyAlphaToColor(float4 color, float additionalAlpha)
{
#if VLB_ALPHA_AS_BLACK
    color.rgb *= color.a;
    color.rgb *= additionalAlpha;
#else
    color.a *= additionalAlpha;
#endif
    return color;
}

#endif // _VLB_SHADER_UTILS_INCLUDED_
