#include "HexaEngine.Core:shaders/camera.hlsl"
#include "HexaEngine.Core:shaders/math.hlsl"

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

SamplerState pointClampSampler : register(s0);
SamplerState linearClampSampler : register(s1);
SamplerState linearBorderSampler : register(s2);

Texture2D inputTex : register(t0);
Texture2D<float> depthTex : register(t1);
Texture2D normalTex : register(t2);
Texture2D<float> linearDepthTex;

#if SSR_TEMPORAL

Texture2D velocityBufferTex;
Texture2D temporalBuffer;

#endif

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

// Hash function for temporal jitter
float hash(uint n)
{
	n = (n << 13U) ^ n;
	n = n * (n * n * 15731U + 789221U) + 1376312589U;
	return float(n & 0x7fffffffU) / float(0x7fffffff);
}

// Generate temporal jitter offset
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
		// Apply temporal jitter to the step size
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

		vDir *= SSR_RAY_STEP;
	}

	return float4(0.0f, 0.0f, 0.0f, 0.0f);
}

float4 main(VertexOut pin) : SV_TARGET
{
	float depth = depthTex.Sample(linearClampSampler, pin.Tex);

#if !SSR_TEMPORAL
	float4 scene_color = inputTex.SampleLevel(linearClampSampler, pin.Tex, 0);

	if (depth == 1)
		return scene_color;
#else
	
	if (depth == 1)
		return float4(0, 0, 0, 0);
	
#endif

	float4 normalRoughness = normalTex.Sample(linearBorderSampler, pin.Tex);
	float roughness = normalRoughness.a;

	float3 normal = normalRoughness.rgb;
	normal = 2 * normal - 1.0;
	normal = normalize(mul(normal, (float3x3)view));

	float3 position = GetPositionVS(pin.Tex, depth);
	float3 reflectDir = normalize(reflect(position, normal));

	// Calculate temporal jitter
	uint2 pixelCoord = uint2(pin.Tex * screenDim);
	float jitter = GetTemporalJitter(frame, pixelCoord);

	//Raycast with temporal jitter
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

    float2 velocity = velocityBufferTex.SampleLevel(linearClampSampler, pin.Tex, 0).xy;
    float2 prevUV = pin.Tex - velocity; // Velocity is already in UV space
    
    bool validHistory = all(prevUV > 0.0f) && all(prevUV < 1.0f);
    
    float4 historyReflection = temporalBuffer.SampleLevel(linearClampSampler, prevUV, 0);
    
    float historyDepth = depthTex.SampleLevel(linearClampSampler, prevUV, 0);
    float depthDiff = abs(depth - historyDepth);
    
    const float depthThreshold = 0.02f;
    bool depthValid = depthDiff < depthThreshold;
    
    // Adaptive blend factor
    float blendFactor = (validHistory && depthValid) ? 0.05f : 1.0f;
    
    float3 finalReflection = lerp(historyReflection.rgb, reflectionColor, blendFactor);
    
    return float4(finalReflection, 1.0f) * (1 - roughness);
	
#else
	
    return scene_color + (1 - roughness) * max(0, float4(reflectionColor, 1.0f));
#endif
}