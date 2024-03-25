#include "../../camera.hlsl"

cbuffer ModelBuffer
{
	float4x4 model;
};

struct VS_INPUT
{
	float3 pos : POSITION;
	float3 tex : TEXCOORD;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct VS_OUTPUT
{
	float4 PositionWS : POSITION;
	float2 Uvs : TEX;
	float3 NormalWS : NORMAL0;
};

VS_OUTPUT main(VS_INPUT input)
{
	VS_OUTPUT Output;

	float4 pos = mul(float4(input.pos, 1.0), model);
	Output.PositionWS = pos / pos.w;
	Output.Uvs = input.Uvs.xy;
	float3 normal_ws = mul(input.Normal, (float3x3)model);
	Output.NormalWS = normalize(normal_ws);
	return Output;
}