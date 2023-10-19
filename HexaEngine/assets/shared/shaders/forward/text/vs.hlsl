#include "../../camera.hlsl"

cbuffer WorldBuffer
{
	float4x4 model;
	float2 screenSize;
	float2 atlasSize;
};

struct VSInput
{
	float3 pos : POSITION;
	float2 tex : TexCoord;
};

struct PSInput
{
	float4 pos : SV_POSITION;
	float2 tex : TexCoord;
};

PSInput main(VSInput vin)
{
	PSInput output;
	output.pos = mul(float4(vin.pos / screenSize, 1), model);
	output.pos = mul(output.pos, viewProj);
	output.tex = vin.tex / atlasSize;
	return output;
}