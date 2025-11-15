#include "HexaEngine.Core:shaders/camera.hlsl"

// SSR_QUALITY == -1 Custom
// SSR_QUALITY == 0 Dynamic
// SSR_QUALITY == 1 Low
// SSR_QUALITY == 2 Mid
// SSR_QUALITY == 3 High

#ifndef SSR_QUALITY
#define SSR_QUALITY 2
#endif

#ifndef SSR_TEMPORAL
#define SSR_TEMPORAL 0
#endif

cbuffer SSRParams : register(b0)
{
	float intensity;
    float2 dims;
#if SSR_QUALITY == 0
	int SSR_MAX_RAY_COUNT;
	int SSR_RAY_STEPS;
	float SSR_RAY_STEP;
	float SSR_RAY_HIT_THRESHOLD;
#endif
};

#if SSR_QUALITY == 1
#define SSR_MAX_RAY_COUNT 8
#define SSR_RAY_STEPS 4
#define SSR_RAY_STEP 1.80f
#define SSR_RAY_HIT_THRESHOLD 3.00f
#endif

#if SSR_QUALITY == 2
#define SSR_MAX_RAY_COUNT 16
#define SSR_RAY_STEPS 16
#define SSR_RAY_STEP 1.60f
#define SSR_RAY_HIT_THRESHOLD 2.00f
#endif

#if SSR_QUALITY == 3
#define SSR_MAX_RAY_COUNT 32
#define SSR_RAY_STEPS 16
#define SSR_RAY_STEP 1.60f
#define SSR_RAY_HIT_THRESHOLD 2.00f
#endif

SamplerState pointClampSampler;
SamplerState linearClampSampler;
SamplerState linearBorderSampler;

Texture2D inputTex;
Texture2D<float> depthTex;
Texture2D normalTex;
Texture2D<float> linearDepthTex;

#if SSR_TEMPORAL
Texture2D velocityBufferTex;
Texture2D temporalBuffer;
#endif

RWTexture2D<float4> outputTex;

float hash(uint n)
{
	n = (n << 13U) ^ n;
	n = n * (n * n * 15731U + 789221U) + 1376312589U;
	return float(n & 0x7fffffffU) / float(0x7fffffff);
}

float GetTemporalJitter(uint frameIndex, uint2 pixelCoord)
{
	uint seed = frameIndex + pixelCoord.x * 1973 + pixelCoord.y * 9277;
	return hash(seed);
}

float4 ProjectUVW(float3 uv)
{
    float4 coords = mul(float4(uv, 1.0f), proj);
    coords.xy /= coords.w;
    coords.xy = coords.xy * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
    return coords;
}

float3 GetVSPos(float3 uv)
{
    float4 coords = ProjectUVW(uv);

    float depth = depthTex.SampleLevel(pointClampSampler, coords.xy, 0);

    return GetPositionVS(coords.xy, depth);
}

float4 SSRBinarySearch(float3 vDir, inout float3 vHitCoord)
{
	for (int i = 0; i < SSR_RAY_STEPS; i++)
	{
		// linearize depth here
        float3 fPositionVS = GetVSPos(vHitCoord);
		float fDepthDiff = vHitCoord.z - fPositionVS.z;

		if (fDepthDiff <= 0.0f)
			vHitCoord += vDir;

		vDir *= 0.5f;
		vHitCoord -= vDir;
	}

    float4 coords = ProjectUVW(vHitCoord);
    float depth = depthTex.SampleLevel(pointClampSampler, coords.xy, 0);

	// linearize depth here
    float3 fPositionVS = GetPositionVS(coords.xy, depth);
	float fDepthDiff = vHitCoord.z - fPositionVS.z;

    return float4(coords.xy, depth, abs(fDepthDiff) < SSR_RAY_HIT_THRESHOLD ? 1.0f : 0.0f);
}

