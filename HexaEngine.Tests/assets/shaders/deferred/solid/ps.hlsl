////////////////////////////////////////////////////////////////////////////////
// Filename: light.ps
////////////////////////////////////////////////////////////////////////////////
#include "../../gbuffer.hlsl" 
#include "../../brdf.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"
#define Shininess 20.0;
#define SHADOW_SAMPLE_COUNT 17;
#define SHADOW_SAMPLE_COUNT_HALF 8;
#define SHADOW_SAMPLE_COUNT_HALF_NEG -8;
#define SHADOW_SAMPLE_COUNT_SQUARE 289;
#define PI 3.14159265359;

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

Texture2D depthMapTexture : register(t12);

SamplerState SampleTypePoint : register(s0);

//////////////
// TYPEDEFS //
//////////////
struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float ShadowCalculation(DirectionalLightSD light, float4 projectedEyeDir, float bias)
{
    projectedEyeDir = mul(projectedEyeDir, light.view);
    projectedEyeDir = mul(projectedEyeDir, light.proj);
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
    float3 V = normalize(GetCameraPos() - position);
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), albedo, metalness);

    float3 Lo = float3(0.0f, 0.0f, 0.0f);
    
    for (int x = 0; x < directionalLightCount; x++)
    {
        DirectionalLight light = directionalLights[x];
        Lo += BRDFDirect(light.color.rgb, normalize(-light.dir), F0, V, N, albedo, roughness, metalness);
    }
    
    for (int y = 0; y < directionalLightSDCount; y++)
    {
        DirectionalLightSD light = directionalLightSDs[y];
        float bias = max(0.05f * (1.0 - dot(N, V)), 0.005f);
        float shadow = ShadowCalculation(light, float4(attrs.pos, 1), 0.005f);
        Lo += (1.0f - shadow) * BRDFDirect(light.color.rgb, normalize(-light.dir), F0, V, N, albedo, roughness, metalness);
    }
    
    for (int z = 0; z < pointLightCount; z++)
    {
        PointLight light = pointLights[z];
        float3 LN = light.position - position;
        float distance = length(LN);
        float3 L = normalize(LN);
        
        float attenuation = 1.0 / (distance * distance);
        float3 radiance = light.color.rgb * attenuation;
        
        Lo += BRDFDirect(radiance, L, F0, V, N, albedo, roughness, metalness);
    }
    
    for (int w = 0; w < spotlightCount; w++)
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
            Lo += BRDFDirect(radiance, L, F0, V, N, albedo, roughness, metalness);
        }
    }
		
    float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
    float3 ambient = BRDFIndirect2(SampleTypePoint, irradianceTexture, prefilterTexture, brdfLUT, F0, N, V, albedo, roughness, ao);
	
    float3 color = ambient + Lo;
    
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
    float4 color = ComputeLightingPBR(pixel, attrs);
    color.rgb = OECF_sRGBFast(color.rgb);
    color.rgb = ACESFilm(color.rgb);
    return color;
}

