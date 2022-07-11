////////////////////////////////////////////////////////////////////////////////
// Filename: light.ps
////////////////////////////////////////////////////////////////////////////////
#include "../gbuffer.hlsl" 
#include "../brdf.hlsl"
#define Shininess 20.0;
#define SHADOW_SAMPLE_COUNT 17;
#define SHADOW_SAMPLE_COUNT_HALF 8;
#define SHADOW_SAMPLE_COUNT_HALF_NEG -8;
#define SHADOW_SAMPLE_COUNT_SQUARE 289;
#define PI 3.14159265359;

struct DirectionalLight
{
    float4 Color;
    float3 LightDirection;
    float reserved;
    matrix view;
    matrix projection;
};

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

SamplerState SampleTypePoint : register(s0);

cbuffer LightBuffer : register(b0)
{
    DirectionalLight light;
};

cbuffer CamBuffer : register(b1)
{
    float3 viewPos;
    float reserved;
    matrix viewMatrix;
    matrix projectionMatrix;
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
    float3 albedo = attrs.albedo;
   
    float roughness = attrs.roughness;
    float metalness = attrs.metalness;
	
    float3 N = normalize(attrs.normal);
    float3 V = normalize(viewPos - position);
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), albedo, metalness);
		
    float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
    float3 ambient = BRDFIndirect2(SampleTypePoint, irradianceTexture, prefilterTexture, brdfLUT, F0, N, V, albedo, roughness, ao);
	
    float3 color = ambient;
    
    return float4(color, attrs.opacity);
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
    return ComputeLightingPBR(pixel, attrs);
}

