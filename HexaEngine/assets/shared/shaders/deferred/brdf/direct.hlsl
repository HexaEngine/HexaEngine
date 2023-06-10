////////////////////////////////////////////////////////////////////////////////
// Filename: light.ps
////////////////////////////////////////////////////////////////////////////////
#include "../../gbuffer.hlsl"
//#include "../../brdf.hlsl"
#include "../../brdf2.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"

Texture2D GBufferA : register(t0);
Texture2D GBufferB : register(t1);
Texture2D GBufferC : register(t2);
Texture2D GBufferD : register(t3);
Texture2D<float> Depth : register(t4);
Texture2D ssao : register(t8);

StructuredBuffer<DirectionalLight> directionalLights : register(t9);
StructuredBuffer<PointLight> pointLights : register(t10);
StructuredBuffer<Spotlight> spotlights : register(t11);

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

float4 ComputeLightingPBR(VSOut input, float3 position, GeometryAttributes attrs)
{
    float3 baseColor = attrs.baseColor;
    float roughness = attrs.roughness;
    float metallic = attrs.metallic;

    float3 N = normalize(attrs.normal);
    float3 V = normalize(GetCameraPos() - position);
    
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metallic);

    float3 Lo = 0;

    for (uint x = 0; x < directionalLightCount; x++)
    {
        DirectionalLight light = directionalLights[x];
        float3 L = normalize(-light.dir);
        float3 radiance = light.color.rgb;
        
        Lo += BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
    }

    for (uint z = 0; z < pointLightCount; z++)
    {
        PointLight light = pointLights[z];
        float3 LN = light.position - position;
        float distance = length(LN);
        float3 L = normalize(LN);

        float attenuation = 1.0 / (distance * distance);
        float3 radiance = light.color.rgb * attenuation;

        Lo += BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
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
            Lo += BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metallic);
        }
    }
    float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
    return float4(Lo * ao, 1);
}

float4 main(VSOut pixel) : SV_TARGET
{
    float depth = Depth.Sample(SampleTypePoint, pixel.Tex);
    float3 position = GetPositionWS(pixel.Tex, depth);
    GeometryAttributes attrs;
    ExtractGeometryData(pixel.Tex, GBufferA, GBufferB, GBufferC, GBufferD, SampleTypePoint, attrs);

    return ComputeLightingPBR(pixel, position, attrs);
}