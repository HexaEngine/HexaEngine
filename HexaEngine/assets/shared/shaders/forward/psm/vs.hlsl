#include "defs.hlsl"

cbuffer cb
{
	uint offset;
}

StructuredBuffer<float4x4> instances;
StructuredBuffer<uint> offsets;

HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	HullInput output;

	float4x4 mat = instances[instanceId + offsets[offset]];
	output.pos = mul(float4(input.pos, 1), mat).xyz;
	output.tex = input.tex;
	output.normal = mul(input.normal, (float3x3)mat);

	output.TessFactor = 1;
	return output;
}