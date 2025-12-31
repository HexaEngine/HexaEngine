#include "HexaEngine.Core:shaders/camera.hlsl"

Texture2D inputTex;
Texture2D indirectTex;
Texture2D GBufferA;
Texture2D<float> ssao;

SamplerState linearClampSampler;

cbuffer SSGIParams : register(b0)
{
	float intensity;
};

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float4 main(VertexOut input) : SV_TARGET
{
	float2 uv = input.Tex;
    float3 direct = inputTex.Sample(linearClampSampler, uv).rgb;
    float3 indirect = saturate(indirectTex.Sample(linearClampSampler, uv).rgb);
    float3 baseColor = GBufferA.SampleLevel(linearClampSampler, uv, 0).rgb;
    float ao = ssao.Sample(linearClampSampler, uv).r;
    float3 finalIndirect = indirect * baseColor * ao * intensity;
    
    return float4(direct + finalIndirect, 1.0);
}