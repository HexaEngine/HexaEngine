#include "defs.hlsl"

cbuffer offsetBuffer
{
	uint offset;
}

cbuffer lightBuffer : register(b1)
{
	float4x4 lightView;
	float4x4 lightViewProj;
	float3 lightPosition;
	float lightFar;
	float esmExponent;
};

float GetLinearDepth(float depth, float near, float far)
{
	float z_b = depth;
	float z_n = 2.0 * z_b - 1.0;
	float z_e = 2.0 * near * far / (far + near - z_n * (far - near));
	return z_e;
}

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

#if VtxSkinned

StructuredBuffer<float4x4> boneMatrices;
StructuredBuffer<uint> boneMatrixOffsets;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID) : SV_Position
{
	float4 totalPosition = 0;

	uint boneMatrixOffset = boneMatrixOffsets[instanceId + offset];
	for (int i = 0; i < MaxBoneInfluence; i++)
	{
		if (input.boneIds[i] == -1)
			continue;
		if (input.boneIds[i] >= MaxBones)
		{
			totalPosition = float4(input.position, 1.0f);
			break;
		}

		float4 localPosition = mul(float4(input.position, 1.0f), boneMatrices[input.boneIds[i] + boneMatrixOffset]);
		totalPosition += localPosition * input.weights[i];
	}

	float4x4 world = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	PixelInput output;
	output.position = mul(totalPosition, world);
	output.pos = mul(output.position, lightView);
	output.position = mul(output, lightViewProj);

	return output;
}

#else

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	float4x4 world = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	PixelInput output;
	output.position = mul(float4(input.position, 1), world);
	output.pos = mul(output.position, lightView);
	output.position = mul(output.position, lightViewProj);

	return output;
}
#endif