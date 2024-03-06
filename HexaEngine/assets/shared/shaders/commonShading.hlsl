#ifndef INCLUDE_H_COMMON_SHADING
#define INCLUDE_H_COMMON_SHADING

#define SOFT_SHADOWS 4

#include "brdf.hlsl"

#include "common.hlsl"
#include "commonShadows.hlsl"
#include "camera.hlsl"
#include "light.hlsl"
#include "weather.hlsl"
#include "gbuffer.hlsl"
#include "cluster.hlsl"
#include "material.hlsl"

SamplerState linearClampSampler : register(s0);
SamplerState linearWrapSampler : register(s1);
SamplerState pointClampSampler : register(s2);

Texture2D<float> ssao : register(t0);
Texture2D iblDFG : register(t1);

StructuredBuffer<GlobalProbe> globalProbes : register(t2);
StructuredBuffer<Light> lights : register(t3);
StructuredBuffer<ShadowData> shadowData : register(t4);

StructuredBuffer<uint> lightIndexList : register(t5); //MAX_CLUSTER_LIGHTS * 16^3
StructuredBuffer<LightGrid> lightGrid : register(t6); //16^3

Texture2D depthAtlas : register(t7);
Texture2DArray depthCSM : register(t8);

TextureCube globalDiffuse : register(t9);
TextureCube globalSpecular : register(t10);

float3 PrefilteredDFG_LUT(float lod, float NoV)
{
	// coord = sqrt(linear_roughness), which is the mapping used by cmgen.
	return iblDFG.SampleLevel(linearWrapSampler, float2(NoV, lod), 0.0).rgb;
}

float3 PrefilteredDFG(float perceptualRoughness, float NoV)
{
	// PrefilteredDFG_LUT() takes a LOD, which is sqrt(roughness) = perceptualRoughness
	return PrefilteredDFG_LUT(perceptualRoughness, NoV);
}

PixelParams ComputeSurfaceProps(float3 pos, float3 V, Material material)
{
	float reflectance0 = ComputeDielectricF0(material.reflectance);
	PixelParams pixel;
	pixel.Pos = pos;
	pixel.N = material.normal;
	pixel.V = V;
	pixel.F0 = ComputeF0(material.baseColor, material.metallic, reflectance0);
	pixel.NdotV = dot(material.normal, V);
	pixel.DiffuseColor = ComputeDiffuseColor(material.baseColor, material.metallic);
	float perceptualRoughness = material.roughness;
	pixel.PerceptualRoughnessUnclamped = perceptualRoughness;
	pixel.PerceptualRoughness = clamp(perceptualRoughness, MIN_PERCEPTUAL_ROUGHNESS, 1);
	pixel.Roughness = PerceptualRoughnessToRoughness(pixel.PerceptualRoughness);
	pixel.DFG = PrefilteredDFG(pixel.PerceptualRoughness, pixel.NdotV);
	pixel.EnergyCompensation = 1.0 + pixel.F0 * (1.0 / pixel.DFG.y - 1.0);
	return pixel;
}

PixelParams ComputeSurfaceProps(float3 pos, float3 V, float3 N, float3 baseColor, float roughness, float metallic, float reflectance)
{
	float reflectance0 = ComputeDielectricF0(reflectance);
	PixelParams pixel;
	pixel.Pos = pos;
	pixel.N = N;
	pixel.V = V;
	pixel.F0 = ComputeF0(baseColor, metallic, reflectance0);
	pixel.NdotV = dot(N, V);
	pixel.DiffuseColor = ComputeDiffuseColor(baseColor, metallic);
	float perceptualRoughness = roughness;
	pixel.PerceptualRoughnessUnclamped = perceptualRoughness;
	pixel.PerceptualRoughness = clamp(perceptualRoughness, MIN_PERCEPTUAL_ROUGHNESS, 1);
	pixel.Roughness = PerceptualRoughnessToRoughness(pixel.PerceptualRoughness);
	pixel.DFG = PrefilteredDFG(pixel.PerceptualRoughness, pixel.NdotV);
	pixel.EnergyCompensation = 1.0 + pixel.F0 * (1.0 / pixel.DFG.y - 1.0);
	return pixel;
}

