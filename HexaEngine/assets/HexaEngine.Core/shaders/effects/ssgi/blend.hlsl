#include "../../camera.hlsl"

Texture2D inputTex;
Texture2D indirectTex;

SamplerState linearWrapSampler;

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
	float3 direct = inputTex.Sample(linearWrapSampler, uv).rgb;
	float3 indirect = saturate(indirectTex.Sample(linearWrapSampler, uv).rgb);

    return float4(direct + indirect * intensity, 1.0);
}