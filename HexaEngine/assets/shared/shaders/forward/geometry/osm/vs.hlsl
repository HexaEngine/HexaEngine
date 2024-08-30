#include "defs.hlsl"

cbuffer offsetBuffer
{
	uint offset;
}

cbuffer lightBuffer : register(b1)
{
	matrix view;
	float3 lightPosition;
	float lightFar;
};

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

#if Tessellation
HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	HullInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	output.pos = mul(float4(input.pos, 1), mat).xyz;

	output.TessFactor = TessellationFactor;
	return output;
}

#elif VtxSkinned

StructuredBuffer<float4x4> boneMatrices;
StructuredBuffer<uint> boneMatrixOffsets;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	float4 totalPosition = 0;

	uint boneMatrixOffset = boneMatrixOffsets[instanceId + offset];
	for (int i = 0; i < MaxBoneInfluence; i++)
	{
		if (input.boneIds[i] == -1)
			continue;
		if (input.boneIds[i] >= MaxBones)
		{
			totalPosition = float4(input.pos, 1.0f);
			break;
		}

		float4 localPosition = mul(float4(input.pos, 1.0f), boneMatrices[input.boneIds[i] + boneMatrixOffset]);
		totalPosition += localPosition * input.weights[i];
	}

	PixelInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	output.position = mul(totalPosition, mat).xyzw;
	output.position = mul(output.position, view);
	output.depth = output.position.z / output.position.w;

	return output;
}

#else

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	PixelInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	output.position = mul(float4(input.pos, 1), mat).xyzw;
	float3 L = output.position.xyz - lightPosition;
	output.position = mul(output.position, view);
	output.depth = length(L) / lightFar;

	return output;
}
#endif