////////////////////////////////////////////////////////////////////////////////
// Filename: light.ps
////////////////////////////////////////////////////////////////////////////////
#include "../../gbuffer.hlsl"
#include "../../brdf.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"

Texture2D colorTexture : register(t0);
Texture2D positionTexture : register(t1);
Texture2D normalTexture : register(t2);
Texture2D cleancoatNormalTexture : register(t3);
Texture2D emissionTexture : register(t4);
Texture2D misc0Texture : register(t5);
Texture2D misc1Texture : register(t6);
Texture2D misc2Texture : register(t7);

StructuredBuffer<DirectionalLight> directionalLights : register(t8);
StructuredBuffer<PointLight> pointLights : register(t9);
StructuredBuffer<Spotlight> spotlights : register(t10);

Texture2DArray depthCSM : register(t12);
TextureCube depthOSM[8] : register(t13);
Texture2D depthPSM[8] : register(t21);

SamplerState SampleTypePoint : register(s0);
SamplerState SampleTypeAnsio : register(s1);

cbuffer constants : register(b0)
{
	uint directionalLightCount;
	uint pointLightCount;
	uint spotlightCount;
	uint PaddLight;
};

//////////////
// TYPEDEFS //
//////////////
struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float ShadowCalculation(SpotlightSD light, float3 fragPos, float bias, Texture2D depthTex, SamplerState state)
{
#if HARD_SHADOWS
	float4 fragPosLightSpace = mul(float4(fragPos, 1.0), light.view);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	float currentDepth = projCoords.z;

	projCoords = projCoords * 0.5 + 0.5;

	float depth = depthTex.Sample(state, projCoords.xy).r;
	float shadow += (currentDepth - bias) > depth ? 1.0 : 0.0;

	if (currentDepth > 1.0)
		shadow = 0;

	return shadow;
#else
	float4 fragPosLightSpace = mul(float4(fragPos, 1.0), light.view);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	float currentDepth = projCoords.z;

	projCoords = projCoords * 0.5 + 0.5;

	float w;
	float h;
	depthTex.GetDimensions(w, h);

	// PCF
	float shadow = 0.0;
	float2 texelSize = 1.0 / float2(w, h);
	[unroll]
	for (int x = -1; x <= 1; ++x)
	{
		[unroll]
		for (int y = -1; y <= 1; ++y)
		{
			float pcfDepth = depthTex.Sample(state, projCoords.xy + float2(x, y) * texelSize).r;
			shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
		}
	}

	shadow /= 9;

	if (currentDepth > 1.0)
		shadow = 0;

	return shadow;
#endif
}

// array of offset direction for sampling
float3 gridSamplingDisk[20] =
{
	float3(1, 1, 1), float3(1, -1, 1), float3(-1, -1, 1), float3(-1, 1, 1),
   float3(1, 1, -1), float3(1, -1, -1), float3(-1, -1, -1), float3(-1, 1, -1),
   float3(1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0), float3(-1, 1, 0),
   float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
   float3(0, 1, 1), float3(0, -1, 1), float3(0, -1, -1), float3(0, 1, -1)
};

float ShadowCalculation(PointLightSD light, float3 fragPos, float3 V, TextureCube depthTex, SamplerState state)
{
#if (HARD_SHADOWS)
	// get vector between fragment position and light position
	float3 fragToLight = fragPos - light.position;
	// use the light to fragment vector to sample from the depth map
	float closestDepth = depthTex.Sample(state, fragToLight).r; //texture(depthMap, fragToLight).r;
	// it is currently in linear range between [0,1]. Re-transform back to original value
	closestDepth *= light.far;
	// now get current linear depth as the length between the fragment and light position
	float currentDepth = length(fragToLight);
	// now test for shadows
	float bias = 0.05;
	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	return shadow;
#else
	// get vector between fragment position and light position
	float3 fragToLight = fragPos - light.position;
	// now get current linear depth as the length between the fragment and light position
	float currentDepth = length(fragToLight);
	float shadow = 0.0;
	float bias = 0.15;
	float viewDistance = length(V);
	float diskRadius = (1.0 + (viewDistance / light.far)) / 25.0;

	[unroll]
	for (int i = 0; i < 20; ++i)
	{
		float closestDepth = depthTex.Sample(state, fragToLight + gridSamplingDisk[i] * diskRadius).r;
		closestDepth *= light.far; // undo mapping [0;1]
		shadow += (currentDepth - bias) > closestDepth ? 1.0 : 0.0;
	}
	shadow /= 20;
	return shadow;
#endif
}

