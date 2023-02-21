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
Texture2D ssao : register(t8);
TextureCube irradianceTexture : register(t9);
TextureCube prefilterTexture : register(t10);
Texture2D brdfLUT : register(t11);


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
    float metallic = attrs.metalness;

    float3 N = normalize(attrs.normal);
    float3 V = normalize(GetCameraPos() - position);

	//float IOR = 1.5;
	//float3 F0 = float3(pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0), pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0), pow(IOR - 1.0, 2.0) / pow(IOR + 1.0, 2.0));

    float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
    float3 ambient = BRDFIndirect(SampleTypeAnsio, irradianceTexture, prefilterTexture, brdfLUT, N, V, baseColor, metallic, roughness, clearcoat, clearcoatGloss, sheen, sheenTint, ao, anisotropic);

    return float4(ambient + attrs.emission, 1);
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