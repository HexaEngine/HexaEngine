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

float4 ComputeLightingPBR(VSOut input, GeometryAttributes attrs)
{
	float3 position = attrs.pos;
	float3 baseColor = attrs.albedo;

    float specular = attrs.specular;
    float specularTint = attrs.speculartint;
    float sheen = attrs.sheen;
    float sheenTint = attrs.sheentint;
    float clearcoat = attrs.clearcoat;
    float clearcoatGloss = attrs.clearcoatGloss;
	float anisotropic = attrs.anisotropic;
    float subsurface = attrs.subsurface;
	float roughness = attrs.roughness;
	float metalness = attrs.metalness;

	float3 N = normalize(attrs.normal);
	float3 X = normalize(attrs.tangent);
	float3 Y = normalize(cross(N, X));

	float3 V = normalize(GetCameraPos() - position);

	//float IOR = 1.5;
	//float3 F0 = float3(pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0), pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0), pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0));
	float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metalness);

	float3 Lo = 0;

	for (uint x = 0; x < directionalLightCount; x++)
	{
		DirectionalLight light = directionalLights[x];
		float3 L = normalize(-light.dir);
		float3 radiance = light.color.rgb;
		Lo += BRDF(L, V, N, X, Y, baseColor, specular, specularTint, metalness, roughness, sheen, sheenTint, clearcoat, clearcoatGloss, anisotropic, subsurface) * radiance;
	}

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

	return float4(Lo, 1);
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