float ShadowCalculation(DirectionalLightSD light, float3 fragPosWorldSpace, float3 normal, Texture2DArray depthTex, SamplerState state)
{
	float cascadePlaneDistances[16] = (float[16]) light.cascades;
	float farPlane = cam_far;

	float w;
	float h;
	uint cascadeCount;
	depthTex.GetDimensions(w, h, cascadeCount);

	// select cascade layer
	float4 fragPosViewSpace = mul(float4(fragPosWorldSpace, 1.0), view);
	float depthValue = abs(fragPosViewSpace.z);
	float cascadePlaneDistance;
	uint layer = cascadeCount;
	for (uint i = 0; i < cascadeCount; ++i)
	{
		if (depthValue < cascadePlaneDistances[i])
		{
			cascadePlaneDistance = cascadePlaneDistances[i];
			layer = i;
			break;
		}
	}

	float4 fragPosLightSpace = mul(float4(fragPosWorldSpace, 1.0), light.views[layer]);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	float currentDepth = projCoords.z;
	projCoords = projCoords * 0.5 + 0.5;

	// calculate bias (based on depth map resolution and slope)
	normal = normalize(normal);
	float bias = max(0.05 * (1.0 - dot(normal, light.dir)), 0.005);
	const float biasModifier = 0.5f;
	if (layer == cascadeCount)
	{
		bias *= 1 / (farPlane * biasModifier);
	}
	else
	{
		bias *= 1 / (cascadePlaneDistance * biasModifier);
	}

#if (HARD_SHADOWS)
	float depth = depthTex.Sample(state, float3(projCoords.xy, layer)).r;
	float shadow = (currentDepth - bias) > depth ? 1.0 : 0.0;

	// keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
	if (currentDepth > 1.0)
	{
		shadow = 0.0;
	}

	return shadow;
#else
	// PCF
	float shadow = 0.0;
	float2 texelSize = 1.0 / float2(w, h);
	[unroll]
	for (int x = -1; x <= 1; ++x)
	{
		[unroll]
		for (int y = -1; y <= 1; ++y)
		{
			float pcfDepth = depthTex.Sample(state, float3(projCoords.xy + float2(x, y) * texelSize, layer)).r;
			shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
		}
	}

	shadow /= 9;

	// keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
	if (currentDepth > 1.0)
	{
		shadow = 0.0;
	}

	return shadow;
#endif
}