float3 FresnelSchlickRoughness(float3 F0, float cosTheta, float roughness)
{
	return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float3 IsotropicLobe(const PixelParams pixel, const LightParams light, const float3 h, float NoV, float NoL, float NoH, float LoH)
{
	float D = distribution(pixel.Roughness, NoH, h);
	float V = visibility(pixel.Roughness, NoV, NoL);
	float3 F = fresnel(pixel.F0, LoH);

	return (D * V) * F;
}

float3 SpecularLobe(const PixelParams pixel, const LightParams light, const float3 h, float NdotV, float NdotL, float NdotH, float LdotH)
{
#if defined(MATERIAL_HAS_ANISOTROPY)
	return AnisotropicLobe(pixel, light, h, NdotV, NdotL, NdotH, LdotH);
#else
	return IsotropicLobe(pixel, light, h, NdotV, NdotL, NdotH, LdotH);
#endif
}

float3 DiffuseLobe(PixelParams pixel, float NdotV, float NdotL, float LdotH)
{
	return pixel.DiffuseColor * diffuse(pixel.Roughness, pixel.NdotV, NdotL, LdotH);
}

float3 SurfaceShading(PixelParams pixel, LightParams light)
{
	float3 h = normalize(pixel.V + light.L);

	float NdotV = pixel.NdotV;
	float NdotL = saturate(light.NdotL);
	float NdotH = saturate(dot(pixel.N, h));
	float LdotH = saturate(dot(light.L, h));

	float3 Fr = SpecularLobe(pixel, light, h, NdotV, NdotL, NdotH, LdotH);
	float3 Fd = DiffuseLobe(pixel, pixel.NdotV, NdotL, LdotH);
	float3 color = Fd + Fr * pixel.EnergyCompensation;

	return (color * light.color.rgb) * (light.color.w * light.attenuation * NdotL);
}

float3 BRDF_IBL(SamplerState samplerState, TextureCube irradianceTex, TextureCube prefilterMap, Texture2D brdfLUT, float3 F0, float3 N, float3 V, float3 albedo, float roughness)
{
	float3 irradiance = irradianceTex.Sample(samplerState, N).rgb;
	float3 kS = FresnelSchlickRoughness(F0, max(dot(N, V), 0.0), roughness);
	float3 kD = 1.0 - kS;
	float3 diffuse = irradiance * albedo;

	float3 R = reflect(-V, N);
	const float MAX_REFLECTION_LOD = 4.0;

	float3 prefilteredColor = prefilterMap.SampleLevel(samplerState, R, roughness * MAX_REFLECTION_LOD).rgb;
	float2 brdf = brdfLUT.Sample(samplerState, float2(max(dot(N, V), 0.0), roughness)).rg;
	float3 specular = prefilteredColor * (kS * brdf.x + brdf.y);

	return kD * diffuse + specular;
}

float3 BRDF_IBL(PixelParams surface)
{
	return 0;
}

float3 DirectionalLightBRDF(Light light, PixelParams pixel)
{
	float3 L = normalize(-light.direction.xyz);
	LightParams clight;
	clight.color = light.color;
	clight.L = L;
	clight.attenuation = 1;
	clight.position = light.position.xyz;
	clight.NdotL = saturate(dot(pixel.N, L));
	clight.direction = light.direction.xyz;
	clight.range = light.range;
	clight.castsShadows = light.castsShadows;
	clight.type = light.type;
	clight.shadowMapIndex = light.shadowMapIndex;

	bool castsShadows = GetBit(light.castsShadows, 0);
	bool contactShadows = GetBit(light.castsShadows, 1);

	float visibility = 1;

	if (castsShadows)
	{
		ShadowData data = shadowData[light.shadowMapIndex];
		visibility = ShadowFactorDirectionalLightCascaded(linearClampSampler, depthCSM, data, pixel.Pos, clight.NdotL);
	}

	return SurfaceShading(pixel, clight) * visibility;
}

float3 PointLightBRDF(Light light, PixelParams pixel)
{
	float3 LN = light.position.xyz - pixel.Pos;
	float distance = length(LN);
	float3 L = normalize(LN);
	float attenuation = Attenuation(distance, light.range);

	LightParams clight;
	clight.color = light.color;
	clight.L = L;
	clight.attenuation = attenuation;
	clight.position = light.position.xyz;
	clight.NdotL = saturate(dot(pixel.N, L));
	clight.direction = light.direction.xyz;
	clight.range = light.range;
	clight.castsShadows = light.castsShadows;
	clight.type = light.type;
	clight.shadowMapIndex = light.shadowMapIndex;

	bool castsShadows = GetBit(light.castsShadows, 0);
	bool contactShadows = GetBit(light.castsShadows, 1);

	float visibility = 1;

	if (castsShadows)
	{
		ShadowData data = shadowData[light.shadowMapIndex];
		visibility = ShadowFactorPointLight(linearClampSampler, depthAtlas, light, data, pixel.Pos, clight.NdotL);
	}

	return SurfaceShading(pixel, clight) * visibility;
}

float3 SpotlightBRDF(Light light, PixelParams pixel)
{
	float3 LN = light.position.xyz - pixel.Pos;
	float3 L = normalize(LN);

	float theta = dot(L, normalize(-light.direction.xyz));
	if (theta > light.outerCosine)
	{
		float distance = length(LN);
		float epsilon = light.innerCosine - light.outerCosine;
		float falloff = 1;
		if (epsilon != 0)
		{
			falloff = smoothstep(0.0, 1.0, (theta - light.innerCosine) / epsilon);
		}

		float attenuation = Attenuation(distance, light.range);
		LightParams clight;
		clight.color = light.color;
		clight.L = L;
		clight.attenuation = attenuation * falloff;
		clight.position = light.position.xyz;
		clight.NdotL = saturate(dot(pixel.N, L));
		clight.direction = light.direction.xyz;
		clight.range = light.range;
		clight.castsShadows = light.castsShadows;
		clight.type = light.type;
		clight.shadowMapIndex = light.shadowMapIndex;
		bool castsShadows = GetBit(light.castsShadows, 0);
		bool contactShadows = GetBit(light.castsShadows, 1);

		float visibility = 1;

		if (castsShadows)
		{
			ShadowData data = shadowData[light.shadowMapIndex];
			visibility = ShadowFactorSpotlight(linearClampSampler, depthAtlas, light, data, pixel.Pos, clight.NdotL);
		}

		return SurfaceShading(pixel, clight) * visibility;
	}

	return 0;
}

float3 ComputeDirectLightning(float depth, PixelParams surface)
{
	float3 Lo = float3(0, 0, 0);

	uint tileIndex = GetClusterIndex(depth, camNear, camFar, screenDim, float4(surface.Pos, 1));

	uint lightCount = lightGrid[tileIndex].lightCount;
	uint lightOffset = lightGrid[tileIndex].lightOffset;

	[loop]
		for (uint i = 0; i < lightCount; i++)
		{
			float3 L = 0;

			uint lightIndex = lightIndexList[lightOffset + i];
			Light light = lights[lightIndex];

			[branch]
				switch (light.type)
				{
				case POINT_LIGHT:
					L += PointLightBRDF(light, surface);
					break;
				case SPOT_LIGHT:
					L += SpotlightBRDF(light, surface);
					break;
				case DIRECTIONAL_LIGHT:
					L += DirectionalLightBRDF(light, surface);
					break;
				}

			Lo += L;
		}

	return Lo;
}

float3 ComputeIndirectLightning(
	float2 screenUV,
	PixelParams pixel,
	float ao,
	float3 emissive)
{
	float3 ambient = pixel.DiffuseColor * ambient_color.rgb;

	ao *= ssao.Sample(linearClampSampler, screenUV);

	ambient = (ambient + BRDF_IBL(pixel)) * ao;
	ambient += emissive;

	return ambient;
}

#endif