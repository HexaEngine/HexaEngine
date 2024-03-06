#ifndef SHADOW_COMMON_H_INCLUDED
#define SHADOW_COMMON_H_INCLUDED

#define HARD_SHADOWS 0
#define PCF_SHADOWS 1
#define PCSS_SHADOWS 2
#define ESM_SHADOWS 3
#define VSM_SHADOWS 4
#define EVSM_SHADOWS 5
#define SAVSM_SHADOWS 6
#define MSM_SHADOWS 7

#ifndef SOFT_SHADOWS
#define SOFT_SHADOWS 0
#endif

#include "light.hlsl"
#include "shadow.hlsl"
#include "camera.hlsl"

inline float3 GetShadowUVD(float3 pos, float4x4 view)
{
	float4 fragPosLightSpace = mul(float4(pos, 1.0), view);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	projCoords.xy = projCoords.xy * 0.5 + 0.5;
	return projCoords;
}

inline float3 GetShadowAtlasUVD(float3 pos, float size, float4 region, float4x4 view)
{
	float4 fragPosLightSpace = mul(float4(pos, 1.0), view);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 shadowSpaceCoord = fragPosLightSpace.xyz / fragPosLightSpace.w;
	shadowSpaceCoord.xy = shadowSpaceCoord.xy * 0.5 + 0.5;
	shadowSpaceCoord.xy = region.xy + shadowSpaceCoord.xy * region.zw;
	return shadowSpaceCoord;
}

inline float2 GetShadowAtlasUV(float3 pos, float size, float4 region, float4x4 view)
{
	float4 fragPosLightSpace = mul(float4(pos, 1.0), view);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float2 shadowSpaceCoord = fragPosLightSpace.xy / fragPosLightSpace.w;
	shadowSpaceCoord.xy = shadowSpaceCoord.xy * 0.5 + 0.5;
	shadowSpaceCoord.xy = region.xy + shadowSpaceCoord.xy * region.zw;
	return shadowSpaceCoord;
}

inline float2 NormalizeShadowAtlasUV(float2 shadowSpaceCoord, float4 region)
{
	return region.xy + shadowSpaceCoord.xy * region.zw;
}

inline float NormalizeDepth(float depth, float near, float far)
{
	return (depth - near) / (far - near);
}

inline float4 paraboloid(float4 frag_pos, float dir, float near, float far)
{
	float4 result = frag_pos;

	result /= result.w;

	result.z *= dir;

	float len = length(result.xyz);
	result /= len;

	result.x /= result.z + 1.0f;
	result.y /= result.z + 1.0f;

	result.z = (len - near) / (far - near);
	result.w = 1.0;

	return result;
}

int GetPointLightFace(float3 r)
{
	float rx = abs(r.x);
	float ry = abs(r.y);
	float rz = abs(r.z);
	float d = max(rx, max(ry, rz));
	if (d == rx)
	{
		return (r.x >= 0.0 ? 0 : 1); // X+: 0, X-: 1
	}
	else if (d == ry)
	{
		return (r.y >= 0.0 ? 2 : 3); // Y+: 2, Y-:3
	}
	else
	{
		return (r.z >= 0.0 ? 4 : 5); // Z+: 4, Z-:5
	}
}

float SampleHard(SamplerState state, Texture2D shadowMap, float2 uvd, float depth, float bias)
{
	float shadowDepth = shadowMap.SampleLevel(state, uvd.xy, 0).r;
	float shadowFactor = (depth - bias > shadowDepth) ? 1.0f : 0.0f;
	return (1.0f - shadowFactor);
}

float SampleHardArray(SamplerState state, Texture2DArray shadowMap, float2 uvd, uint layer, float depth, float bias)
{
	float shadowDepth = shadowMap.SampleLevel(state, float3(uvd.xy, layer), 0).r;
	float shadowFactor = (depth - bias > shadowDepth) ? 1.0f : 0.0f;
	return (1.0f - shadowFactor);
}

