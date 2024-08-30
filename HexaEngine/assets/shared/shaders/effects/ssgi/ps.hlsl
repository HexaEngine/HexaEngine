#include "../../camera.hlsl"

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

Texture2D inputTex : register(t0);
Texture2D<float> depthTex : register(t1);
Texture2D normalTex : register(t2);

SamplerState linearWrapSampler : register(s0);

#if SSGI_QUALITY == 1
#define SSGI_RAY_STEPS 1
#define SSGI_RAY_COUNT 4
#define SSGI_DEPTH_BIAS 0.1
#elif SSGI_QUALITY == 2
#define SSGI_RAY_STEPS 1
#define SSGI_RAY_COUNT 8
#define SSGI_DEPTH_BIAS 0.1
#elif SSGI_QUALITY == 3
#define SSGI_RAY_STEPS 1
#define SSGI_RAY_COUNT 16
#define SSGI_DEPTH_BIAS 0.1
#elif SSGI_QUALITY == 4
#define SSGI_RAY_STEPS 1
#define SSGI_RAY_COUNT 32
#define SSGI_DEPTH_BIAS 0.1
#endif



#define PI 3.14159265359

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float GetMipLevel(float2 offset)
{
	float v = clamp(pow(dot(offset, offset), 0.1), 0.0, 1.0);
	float lod = 10.0 * v;
	return lod;
}

float Rand(float2 co)
{
	return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float CalculateShadowRayCast(float3 startPosition, float3 endPosition, float2 startUV)
{
	float rayDistance = length(endPosition - startPosition);
	float3 rayDirection = normalize(endPosition - startPosition);

	float distancePerStep = rayDistance / SSGI_RAY_STEPS;
	for (int i = 1; i < SSGI_RAY_STEPS; i++)
	{
		float currentDistance = i * distancePerStep;
		float3 currentPosition = startPosition + rayDirection * currentDistance;

		float4 projectedPosition = mul(float4(currentPosition, 1.0f), proj);
		projectedPosition.xyz /= projectedPosition.w;
		float2 currentUV = projectedPosition.xy;

		float lod = GetMipLevel(currentUV - startUV);
		float projectedDepth = 1.0 / projectedPosition.z;

		float currentDepth = 1.0 / depthTex.SampleLevel(linearWrapSampler, currentUV, lod).r;
		if (projectedDepth > currentDepth + SSGI_DEPTH_BIAS)
		{
			return float(i - 1) / SSGI_RAY_STEPS;
		}
	}

	return 1.0;
}

float3 GetViewPos(float2 coord, float4x4 ipm)
{
	float depth = depthTex.SampleLevel(linearWrapSampler, coord, 0.0f);
	return GetPositionWS(coord, depth);
}
float3 UnpackNormal(float3 normal)
{
    return 2 * normal - 1;
}

float3 GetViewNormal(float2 coord, float4x4 ipm, float2 texel)
{
	return UnpackNormal(normalTex.SampleLevel(linearWrapSampler, coord, 0).xyz);
}

float2 dither(float2 coord, float seed, float2 size)
{
	float noiseX = ((frac(1.0 - (coord.x + seed * 1.0) * (size.x / 2.0)) * 0.25) + (frac((coord.y + seed * 2.0) * (size.y / 2.0)) * 0.75)) * 2.0 - 1.0;
	float noiseY = ((frac(1.0 - (coord.x + seed * 3.0) * (size.x / 2.0)) * 0.75) + (frac((coord.y + seed * 4.0) * (size.y / 2.0)) * 0.25)) * 2.0 - 1.0;
	return float2(noiseX, noiseY);
}

float lenSq(float3 v)
{
	return pow(v.x, 2.0) + pow(v.y, 2.0) + pow(v.z, 2.0);
}

float3 LightBounce(float2 coord, float2 lightcoord, float3 normal, float3 position)
{
	float2 lightUV = coord + lightcoord;
		
	float3 lightColor = inputTex.SampleLevel(linearWrapSampler, lightUV, 0).rgb;
	float3 lightPosition = GetViewPos((lightUV), projInv).xyz;

	float3 lightDirection = lightPosition - position;
	float3 L = normalize(lightDirection);

	float currentDistance = length(lightDirection);
	float distanceAttenuation = clamp(1.0f - pow(currentDistance / distance, 4.0f), 0.0, 1.0);
	distanceAttenuation = isinf(currentDistance) ? 0.0 : distanceAttenuation;

	float NdotL = saturate(dot(normal, L));

	return lightColor * NdotL * distanceAttenuation;
}

float3 SecondBounce(float2 coords, float r, float3 normal, float3 position)
{
	float3 indirect = float3(0.0,0.0,0.0);
	for (int i = 0; i < SSGI_RAY_STEPS; i++)
	{
		float sampleDistance = exp(i - SSGI_RAY_STEPS);
		float phi = ((i + r * SSGI_RAY_COUNT) * 2.0 * PI) / SSGI_RAY_COUNT;
 		float2 uv = sampleDistance * float2(cos(phi), sin(phi));

		indirect += LightBounce(coords, uv, normal, position);
	}

	return indirect / SSGI_RAY_STEPS;
}

float4 main(VertexOut input) : SV_TARGET
{

	float depth = depthTex.SampleLevel(linearWrapSampler, input.Tex, 0.0f);
	if (depth == 1)
		discard;
	uint3 dimensions;
	inputTex.GetDimensions(0, dimensions.x, dimensions.y, dimensions.z);
	float2 texSize = float2(dimensions.xy);
	float2 inverse_size = 1.0f / float2(dimensions.xy);


	float3 position = GetViewPos(input.Tex, projInv);
	float3 normal = GetViewNormal(input.Tex, projInv, inverse_size);

	float3 indirect = float3(0.0,0.0,0.0);
	float r = Rand(input.Tex);

	for (int i = 0; i < SSGI_RAY_COUNT; i++)
	{
		float sampleDistance = exp(i - SSGI_RAY_COUNT);
 		float phi = ((i + r * SSGI_RAY_COUNT) * 2.0 * PI) / SSGI_RAY_COUNT;
 		float2 uv = sampleDistance * float2(cos(phi), sin(phi));

		float2 lightUV = input.Tex + uv;

		float3 lightPosition = GetViewPos((lightUV), projInv).xyz;
		float3 lightNormal = GetViewNormal((lightUV), projInv, inverse_size);
		
		float3 lightColor = LightBounce(input.Tex, uv, normal, position) + SecondBounce(lightUV, r, lightNormal, lightPosition);

		indirect += lightColor;
	}

	return float4(indirect / float(SSGI_RAY_COUNT) * intensity, 1.0);
}