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

	float pW = screenDim.x;
	float pH = screenDim.y;

	float3 p1 = GetViewPos(coord + float2(pW, 0.0)).xyz;
	float3 p2 = GetViewPos(coord + float2(0.0, pH)).xyz;
	float3 p3 = GetViewPos(coord + float2(-pW, 0.0)).xyz;
	float3 p4 = GetViewPos(coord + float2(0.0, -pH)).xyz;

	float3 vP = GetViewPos(coord);

	float3 dx = vP - p1;
	float3 dy = p2 - vP;
	float3 dx2 = p3 - vP;
	float3 dy2 = vP - p4;

	if (length(dx2) < length(dx) && coord.x - pW >= 0.0 || coord.x + pW > 1.0)
	{
		dx = dx2;
	}

	if (length(dy2) < length(dy) && coord.y - pH >= 0.0 || coord.y + pH > 1.0)
	{
		dy = dy2;
	}

	return normalize(-cross(dx, dy).xyz);
}

float lenSq(float3 v)
{
	return pow(v.x, 2.0) + pow(v.y, 2.0) + pow(v.z, 2.0);
}

#define _NoiseAmount 0.1
#define _Noise 1

float3 lightSample(float2 coord, float2 lightcoord, float3 normal, float3 position, float n, float2 texsize)
{
	float2 random = float2(1.0, 1.0);

	if (_Noise > 0)
	{
		random = (mod_dither3((coord * texsize) + float2(n * 82.294, n * 127.721))) * 0.01 * _NoiseAmount;
	}
	else
	{
		random = dither(coord, 1.0, texsize) * 0.1 * _NoiseAmount;
	}

	lightcoord *= float2(0.7, 0.7);

	//light absolute data
	float3 lightcolor = inputTex.Sample(linearWrapSampler, ((lightcoord)+random)).rgb;
	float3 lightnormal = getViewNormal(frac(lightcoord) + random).rgb;
	float3 lightposition = GetViewPos(frac(lightcoord) + random).xyz;

	//light variable data
	float3 lightpath = lightposition - position;
	float3 lightdir = normalize(lightpath);

	//falloff calculations
	float cosemit = clamp(dot(lightdir, -lightnormal), 0.0, 1.0);
	float coscatch = clamp(dot(lightdir, normal) * 0.5 + 0.5, 0.0, 1.0);
	float distfall = pow(lenSq(lightpath), 0.1) + 1.0;

	return (lightcolor * cosemit * coscatch / distfall) * (length(lightposition) / 20);
}

float2 ReprojectUV(float2 currentUV, float currentDepth)
{
	float4 screenPos = float4(currentUV.x * 2.0 - 1.0, (1.0 - currentUV.y) * 2.0 - 1.0, currentDepth, 1.0);
	float4 worldPos = mul(screenPos, viewProjInv);
	float4 newScreenPos = mul(worldPos, prevViewProj);
	float2 reprojectedUV = (newScreenPos.xy * 0.5) + 0.5;
	reprojectedUV.y = 1.0 - reprojectedUV.y;

	return reprojectedUV;
}

float4 main(VertexOut input) : SV_TARGET
{
	float depth = depthTex.SampleLevel(linearWrapSampler, input.Tex, 0.0f);
	if (depth == 1)
		discard;

	float3 direct = inputTex.Sample(linearWrapSampler, input.Tex).rgb;
	float3 indirect = float3(0.0, 0.0, 0.0);
	float3 position = GetViewPos(input.Tex);
	float3 normal = getViewNormal(input.Tex);

	float dlong = PI * (3.0 - sqrt(5.0));
	float dz = 1.0 / float(SSGI_RAY_COUNT);
	float l = 0.0;
	float z = 1.0 - dz / 2.0;

	for (int i = 0; i < SSGI_RAY_COUNT; i++)
	{
		float r = sqrt(1.0 - z);

		float xpoint = (cos(l) * r) * 0.5 + 0.5;
		float ypoint = (sin(l) * r) * 0.5 + 0.5;

		z = z - dz;
		l = l + dlong;

		indirect += lightSample(input.Tex, float2(xpoint, ypoint), normal, position, float(i), screenDim);
	}

	float3 result = (indirect / float(SSGI_RAY_COUNT) * intensity);

	return float4(result, 1.0);
}