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
Texture2D depthMapTexture : register(t8);
TextureCube irradianceTexture : register(t9);
TextureCube prefilterTexture : register(t10);
Texture2D brdfLUT : register(t11);
Texture2D ssao : register(t12);

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

float ShadowCalculation(float4 projectedEyeDir, float bias)
{
    projectedEyeDir = mul(projectedEyeDir, light.view);
    projectedEyeDir = mul(projectedEyeDir, light.projection);
    projectedEyeDir.y = -projectedEyeDir.y;
    float3 projCoords = projectedEyeDir.xyz / projectedEyeDir.w;
    float currentDepth = projCoords.z;
    if (currentDepth > 1.0)
        return 0.0f;
		
    projCoords = projCoords * 0.5f + 0.5f;

    float shadow = 0.0;
    float w;
    float h;
    depthMapTexture.GetDimensions(w, h);
    float2 texelSize = 1.0 / float2(w, h);

	[unroll]
    for (int x = -8; x <= 8; x++)
    {
		[unroll]
        for (int y = -8; y <= 8; y++)
        {
            float pcfDepth = depthMapTexture.Sample(SampleTypePoint, projCoords.xy + float2(x, y) * texelSize).r;
            shadow += currentDepth - bias > pcfDepth ? 1.0 : 0.0;
        }
    }
    shadow /= 289;

    return shadow;
}

float4 ComputeLightingPBR(VSOut input, GeometryAttributes attrs)
{
    float3 position = attrs.pos;
    float3 albedo = attrs.albedo;
    
    
    float roughness = attrs.roughness;
    float metalness = attrs.metalness;
	
    float3 N = normalize(attrs.normal);
    float3 V = normalize(viewPos - position);
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), albedo, metalness);

    float3 Lo = float3(0.0f, 0.0f, 0.0f);
    
    float3 lightColor = light.Color.rgb;
    Lo += BRDFDirect(lightColor, -light.LightDirection, F0, V, N, albedo, roughness, metalness);
		
    float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
    float3 ambient = BRDFIndirect2(SampleTypePoint, irradianceTexture, prefilterTexture, brdfLUT, F0, N, V, albedo, roughness, ao);

	// Shadow
    float bias = max(0.05f * (1.0 - dot(N, V)), 0.005f);
    float shadow = ShadowCalculation(float4(attrs.pos, 1), 0.005f);
	
    float3 color = ambient + (1.0f - shadow) * Lo;
    
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