float SamplePCF(SamplerState state, Texture2D shadowMap, float2 uvd, float depth, float bias, float size, float softness)
{
	const float dx = 1.0f / size;

	float percentLit = 0.0f;

	float2 offsets[9] =
	{
		float2(-dx, -dx), float2(0.0f, -dx), float2(dx, -dx),
		float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
		float2(-dx, +dx), float2(0.0f, +dx), float2(dx, +dx)
	};

	[unroll]
		for (int i = 0; i < 9; ++i)
		{
			offsets[i] = offsets[i] * float2(softness, softness);
			float shadowDepth = shadowMap.SampleLevel(state, uvd.xy + offsets[i], 0).r;
			percentLit += (depth - bias > shadowDepth) ? 1.0f : 0.0f;
		}
	return 1 - (percentLit /= 9.0f);
}

float SamplePCFArray(SamplerState state, Texture2DArray shadowMap, float2 uvd, uint layer, float depth, float bias, float size, float softness)
{
	const float dx = 1.0f / size;

	float percentLit = 0.0f;

	float2 offsets[9] =
	{
		float2(-dx, -dx), float2(0.0f, -dx), float2(dx, -dx),
		float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
		float2(-dx, +dx), float2(0.0f, +dx), float2(dx, +dx)
	};

	[unroll]
		for (int i = 0; i < 9; ++i)
		{
			offsets[i] = offsets[i] * float2(softness, softness);
			float shadowDepth = shadowMap.SampleLevel(state, float3(uvd.xy + offsets[i], layer), 0).r;
			percentLit += (depth - bias > shadowDepth) ? 1.0f : 0.0f;
		}
	return 1 - (percentLit /= 9.0f);
}

float Linstep(float a, float b, float v)
{
	return saturate((v - a) / (b - a));
}

float ReduceLightBleeding(float pMax, float amount)
{
	// Remove the [0, amount] tail and linearly rescale (amount, 1].
	return Linstep(amount, 1.0f, pMax);
}

float Chebyshev(float2 moments, float depth)
{
	if (depth <= moments.x)
	{
		return 1.0;
	}

	float variance = moments.y - (moments.x * moments.x);

	float d = depth - moments.x; // attenuation
	float pMax = variance / (variance + d * d);

	return pMax;
}

float SampleVSM(SamplerState state, Texture2D depthTex, float2 texCoords, float fragDepth, float bias, float lightBleedingReduction)
{
	float2 moments = depthTex.Sample(state, texCoords).rg;
	float p = Chebyshev(moments, fragDepth);
	p = ReduceLightBleeding(p, lightBleedingReduction);
	return max(p, fragDepth <= moments.x);
}

float SampleVSMArray(SamplerState state, Texture2DArray depthTex, float2 texCoords, uint layer, float fragDepth, float bias, float lightBleedingReduction)
{
	float2 moments = depthTex.Sample(state, float3(texCoords, layer)).rg;
	float p = Chebyshev(moments, fragDepth);
	p = ReduceLightBleeding(p, lightBleedingReduction);
	return max(p, fragDepth <= moments.x);
}

float SampleESM(SamplerState state, Texture2D depthTex, float2 texCoords, float fragDepth, float bias, float exponent)
{
	float lit = 0.0f;
	float moment = depthTex.Sample(state, texCoords).r + bias;
	float visibility = exp(-exponent * fragDepth) * moment;
	return clamp(visibility, 0, 1);
}

float SampleESMArray(SamplerState state, Texture2DArray depthTex, float2 texCoords, uint layer, float fragDepth, float bias, float exponent)
{
	float lit = 0.0f;
	float moment = depthTex.Sample(state, float3(texCoords.xy, layer)).r + bias;
	float visibility = exp(-exponent * fragDepth) * moment;
	return clamp(visibility, 0, 1);
}

float SampleEVSM(SamplerState state, Texture2D depthTex, float2 texCoords, float fragDepth, float bias, float exponent, float lightBleedingReduction)
{
	float shadow = 0.0;
	float4 moments = depthTex.Sample(state, texCoords.xy); // pos, pos^2, neg, neg^2

	fragDepth = 2 * fragDepth - 1;
	float pos = exp(exponent * fragDepth);
	float neg = -exp(-exponent * fragDepth);

	float posShadow = Chebyshev(moments.xy, pos);
	float negShadow = Chebyshev(moments.zw, neg);

	posShadow = ReduceLightBleeding(posShadow, lightBleedingReduction);
	negShadow = ReduceLightBleeding(negShadow, lightBleedingReduction);

	shadow = min(posShadow, negShadow);
	return shadow;
}

