#include "../../camera.hlsl"

struct VertexInput
{
	float3 pos : POSITION;
	float2 tex : TEXCOORD0;
	float3 normal : NORMAL;
	float3 tangent : TANGENT;
};

struct PixelInput
{
	float4 pos : SV_POSITION;
	nointerpolation uint4 color : COLOR;
};

cbuffer cb : register(b0)
{
	uint typeId;
}

StructuredBuffer<float4x4> instances;
StructuredBuffer<uint> offsets;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	PixelInput output;

	float4x4 mat = instances[instanceId + offsets[typeId]];
	output.pos = mul(float4(input.pos, 1), mat);
	output.pos = mul(output.pos, view);
	output.pos = mul(output.pos, proj);
	output.color = uint4(instanceId + 1, typeId + 1, 0, 1);
	return output;
}