float4 SSRRayMarch(float3 vDir, inout float3 vHitCoord, float jitter)
{
	float fDepth;

	for (int i = 0; i < SSR_MAX_RAY_COUNT; i++)
	{
		float stepMultiplier = 1.0f + jitter * 0.5f;
		vHitCoord += vDir * stepMultiplier;

		float4 vProjectedCoord = mul(float4(vHitCoord, 1.0f), proj);
		vProjectedCoord.xy /= vProjectedCoord.w;
		vProjectedCoord.xy = vProjectedCoord.xy * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);

		fDepth = depthTex.SampleLevel(pointClampSampler, vProjectedCoord.xy, 0);

		float3 fPositionVS = GetPositionVS(vProjectedCoord.xy, fDepth);

		float fDepthDiff = vHitCoord.z - fPositionVS.z;

		[branch]
		if (fDepthDiff > 0.0f)
		{
			return SSRBinarySearch(vDir, vHitCoord);
		}
	}

	return float4(0.0f, 0.0f, 0.0f, 0.0f);
}

[numthreads(32, 32, 1)]
void main(uint3 dispatchThreadID : SV_DispatchThreadID)
{
	uint2 pixelCoord = dispatchThreadID.xy;
    int3 pixel = int3(pixelCoord, 0);
	
    if (pixelCoord.x >= dims.x || pixelCoord.y >= dims.y)
        return;

    float2 uv = pixelCoord / dims;
    float depth = depthTex.Load(pixel);

#if !SSR_TEMPORAL
    float4 scene_color = inputTex.Load(pixel);

	if (depth == 1)
	{
		outputTex[pixelCoord] = scene_color;
		return;
	}
#else
	if (depth == 1)
	{
		outputTex[pixelCoord] = float4(0, 0, 0, 0);
		return;
	}
	
	float2 velocity = velocityBufferTex.Load(pixel).xy;
    float2 prevUV = uv - velocity; // Velocity is already in UV space
    float4 historyReflection = temporalBuffer.SampleLevel(linearClampSampler, prevUV, 0);
	
#endif

	float4 normalRoughness = normalTex.SampleLevel(linearBorderSampler, uv, 0);
	float roughness = normalRoughness.a;

	float3 normal = normalRoughness.rgb;
	normal = 2 * normal - 1.0;
	normal = normalize(mul(normal, (float3x3)view));

	float3 position = GetPositionVS(uv, depth);
	float3 reflectDir = normalize(reflect(position, normal));

	float jitter = GetTemporalJitter(frame, pixelCoord);

	float3 hitPos = position;
    float4 vCoords = SSRRayMarch(reflectDir, hitPos, jitter);

	float2 vCoordsEdgeFact = float2(1, 1) - pow(saturate(abs(vCoords.xy - float2(0.5f, 0.5f)) * 2), 8);
	float fScreenEdgeFactor = saturate(min(vCoordsEdgeFact.x, vCoordsEdgeFact.y));
	float reflectionIntensity =
		saturate(
			fScreenEdgeFactor * // screen fade
			saturate(reflectDir.z) // camera facing fade
			* (vCoords.w) // / 2 + 0.5f) // rayhit binary fade
			);

	float3 reflectionColor = reflectionIntensity * inputTex.SampleLevel(linearClampSampler, vCoords.xy, 0).rgb * intensity;
	
#if SSR_TEMPORAL    
    float historyDepth = depthTex.SampleLevel(linearClampSampler, prevUV, 0);
    float depthDiff = abs(depth - historyDepth);
    
    const float depthThreshold = 0.02f;
    bool depthValid = depthDiff < depthThreshold;
    
	bool validHistory = all(prevUV > 0.0f) && all(prevUV < 1.0f);
    float blendFactor = (validHistory && depthValid) ? 0.05f : 1.0f;
    
    float3 finalReflection = lerp(historyReflection.rgb, reflectionColor, blendFactor);
    float4 output = float4(finalReflection, 1.0f) * (1 - roughness);
	
    outputTex[pixelCoord] = output;
#else
    outputTex[pixelCoord] = scene_color + (1 - roughness) * max(0, float4(reflectionColor, 1.0f));
#endif
}