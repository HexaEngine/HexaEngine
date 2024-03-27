#include "../../camera.hlsl"

Texture2D sceneTexture : register(t0);
Texture2D blurredTexture : register(t1);
Texture2D<float> cocTex : register(t2);

SamplerState linearWrapSampler : register(s0);

float GetCircleOfConfusion(float2 tex)
{
	float coc = cocTex.SampleLevel(linearWrapSampler, tex, 0) * 2 - 1;
	return abs(coc);
}

float3 DistanceDOF(float3 colorFocus, float3 colorBlurred, float2 tex)
{
	float coc = GetCircleOfConfusion(tex);
	return lerp(colorFocus, colorBlurred, coc);
}

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float4 main(VertexOut pin) : SV_TARGET
{
	float4 color = sceneTexture.Sample(linearWrapSampler, pin.Tex);
	float3 colorBlurred = blurredTexture.Sample(linearWrapSampler, pin.Tex).xyz;
	color = float4(DistanceDOF(color.xyz, colorBlurred, pin.Tex), 1.0);

	return color;
}