float SampleEVSMArray(SamplerState state, Texture2DArray depthTex, float2 texCoords, uint layer, float fragDepth, float bias, float exponent, float lightBleedingReduction)
{
	float shadow = 0.0;
	float4 moments = depthTex.Sample(state, float3(texCoords.xy, layer)); // pos, pos^2, neg, neg^2

	fragDepth = 2 * fragDepth - 1;
	float pos = exp(exponent * fragDepth);
	float neg = -exp(-exponent * fragDepth);

	float posShadow = Chebyshev(moments.xy, pos);
	float negShadow = Chebyshev(moments.zw, neg);

	posShadow = ReduceLightBleeding(posShadow, lightBleedingReduction);
	negShadow = ReduceLightBleeding(negShadow, lightBleedingReduction);

	shadow = min(posShadow, negShadow);
	return shadow;
}

float4 ConvertOptimizedMoments(in float4 optimizedMoments)
{
	optimizedMoments[0] -= 0.035955884801f;
	return mul(optimizedMoments, float4x4(0.2227744146f, 0.1549679261f, 0.1451988946f, 0.163127443f,
		0.0771972861f, 0.1394629426f, 0.2120202157f, 0.2591432266f,
		0.7926986636f, 0.7963415838f, 0.7258694464f, 0.6539092497f,
		0.0319417555f, -0.1722823173f, -0.2758014811f, -0.3376131734f));
}
//from github MJP shadows
float ComputeMSMHamburger(in float4 moments, in float fragmentDepth)
{
	// Bias input data to avoid artifacts
	float4 b = lerp(moments, float4(0.5f, 0.5f, 0.5f, 0.5f), 0.001);
	float3 z;
	z[0] = fragmentDepth;

	// Compute a Cholesky factorization of the Hankel matrix B storing only non-
	// trivial entries or related products
	float L32D22 = mad(-b[0], b[1], b[2]);
	float D22 = mad(-b[0], b[0], b[1]);
	float squaredDepthVariance = mad(-b[1], b[1], b[3]);
	float D33D22 = dot(float2(squaredDepthVariance, -L32D22), float2(D22, L32D22));
	float InvD22 = 1.0f / D22;
	float L32 = L32D22 * InvD22;

	// Obtain a scaled inverse image of bz = (1,z[0],z[0]*z[0])^T
	float3 c = float3(1.0f, z[0], z[0] * z[0]);

	// Forward substitution to solve L*c1=bz
	c[1] -= b.x;
	c[2] -= b.y + L32 * c[1];

	// Scaling to solve D*c2=c1
	c[1] *= InvD22;
	c[2] *= D22 / D33D22;

	// Backward substitution to solve L^T*c3=c2
	c[1] -= L32 * c[2];
	c[0] -= dot(c.yz, b.xy);

	// Solve the quadratic equation c[0]+c[1]*z+c[2]*z^2 to obtain solutions
	// z[1] and z[2]
	float p = c[1] / c[2];
	float q = c[0] / c[2];
	float D = (p * p * 0.25f) - q;
	float r = sqrt(D);
	z[1] = -p * 0.5f - r;
	z[2] = -p * 0.5f + r;

	// Compute the shadow intensity by summing the appropriate weights
	float4 switchVal = (z[2] < z[0]) ? float4(z[1], z[0], 1.0f, 1.0f) :
		((z[1] < z[0]) ? float4(z[0], z[1], 0.0f, 1.0f) :
			float4(0.0f, 0.0f, 0.0f, 0.0f));
	float quotient = (switchVal[0] * z[2] - b[0] * (switchVal[0] + z[2]) + b[1]) / ((z[2] - switchVal[1]) * (z[0] - z[1]));
	float shadowIntensity = switchVal[2] + switchVal[3] * quotient;
	return saturate(shadowIntensity);
}

float SampleMSM(SamplerState state, Texture2D shadowMap, float2 texCoords, float fragDepth, float lightleakfix)
{
	float4 moments = shadowMap.Sample(state, texCoords.xy);

	float4 b = ConvertOptimizedMoments(moments); // moments = moments - float4(0.5, 0.0, 0.5, 0.0);

	return 1.0f - clamp(ComputeMSMHamburger(b, fragDepth) / lightleakfix, 0, 1);
}

