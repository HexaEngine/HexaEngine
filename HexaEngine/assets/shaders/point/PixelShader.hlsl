////////////////////////////////////////////////////////////////////////////////
// Filename: light.ps
////////////////////////////////////////////////////////////////////////////////
#include "../common.hlsl" 
#define Shininess 20.0;
#define SHADOW_SAMPLE_COUNT 17;
#define SHADOW_SAMPLE_COUNT_HALF 8;
#define SHADOW_SAMPLE_COUNT_HALF_NEG -8;
#define SHADOW_SAMPLE_COUNT_SQUARE 289;
#define PI 3.14159265359;

struct DirectionalLight
{
    float4 Color;
    float3 Position;  
    float Range;
};

/////////////
// GLOBALS //
/////////////
Texture2D colorTexture : register(t0);
Texture2D positionTexture : register(t1);
Texture2D normalTexture : register(t2);
Texture2D emissiveTexture : register(t3);

Texture2D depthMapTexture : register(t5);
TextureCube environmentTexture : register(t6);

///////////////////
// SAMPLE STATES //
///////////////////
SamplerState SampleTypePoint : register(s0);

//////////////////////
// CONSTANT BUFFERS //
//////////////////////

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
struct PixelInputType
{
	float4 position : SV_POSITION;
	float2 tex : TEXCOORD0;
};

struct GBufferAttributes
{
	float4 color;
	float4 position;
	float4 normal;
	float4 specular;
	float4 emissive;
};

void ExtractGBufferAttributes(in PixelInputType pixel,
	in Texture2D color,
	in Texture2D pos,
	in Texture2D normal,
	in Texture2D specular,
	in Texture2D emissive,
	out GBufferAttributes attrs)
{
    float2 screenPos = (float2) pixel.tex;
    
    attrs.color = (float4) color.Sample(SampleTypePoint, screenPos);
    attrs.position = (float4) pos.Sample(SampleTypePoint, screenPos);
    attrs.normal = (float4) normal.Sample(SampleTypePoint, screenPos);
    attrs.specular = (float4) specular.Sample(SampleTypePoint, screenPos);
    attrs.emissive = (float4) emissive.Sample(SampleTypePoint, screenPos);
}



float3 fresnelSchlick(float cosTheta, float3 F0)
{
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float DistributionGGX(float3 N, float3 H, float roughness)
{
    float a = roughness * roughness;
    float a2 = a * a;
    float NdotH = max(dot(N, H), 0.0);
    float NdotH2 = NdotH * NdotH;
	
    float num = a2;
    float denom = (NdotH2 * (a2 - 1.0) + 1.0);
    denom = 3.14159265359 * denom * denom;
	
    return num / denom;
}

float GeometrySchlickGGX(float NdotV, float roughness)
{
    float r = (roughness + 1.0);
    float k = (r * r) / 8.0;

    float num = NdotV;
    float denom = NdotV * (1.0 - k) + k;
	
    return num / denom;
}

float GeometrySmith(float3 N, float3 V, float3 L, float roughness)
{
    float NdotV = max(dot(N, V), 0.0);
    float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness);
    float ggx1 = GeometrySchlickGGX(NdotL, roughness);
	
    return ggx1 * ggx2;
}

float3 OECF_sRGBFast(float3 color)
{
    float gamma = 1.2;
    return pow(color.rgb, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));
}

float3 Tonemap_ACES(const float3 x)
{
    // Narkowicz 2015, "ACES Filmic Tone Mapping Curve"
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return (x * (a * x + b)) / (x * (c * x + d) + e);
}

float4 ComputeLightingPBR(PixelInputType input, GeometryAttributes attrs)
{
	float3 position = (float3)attrs.pos;
    float3 albedo = attrs.albedo;
	
	float3 N = normalize((float3)attrs.normal);
    float3 V = normalize(viewPos - position);
    float3 F0 = float3(0.04f, 0.04f, 0.04f);
    float3 lightColor = (float3)light.Color;
	
	
    float roughness = attrs.roughness;
    float metalness = attrs.metalness;
	
    F0 = lerp(F0, albedo, metalness);
	
    float3 Lo = float3(0.0f, 0.0f, 0.0f);
	
    float3 L = normalize(-light.LightDirection);
    float3 H = normalize(V + L);
    float distance = length(light.LightDirection);
    float attenuation = 1.0 / (distance * distance);
    float3 radiance = lightColor * attenuation;
        
    // cook-torrance brdf
    float NDF = DistributionGGX(N, H, roughness);
    float G = GeometrySmith(N, V, L, roughness);
    float3 F = fresnelSchlick(max(dot(H, V), 0.0), F0);
        
    float3 kS = F;
    float3 kD = float3(1.0f, 1.0f, 1.0f) - kS;
    kD *= 1.0 - metalness;
        
    float3 numerator = NDF * G * F;
    float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
    float3 specular = numerator / denominator;
            
        // add to outgoing radiance Lo
    float NdotL = max(dot(N, L), 0.0);
    Lo += (kD * albedo / 3.14159265359 + specular) * radiance * NdotL;
	
    float3 ambient = float3(0.03f, 0.03f, 0.03f) * albedo * 1;

	
    float3 color = ambient + Lo;
	
    return float4(color, attrs.opacity);
}

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
float4 main(PixelInputType pixel) : SV_TARGET
{
    GeometryAttributes attrs;
    ExtractGeometryData(pixel.tex, colorTexture, positionTexture, normalTexture, emissiveTexture, SampleTypePoint, attrs);
    return ComputeLightingPBR(pixel, attrs);
}

