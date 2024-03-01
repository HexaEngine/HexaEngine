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

float4 main(VSOut pin) : SV_TARGET
{
    float depth = Depth.Sample(linearWrapSampler, pin.Tex);
    if (depth == 1)
        discard;
    float3 position = GetPositionWS(pin.Tex, depth);
    GeometryAttributes attrs;
    ExtractGeometryData(pin.Tex, GBufferA, GBufferB, GBufferC, GBufferD, linearWrapSampler, attrs);

    float3 N = normalize(attrs.normal);
    float3 VN = camPos - position;
    float3 V = normalize(VN);

    PixelParams pixel = ComputeSurfaceProps(position, V, N, attrs.baseColor, attrs.roughness, attrs.metallic, attrs.reflectance);

    float3 direct = ComputeDirectLightning(depth, pixel);
    float3 ambient = ComputeIndirectLightning(pin.Tex, pixel, attrs.ao, attrs.emission);

    return float4(ambient + direct, 1);
}