float SampleShadow(SamplerState state, Texture2D shadowMap, ShadowData data, float3 uvd, float bias)
{
#if SOFT_SHADOWS == HARD_SHADOWS
	return SampleHard(state, shadowMap, uvd.xy, uvd.z, bias);
#endif
#if SOFT_SHADOWS == PCF_SHADOWS
	return SamplePCF(state, shadowMap, uvd.xy, uvd.z, bias, data.size, data.softness);
#endif
#if SOFT_SHADOWS == ESM_SHADOWS
	return SampleESM(state, shadowMap, uvd.xy, uvd.z, bias, data.softness);
#endif
#if SOFT_SHADOWS == VSM_SHADOWS
	return SampleVSM(state, shadowMap, uvd.xy, uvd.z, bias, data.softness);
#endif
#if SOFT_SHADOWS == EVSM_SHADOWS
	return SampleEVSM(state, shadowMap, uvd.xy, uvd.z, bias, data.softness, data.slopeBias);
#endif
}

float SampleShadowArray(SamplerState state, Texture2DArray shadowMap, ShadowData data, float3 uvd, uint layer, float bias)
{
#if SOFT_SHADOWS == HARD_SHADOWS
	return SampleHardArray(state, shadowMap, uvd.xy, layer, uvd.z, bias);
#endif
#if SOFT_SHADOWS == PCF_SHADOWS
	return SamplePCFArray(state, shadowMap, uvd.xy, layer, uvd.z, bias, data.size, data.softness);
#endif
#if SOFT_SHADOWS == ESM_SHADOWS
	return SampleESMArray(state, shadowMap, uvd.xy, layer, uvd.z, bias, data.softness);
#endif
#if SOFT_SHADOWS == VSM_SHADOWS
	return SampleVSMArray(state, shadowMap, uvd.xy, layer, uvd.z, bias, data.softness);
#endif
#if SOFT_SHADOWS == EVSM_SHADOWS
	return SampleEVSMArray(state, shadowMap, uvd.xy, layer, uvd.z, bias, data.softness, data.slopeBias);
#endif
}

float ShadowFactorDirectionalLight(SamplerState state, Texture2D shadowMap, ShadowData data, float3 position, float NdotL)
{
	float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

	if (uvd.z > 1.0f)
		return 1.0;

	// calculate bias (based on slope)
	float bias = max(0.05 * (1.0 - NdotL), 0.005);

	return SampleShadow(state, shadowMap, data, uvd, bias);
}

float ShadowFactorDirectionalLightCascaded(SamplerState state, Texture2DArray shadowMap, ShadowData data, float3 position, float NdotL)
{
	float cascadePlaneDistances[8] = (float[8]) data.cascades;

	// select cascade layer
	float4 fragPosViewSpace = mul(float4(position, 1.0), view);
	float depthValue = abs(fragPosViewSpace.z);
	float cascadePlaneDistance;
	uint layer = data.cascadeCount;
	for (uint i = 0; i < (uint)data.cascadeCount; ++i)
	{
		if (depthValue < cascadePlaneDistances[i])
		{
			cascadePlaneDistance = cascadePlaneDistances[i];
			layer = i;
			break;
		}
	}

	float3 uvd = GetShadowUVD(position, data.views[layer]);
	if (uvd.z > 1.0f)
		return 1.0;

	// calculate bias (based on depth map resolution and slope)
	float bias = max(0.05 * (1.0 - NdotL), 0.005);
	if (layer == data.cascadeCount)
	{
		bias *= 1 / (camFar * 0.5f);
	}
	else
	{
		bias *= 1 / (cascadePlaneDistance * 0.5f);
	}

	return SampleShadowArray(state, shadowMap, data, uvd, layer, bias);
}

