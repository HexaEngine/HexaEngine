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
    float4 color : COLOR;
};

cbuffer cb : register(b0)
{
	uint typeId;
}



PixelInput main(VertexInput input, uint instanceId : SV_InstanceID, uint vertexId : SV_VertexID)
{
	PixelInput output;

	output.pos = float4(input.pos, 1);
    output.pos = mul(output.pos, viewProj);
	output.color = uint4(instanceId + 1, typeId + 1, 1, vertexId + 1);
	return output;
}