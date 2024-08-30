#include "defs.hlsl"

cbuffer offsetBuffer
{
	uint offset;
}

cbuffer lightBuffer : register(b1)
{
	float4x4 view;
	float lightNear;
	float lightFar;
	float hemiDir;
};

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

// Function to transform vertex to DPSM space
inline PixelInput TransformToDPSMSpace(uint instanceId, float3 position)
{
	PixelInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
	output.position = mul(float4(position, 1), mat).xyzw;

	// transform vertex to DP-space
	output.position = mul(output.position, view);
	output.position /= output.position.w;

	// for the back-map z has to be inverted
	output.position.z *= hemiDir;

	// because the origin is at 0 the proj-vector
	// matches the vertex-position
	float len = length(output.position.xyz);

	// normalize
	output.position /= len;

	// save for clipping
	output.clip = output.position.z;

	// calc "normal" on intersection, by adding the
	// reflection-vector(0,0,1) and divide through
	// his z to get the texture coords
	output.position.x /= output.position.z + 1.0f;
	output.position.y /= output.position.z + 1.0f;

	// set z for z-buffering and neutralize w
	output.position.z = (len - lightNear) / (lightFar - lightNear);
	output.position.w = 1.0;

	// DP-depth
	output.depth = output.position.z;

	return output;
}

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

	PixelInput output = TransformToDPSMSpace(instanceId, totalPosition.xyz);
	return output;
}

#else

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	PixelInput output = TransformToDPSMSpace(instanceId, input.pos);
	return output;
}
#endif