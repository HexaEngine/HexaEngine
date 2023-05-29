#include "defs.hlsl"
#include "../../gbuffer.hlsl"
#include "../../material.hlsl"

SamplerState state;
Texture2DArray colorTex;
Texture2D maskTex;

GeometryData main(PixelInput input)
{
	float3 c0 = colorTex.Sample(state, float3(input.tex, 0)).xyz;
	float3 c1 = colorTex.Sample(state, float3(input.tex, 1)).xyz;
	float3 c2 = colorTex.Sample(state, float3(input.tex, 2)).xyz;

	float3 mask = maskTex.Sample(state, input.ctex).xyz;

	float3 color = float3(0.0f, 0.0f, 0.0f);
	color = lerp(color, c0, mask.r);
	color = lerp(color, c1, mask.g);
	color = lerp(color, c2, mask.b);

	return PackGeometryData(color, 1, input.pos.xyz, 0, input.normal, 0.8f, 0, float3(0, 0, 0), float3(0, 0, 0), 0, 1, 0.5f, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0);
}