float4 ComputeLightingPBR(VSOut input, GeometryAttributes attrs)
{
	float3 position = attrs.pos;
	float3 baseColor = attrs.albedo;

	float specular = 0.5f;
	float specularTint = 0;
	float sheen = 0;
	float sheenTint = 0.5f;
	float clearcoat = 0.0f;
	float clearcoatGloss = 1;
	float anisotropic = attrs.anisotropic;
	float subsurface = 0;
	float roughness = attrs.roughness;
	float metalness = attrs.metalness;

	float3 N = normalize(attrs.normal);
	float3 X = normalize(attrs.tangent);
	float3 Y = normalize(cross(N, X));

	float3 V = normalize(GetCameraPos() - position);

	//float IOR = 1.5;
	//float3 F0 = float3(pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0), pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0), pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0));
	float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metalness);

	float3 Lo = attrs.emission;

	for (uint x = 0; x < directionalLightCount; x++)
	{
		DirectionalLight light = directionalLights[x];
		float3 L = normalize(-light.dir);
		float3 radiance = light.color.rgb;
		Lo += BRDF(L, V, N, X, Y, baseColor, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
	}
#ifdef SHADOWS
	for (uint y = 0; y < directionalLightSDCount; y++)
	{
		DirectionalLightSD light = directionalLightSDs[y];
		float bias = max(0.05f * (1.0 - dot(N, V)), 0.005f);
		float shadow = ShadowCalculation(light, attrs.pos, N, depthCSM, SampleTypeAnsio);
		float3 L = normalize(-light.dir);
		float3 radiance = light.color.rgb;
		Lo += (1.0f - shadow) * BRDF(L, V, N, X, Y, baseColor, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
	}
#endif
	for (uint z = 0; z < pointLightCount; z++)
	{
		PointLight light = pointLights[z];
		float3 LN = light.position - position;
		float distance = length(LN);
		float3 L = normalize(LN);

		float attenuation = 1.0 / (distance * distance);
		float3 radiance = light.color.rgb * attenuation;

		Lo += BRDF(L, V, N, X, Y, baseColor, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
	}
#ifdef SHADOWS
	for (uint zd = 0; zd < pointLightSDCount; zd++)
	{
		PointLightSD light = pointLightSDs[zd];
		float3 LN = light.position - position;
		float distance = length(LN);
		float3 L = normalize(LN);

		float attenuation = 1.0 / (distance * distance);
		float3 radiance = light.color.rgb * attenuation;
		float shadow = ShadowCalculation(light, attrs.pos, V, depthOSM[zd], SampleTypeAnsio);
		Lo += (1.0f - shadow) * BRDF(L, V, N, X, Y, baseColor, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
	}
#endif
	for (uint w = 0; w < spotlightCount; w++)
	{
		Spotlight light = spotlights[w];
		float3 LN = light.pos - position;
		float3 L = normalize(LN);

		float theta = dot(L, normalize(-light.dir));
		if (theta > light.cutOff)
		{
			float distance = length(LN);
			float attenuation = 1.0 / (distance * distance);
			float epsilon = light.cutOff - light.outerCutOff;
			float falloff = 1;
			if (epsilon != 0)
				falloff = 1 - smoothstep(0.0, 1.0, (theta - light.outerCutOff) / epsilon);
			float3 radiance = light.color.rgb * attenuation * falloff;
			Lo += BRDF(L, V, N, X, Y, baseColor, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
		}
	}
#ifdef SHADOWS
	for (uint wd = 0; wd < spotlightSDCount; wd++)
	{
		SpotlightSD light = spotlightSDs[wd];
		float3 LN = light.pos - position;
		float3 L = normalize(LN);

		float theta = dot(L, normalize(-light.dir));
		if (theta > light.cutOff)
		{
			float distance = length(LN);
			float attenuation = 1.0 / (distance * distance);
			float epsilon = light.cutOff - light.outerCutOff;
			float falloff = 1;
			if (epsilon != 0)
				falloff = 1 - smoothstep(0.0, 1.0, (theta - light.outerCutOff) / epsilon);
			float3 radiance = light.color.rgb * attenuation * falloff;
			float cosTheta = dot(N, -L);
			float bias = max(0.00025f * (1.0f - cosTheta), 0.000005f);
			float shadow = ShadowCalculation(light, attrs.pos, bias, depthPSM[wd], SampleTypeAnsio);

			Lo += (1.0f - shadow) * BRDF(L, V, N, X, Y, baseColor, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
		}
	}
#endif
	float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
	float3 ambient = BRDFIndirect(SampleTypeAnsio, irradianceTexture, prefilterTexture, brdfLUT, F0, N, V, baseColor, roughness, ao, anisotropic);

	float3 color = ambient + Lo;

	return float4(color, 1);
}

float4 main(VSOut pixel) : SV_TARGET
{
	GeometryAttributes attrs;
	ExtractGeometryData(
	pixel.Tex,
	colorTexture,
	positionTexture,
	normalTexture,
	cleancoatNormalTexture,
	emissionTexture,
	misc0Texture,
	misc1Texture,
	misc2Texture,
	SampleTypeAnsio,
	attrs);

	if (attrs.opacity < 0.1)
		discard;

	return ComputeLightingPBR(pixel, attrs);
}