float ShadowFactorPointLight(SamplerState state, Texture2D shadowMap, Light light, ShadowData data, float3 position, float NdotL)
{
	//calculate bias (based on slope)
	float bias = max(0.05 * (1.0 - NdotL), 0.005);

	const float near = 0.001;
	const float far = light.range;

	// transform into lightspace
	float4 lightPos = mul(float4(position, 1), data.views[0]);

	if (lightPos.z >= 0.0f)
	{
		float4 posFront = paraboloid(lightPos, 1, near, far);
		float2 uvFront = float2(0.5, 0.5) + float2(0.5, 0.5) * (posFront.xy * float2(1, -1));

		posFront.xy = NormalizeShadowAtlasUV(uvFront, data.regions[0]);

		return SampleShadow(state, shadowMap, data, posFront.xyz, bias);
	}
	else
	{
		float4 posBack = paraboloid(lightPos, -1, near, far);
		float2 uvBack = float2(0.5, 0.5) + float2(0.5, 0.5) * (posBack.xy * float2(1, -1));

		posBack.xy = NormalizeShadowAtlasUV(uvBack, data.regions[1]);

		return SampleShadow(state, shadowMap, data, posBack.xyz, bias);
	}
}

float ShadowFactorSpotlight(SamplerState state, Texture2D shadowMap, Light light, ShadowData data, float3 position, float NdotL)
{
	float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);
	uvd.z = length(light.position.xyz - position) / light.range;

	if (uvd.z > 1.0f)
		return 1.0;

	// calculate bias (based on slope)
	float bias = max(0.001 * (1.0 - NdotL), 0.0001);

	return SampleShadow(state, shadowMap, data, uvd, bias);
}

float ShadowFactorDirectionalLight(SamplerState state, Texture2D shadowMap, ShadowData data, float3 position)
{
	float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);

	if (uvd.z > 1.0f)
		return 1.0;

	// const bias
	const float bias = 0.005;

	return SampleShadow(state, shadowMap, data, uvd, bias);
}

float ShadowFactorDirectionalLightCascaded(SamplerState state, Texture2DArray shadowMap, ShadowData data, float3 position)
{
	float cascadePlaneDistances[8] = (float[8]) data.cascades;

	// select cascade layer
	float4 fragPosViewSpace = mul(float4(position, 1.0), view);
	float depthValue = abs(fragPosViewSpace.z);
	float cascadePlaneDistance;
	uint layer = data.cascadeCount;
	for (uint i = 0; i < (uint)data.cascadeCount; ++i)
	{
		if (depthValue < cascadePlaneDistances[i])
		{
			cascadePlaneDistance = cascadePlaneDistances[i];
			layer = i;
			break;
		}
	}

	float3 uvd = GetShadowUVD(position, data.views[layer]);
	if (uvd.z > 1.0f)
		return 1.0;

	// const bias
	float bias = 0.005;

	if (layer == data.cascadeCount)
	{
		bias *= 1 / (camFar * 0.5f);
	}
	else
	{
		bias *= 1 / (cascadePlaneDistance * 0.5f);
	}

	return SampleShadowArray(state, shadowMap, data, uvd, layer, bias);
}

float ShadowFactorPointLight(SamplerState state, Texture2D shadowMap, Light light, ShadowData data, float3 position)
{
	const float bias = 0.005f;

	const float near = 0.001;
	const float far = light.range;

	// transform into lightspace
	float4 lightPos = mul(float4(position, 1), data.views[0]);

	if (lightPos.z >= 0.0f)
	{
		float4 posFront = paraboloid(lightPos, 1, near, far);
		posFront.xy = float2(0.5, 0.5) + float2(0.5, 0.5) * (posFront.xy * float2(1, -1));

		return SampleShadow(state, shadowMap, data, posFront.xyz, bias);
	}
	else
	{
		float4 posBack = paraboloid(lightPos, -1, near, far);
		posBack.xy = float2(0.5, 0.5) + float2(0.5, 0.5) * (posBack.xy * float2(1, -1));

		return SampleShadow(state, shadowMap, data, posBack.xyz, bias);
	}
}

float ShadowFactorSpotlight(SamplerState state, Texture2D shadowMap, Light light, ShadowData data, float3 position)
{
	float3 uvd = GetShadowAtlasUVD(position, data.size, data.regions[0], data.views[0]);
	uvd.z = length(light.position.xyz - position) / light.range;

	if (uvd.z > 1.0f)
		return 1.0;

	const float bias = 0.00001;

	return SampleShadow(state, shadowMap, data, uvd, bias);
}

#endif