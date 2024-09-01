#include "../../camera.hlsl"

// SSR_QUALITY == -1 Custom
// SSR_QUALITY == 0 Dynamic
// SSR_QUALITY == 1 Low
// SSR_QUALITY == 2 Mid
// SSR_QUALITY == 3 High

#ifndef SSR_QUALITY
#define SSR_QUALITY 2
#endif

#if SSR_QUALITY == 0
cbuffer SSRParams : register(b0)
{
	int SSR_MAX_RAY_COUNT;
	int SSR_RAY_STEPS;
	float SSR_RAY_STEP;
	float SSR_RAY_HIT_THRESHOLD;
};

#endif

#if SSR_QUALITY == 1
#define SSR_MAX_RAY_COUNT 8
#define SSR_RAY_STEPS 8
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

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float4 SSRBinarySearch(float3 vDir, inout float3 vHitCoord)
{
	float fDepth;

	for (int i = 0; i < SSR_RAY_STEPS; i++)
	{
		float4 vProjectedCoord = mul(float4(vHitCoord, 1.0f), proj);
		vProjectedCoord.xy /= vProjectedCoord.w;
		vProjectedCoord.xy = vProjectedCoord.xy * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);

		// linearize depth here
		fDepth = depthTex.SampleLevel(pointClampSampler, vProjectedCoord.xy, 0);
		float3 fPositionVS = GetPositionVS(vProjectedCoord.xy, fDepth);
		float fDepthDiff = vHitCoord.z - fPositionVS.z;

		if (fDepthDiff <= 0.0f)
			vHitCoord += vDir;

		vDir *= 0.5f;
		vHitCoord -= vDir;
	}

	float4 vProjectedCoord = mul(float4(vHitCoord, 1.0f), proj);
	vProjectedCoord.xy /= vProjectedCoord.w;
	vProjectedCoord.xy = vProjectedCoord.xy * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);

	// linearize depth here
	fDepth = depthTex.SampleLevel(pointClampSampler, vProjectedCoord.xy, 0);
	float3 fPositionVS = GetPositionVS(vProjectedCoord.xy, fDepth);
	float fDepthDiff = vHitCoord.z - fPositionVS.z;

	return float4(vProjectedCoord.xy, fDepth, abs(fDepthDiff) < SSR_RAY_HIT_THRESHOLD ? 1.0f : 0.0f);
}

float4 SSRRayMarch(float3 vDir, inout float3 vHitCoord)
{
	float fDepth;

	for (int i = 0; i < SSR_MAX_RAY_COUNT; i++)
	{
		vHitCoord += vDir;

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

bool bInsideScreen(in float2 vCoord)
{
	return !(vCoord.x < 0 || vCoord.x > 1 || vCoord.y < 0 || vCoord.y > 1);
}

float4 main(VertexOut pin) : SV_TARGET
{
	float4 NormalRoughness = normalTex.Sample(linearBorderSampler, pin.Tex);
	float roughness = NormalRoughness.a;
	float4 scene_color = inputTex.SampleLevel(linearClampSampler, pin.Tex, 0);

	if (roughness > 0.8f)
		return scene_color;

	float3 Normal = NormalRoughness.rgb;
	Normal = 2 * Normal - 1.0;
	Normal = normalize(mul(Normal, (float3x3)view));

	float depth = depthTex.Sample(linearClampSampler, pin.Tex);
	float3 Position = GetPositionVS(pin.Tex, depth);
	float3 ReflectDir = normalize(reflect(Position, Normal));

	//Raycast
	float3 HitPos = Position;
	float4 vCoords = SSRRayMarch(ReflectDir, HitPos);

	float2 vCoordsEdgeFact = float2(1, 1) - pow(saturate(abs(vCoords.xy - float2(0.5f, 0.5f)) * 2), 8);
	float fScreenEdgeFactor = saturate(min(vCoordsEdgeFact.x, vCoordsEdgeFact.y));
	float reflectionIntensity =
		saturate(
			fScreenEdgeFactor * // screen fade
			saturate(ReflectDir.z) // camera facing fade
			* (vCoords.w) // / 2 + 0.5f) // rayhit binary fade
			);

	float3 reflectionColor = reflectionIntensity * inputTex.SampleLevel(linearClampSampler, vCoords.xy, 0).rgb;
	return scene_color + (1 - roughness) * max(0, float4(reflectionColor, 1.0f));
}