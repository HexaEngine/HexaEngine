#include "../../camera.hlsl"
#include "../../gbuffer.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer BentNormalsParams
{
    float SampleRadius;
};

Texture2D depthTex : register(t0);
Texture2D normalTex : register(t1);

SamplerState samplerState;

const float3 kernel[8] =
{
    float3(1, 0, 0),
    float3(-1, 0, 0),
    float3(0, 1, 0),
    float3(0, -1, 0),
    float3(1, 1, 0),
    float3(-1, -1, 0),
    float3(1, -1, 0),
    float3(-1, 1, 0)
};

float4 main(VSOut pin) : SV_Target
{
    float depth = depthTex.SampleLevel(samplerState, pin.Tex, 0);
    float3 viewPosition = GetPositionVS(pin.Tex, depth);

    float3 normalSample = normalTex.SampleLevel(samplerState, pin.Tex, 0);
    float3 normal = mul(UnpackNormal(normalSample), (float3x3) view);

    float3 bentNormals = normalize(normal);

    for (int i = 0; i < 8; ++i)
    {
        float3 offset = kernel[i] * SampleRadius;
        float3 samplePos = viewPosition + offset;

        float3 sampleNormal = normalize(samplePos - viewPosition);

        bentNormals += sampleNormal;
    }

    return float4(PackNormal(normalize(bentNormals)), 1);
}