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
Texture2D brdfLUT : register(t9);
StructuredBuffer<GlobalProbe> globalProbes : register(t10);
TextureCube globalDiffuse[4] : register(t11);
TextureCube globalSpecular[4] : register(t15);

SamplerState SampleTypePoint : register(s0);
SamplerState SampleTypeAnsio : register(s1);

struct Params
{
    uint globalProbeCount;
    uint localProbeCount;
    uint padd0;
    uint padd1;
};

cbuffer paramsBuffer : register(b0)
{
    Params params;
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
    float metallic = attrs.metalness;

    float3 N = normalize(attrs.normal);
    float3 V = normalize(GetCameraPos() - position);
    float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
    float3 ambient = 0;
    
    [unroll(4)]
    for (uint i = 0; i < params.globalProbeCount; i++)
    {
        ambient += BRDFIndirect(SampleTypeAnsio, globalDiffuse[i], globalSpecular[i], brdfLUT, N, V, baseColor, metallic, roughness, clearcoat, clearcoatGloss, sheen, sheenTint, ao, anisotropic);
    }

    
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