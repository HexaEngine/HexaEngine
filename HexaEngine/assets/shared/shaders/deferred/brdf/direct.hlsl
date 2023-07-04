#include "../../gbuffer.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"

Texture2D GBufferA : register(t0);
Texture2D GBufferB : register(t1);
Texture2D GBufferC : register(t2);
Texture2D GBufferD : register(t3);
Texture2D<float> Depth : register(t4);
Texture2D ssao : register(t5);
StructuredBuffer<Light> lights : register(t6);

SamplerState linearClampSampler : register(s0);
SamplerState linearWrapSampler : register(s1);
SamplerState pointClampSampler : register(s2);
SamplerComparisonState shadowSampler : register(s3);

cbuffer constants : register(b0)
{
    uint lightCount;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 ComputeLightingPBR(VSOut input, float3 position, GeometryAttributes attrs)
{
    float3 baseColor = attrs.baseColor;
    float roughness = attrs.roughness;
    float metallic = attrs.metallic;

    float3 N = normalize(attrs.normal);
    float3 V = normalize(GetCameraPos() - position);
    
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metallic);

    float3 Lo = 0;
    
    for (uint i = 0; i < lightCount; i++)
    {
        Light light = lights[i];
        switch (light.type)
        {
            case POINT_LIGHT:
                Lo += PointLightBRDF(light, position, F0, V, N, baseColor, roughness, metallic);
                break;
            case SPOT_LIGHT:
                Lo += SpotlightBRDF(light, position, F0, V, N, baseColor, roughness, metallic);
                break;
            case DIRECTIONAL_LIGHT:
                Lo += DirectionalLightBRDF(light, F0, V, N, baseColor, roughness, metallic);
                break;
        }
    }

    float ao = ssao.Sample(linearWrapSampler, input.Tex).r * attrs.ao;
    return float4(Lo * ao, 1);
}

float4 main(VSOut pixel) : SV_TARGET
{
    float depth = Depth.Sample(linearWrapSampler, pixel.Tex);
    float3 position = GetPositionWS(pixel.Tex, depth);
    GeometryAttributes attrs;
    ExtractGeometryData(pixel.Tex, GBufferA, GBufferB, GBufferC, GBufferD, linearWrapSampler, attrs);

    return ComputeLightingPBR(pixel, position, attrs);
}