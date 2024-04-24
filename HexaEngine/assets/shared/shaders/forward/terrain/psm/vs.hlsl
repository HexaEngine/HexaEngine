#include "defs.hlsl"

cbuffer cb
{
    uint offset;
}

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

cbuffer LightBuffer : register(b1)
{
	float4x4 lightView;
	float4x4 lightViewProj;
	float3 lightPosition;
	float lightFar;
	float esmExponent;
};

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	PixelInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	output.position = mul(float4(input.pos, 1), mat);
	output.pos = mul(output.position, lightView);
	output.position = mul(output.position, lightViewProj);
	return output;
}