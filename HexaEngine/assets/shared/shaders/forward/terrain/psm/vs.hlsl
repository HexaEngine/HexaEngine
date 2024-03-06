#include "defs.hlsl"

cbuffer WorldBuffer
{
	float4x4 world;
};

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
	output.position = mul(float4(input.pos, 1), world);
	output.pos = mul(output.position, lightView);
	output.position = mul(output.position, lightViewProj);
	return output;
}