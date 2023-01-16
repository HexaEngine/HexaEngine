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
TextureCube irradianceTexture : register(t8);
TextureCube prefilterTexture : register(t9);
Texture2D brdfLUT : register(t10);
Texture2D ssao : register(t11);

Texture2DArray depthCSM : register(t12);
TextureCube depthOSM[8] : register(t13);
Texture2D depthPSM[8] : register(t21);

SamplerState SampleTypePoint : register(s0);
SamplerState SampleTypeAnsio : register(s1);

//////////////
// TYPEDEFS //
//////////////
struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float4 ComputeLighting(VSOut input, GeometryAttributes attrs)
{
	float3 color = attrs.albedo;
	return float4(color, attrs.opacity);
}

float3 ACESFilm(float3 x)
{
	return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 OECF_sRGBFast(float3 color)
{
	float gamma = 2.2;
	return pow(color.rgb, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));
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
	SampleTypePoint,
	attrs);
	float4 color = ComputeLighting(pixel, attrs);
	color.rgb = OECF_sRGBFast(color.rgb);
	color.rgb = ACESFilm(color.rgb);
	return color;
}