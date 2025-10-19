#include "../../common.hlsl"

#ifndef SSGI_QUALITY
#define SSGI_QUALITY 1
#endif

#if SSGI_QUALITY == 0
cbuffer SSGIParams : register(b0)
{
	float intensity;
	float distance;
	int SSGI_RAY_COUNT;
	int SSGI_RAY_STEPS;
	float SSGI_DEPTH_BIAS;
};

#else
cbuffer SSGIParams : register(b0)
{
	float intensity;
	float distance;
};

#endif

Texture2D inputTex;
Texture2D<float> depthTex;
Texture2D normalTex;

SamplerState linearWrapSampler;

// Improved ray steps for better quality
#if SSGI_QUALITY == 1
#define SSGI_RAY_STEPS 4
#define SSGI_RAY_COUNT 4
#define SSGI_DEPTH_BIAS 0.05
#elif SSGI_QUALITY == 2
#define SSGI_RAY_STEPS 6
#define SSGI_RAY_COUNT 8
#define SSGI_DEPTH_BIAS 0.05
#elif SSGI_QUALITY == 3
#define SSGI_RAY_STEPS 8
#define SSGI_RAY_COUNT 16
#define SSGI_DEPTH_BIAS 0.05
#elif SSGI_QUALITY == 4
#define SSGI_RAY_STEPS 12
#define SSGI_RAY_COUNT 32
#define SSGI_DEPTH_BIAS 0.05
#endif

#define PI 3.14159265359

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float2 mod_dither3(float2 u)
{
	float noiseX = fmod(u.x + u.y + fmod(208. + u.x * 3.58, 13. + fmod(u.y * 22.9, 9.)), 7.) * .143;
	float noiseY = fmod(u.y + u.x + fmod(203. + u.y * 3.18, 12. + fmod(u.x * 27.4, 8.)), 6.) * .139;
	return float2(noiseX, noiseY) * 2.0 - 1.0;
}

float2 dither(float2 coord, float seed, float2 size)
{
	float noiseX = ((frac(1.0 - (coord.x + seed * 1.0) * (size.x / 2.0)) * 0.25) + (frac((coord.y + seed * 2.0) * (size.y / 2.0)) * 0.75)) * 2.0 - 1.0;
	float noiseY = ((frac(1.0 - (coord.x + seed * 3.0) * (size.x / 2.0)) * 0.75) + (frac((coord.y + seed * 4.0) * (size.y / 2.0)) * 0.25)) * 2.0 - 1.0;
	return float2(noiseX, noiseY);
}

float3 GetViewPos(float2 coord)
{
	float depth = depthTex.Sample(linearWrapSampler, coord).r;
	return GetPositionVS(coord, depth);
}

float3 getViewNormal(float2 coord)
{
	float3 normal = normalTex.Sample(linearWrapSampler, coord).rgb;
	return normalize(mul(UnpackNormal(normal), (float3x3)view));
}

float lenSq(float3 v)
{
	return dot(v, v);
}

// Create orthonormal basis from normal
void CreateOrthonormalBasis(float3 n, out float3 tangent, out float3 bitangent)
{
	if (abs(n.z) > 0.999f)
	{
		tangent = float3(1, 0, 0);
	}
	else
	{
		tangent = normalize(float3(-n.y, n.x, 0));
	}
	bitangent = cross(n, tangent);
}

// Convert hemisphere direction to world space
float3 HemisphereToWorld(float3 localDir, float3 normal)
{
	float3 tangent, bitangent;
	CreateOrthonormalBasis(normal, tangent, bitangent);
	return localDir.x * tangent + localDir.y * bitangent + localDir.z * normal;
}

// Cosine weighted hemisphere sampling
float3 CosineSampleHemisphere(float2 u)
{
	float r = sqrt(u.x);
	float theta = 2.0 * PI * u.y;

	float x = r * cos(theta);
	float y = r * sin(theta);
	float z = sqrt(max(0.0, 1.0 - u.x));

	return float3(x, y, z);
}

