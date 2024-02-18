#include "../../commonShading.hlsl"

Texture2D GBufferA : register(t11);
Texture2D GBufferB : register(t12);
Texture2D GBufferC : register(t13);
Texture2D GBufferD : register(t14);
Texture2D<float> Depth : register(t15);

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut pixel) : SV_TARGET
{
    float depth = Depth.Sample(linearWrapSampler, pixel.Tex);
    float3 position = GetPositionWS(pixel.Tex, depth);
    GeometryAttributes attrs;
    ExtractGeometryData(pixel.Tex, GBufferA, GBufferB, GBufferC, GBufferD, linearWrapSampler, attrs);

    float3 N = normalize(attrs.normal);
    float3 VN = camPos - position;
    float3 V = normalize(VN);

    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), attrs.baseColor, attrs.metallic);

    float3 direct = ComputeDirectLightning(depth, position, V, N, attrs.baseColor, F0, attrs.roughness, attrs.metallic);
    float3 ambient = ComputeIndirectLightning(pixel.Tex, V, N, attrs.baseColor, F0, attrs.roughness, attrs.metallic, attrs.ao, attrs.emission);

    return float4(ambient + direct, 1);
}