// Cone tracing implementation
float3 TraceCone(float3 startPos, float3 rayDir, float3 normal, float coneAngle, int rayIndex)
{
	float3 result = float3(0, 0, 0);
	float stepSize = distance / float(SSGI_RAY_STEPS);
	float3 rayPos = startPos;

	// Add initial offset to avoid self-intersection
	rayPos += rayDir * (stepSize * 0.1);

	for (int i = 0; i < SSGI_RAY_STEPS; i++)
	{
		float stepDistance = stepSize * (i + 1);
		rayPos = startPos + rayDir * stepDistance;

		// Project ray position to screen space
		float4 projectedPos = mul(float4(rayPos, 1.0), proj);
		projectedPos.xyz /= projectedPos.w;

		float2 sampleUV = projectedPos.xy * float2(0.5, -0.5) + 0.5;

		// Check if sample is within screen bounds
		if (!IsSaturated(sampleUV))
			continue;

		// Sample depth at projected position
		float sampleDepth = depthTex.SampleLevel(linearWrapSampler, sampleUV, 0).r;
		float3 samplePos = GetViewPos(sampleUV);

		// Calculate cone radius for this step
		float coneRadius = tan(coneAngle) * stepDistance;
		float mipLevel = log2(coneRadius * max(screenDim.x, screenDim.y));

		// Check for ray hit (ray is behind geometry)
		float rayDepth = projectedPos.z;
		float depthDiff = rayDepth - sampleDepth;

		if (depthDiff > SSGI_DEPTH_BIAS)
		{
			// Ray hit something, sample color and normal
			float3 hitColor = inputTex.SampleLevel(linearWrapSampler, sampleUV, clamp(mipLevel, 0, 5)).rgb;
			float3 hitNormal = getViewNormal(sampleUV);

			// Calculate contribution factors
			float distanceFactor = 1.0 / (1.0 + stepDistance * stepDistance * 0.1);
			float normalFactor = max(0.0, dot(hitNormal, -rayDir));
			float receiverNormalFactor = max(0.0, dot(normal, rayDir));

			// Energy conservation - Lambert BRDF
			float brdf = receiverNormalFactor / PI;

			// Accumulate result with proper weighting
			float weight = distanceFactor * normalFactor * brdf;
			result += hitColor * weight;
			break;
		}
	}

	return result;
}

#define _NoiseAmount 0.05

float4 main(VertexOut input) : SV_TARGET
{
	float depth = depthTex.SampleLevel(linearWrapSampler, input.Tex, 0.0f);
	if (depth >= 1.0)
		discard;

	float3 position = GetViewPos(input.Tex);
	float3 normal = getViewNormal(input.Tex);
	float3 indirect = float3(0.0, 0.0, 0.0);

	// Use better random number generation
	float2 noise = mod_dither3(input.Tex * screenDim + frame * 0.618034);

	// Generate rays using cosine-weighted hemisphere sampling
	for (int i = 0; i < SSGI_RAY_COUNT; i++)
	{
		// Generate stratified random numbers
		float2 u = float2(
			(float(i) + noise.x) / float(SSGI_RAY_COUNT),
			frac(noise.y + float(i) * 0.618034)
		);

		// Sample hemisphere direction
		float3 localRayDir = CosineSampleHemisphere(u);

		// Transform to world space
		float3 rayDir = HemisphereToWorld(localRayDir, normal);

		// Add some jitter to reduce banding
		float3 jitter = normalize(float3(
			noise.x - 0.5,
			noise.y - 0.5,
			0.1
		)) * _NoiseAmount;
		rayDir = normalize(rayDir + jitter);

		// Ensure ray is in the correct hemisphere
		if (dot(rayDir, normal) < 0.0)
			rayDir = reflect(rayDir, normal);

		// Cone angle based on ray count (fewer rays = wider cones)
		float coneAngle = PI / (4.0 * sqrt(float(SSGI_RAY_COUNT)));

		// Trace cone and accumulate result
		float3 coneResult = TraceCone(position, rayDir, normal, coneAngle, i);
		indirect += coneResult;
	}

	// Average and apply intensity
	indirect /= float(SSGI_RAY_COUNT);

	// Apply intensity
	float3 result = indirect * intensity;

	return float4(result, 1.0);
}