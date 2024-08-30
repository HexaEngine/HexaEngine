#include "defs.hlsl"

cbuffer offsetBuffer
{
	uint offset;
}

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

#if HasBakedLightMap || BAKE_PASS
Buffer<float4> BakedVertexData : register(t2);
#endif

#if Tessellation
HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	HullInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	output.pos = mul(float4(input.pos, 1), mat).xyz;
	output.tex = input.tex;
	output.normal = mul(input.normal, (float3x3)mat);
	output.tangent = mul(input.tangent, (float3x3)mat);

	output.TessFactor = TessellationFactor;
	return output;
}

#elif VtxSkinned

#include "../../camera.hlsl"

StructuredBuffer<float4x4> boneMatrices;
StructuredBuffer<uint> boneMatrixOffsets;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	float4 totalPosition = 0;
	float3 totalNormal = 0;
	float3 totalTangent = 0;

	uint boneMatrixOffset = boneMatrixOffsets[instanceId + offset];
	for (int i = 0; i < MaxBoneInfluence; i++)
	{
		if (input.boneIds[i] == -1)
			continue;
		if (input.boneIds[i] >= MaxBones)
		{
			totalPosition = float4(input.pos, 1.0f);
			totalNormal = input.normal;
			totalTangent = input.tangent;
			break;
		}

		float4 localPosition = mul(float4(input.pos, 1.0f), boneMatrices[input.boneIds[i] + boneMatrixOffset]);
		totalPosition += localPosition * input.weights[i];

		float3 localNormal = mul(input.normal, (float3x3)boneMatrices[input.boneIds[i] + boneMatrixOffset]);
		totalNormal += localNormal * input.weights[i];

		float3 localTangent = mul(input.tangent, (float3x3)boneMatrices[input.boneIds[i] + boneMatrixOffset]);
		totalTangent += localTangent * input.weights[i];
	}

	PixelInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	output.position = mul(totalPosition, mat).xyzw;
	output.pos = output.position;
	output.tex = input.tex;
	output.normal = mul(totalNormal, (float3x3)mat);
	output.tangent = mul(totalTangent, (float3x3)mat);
	output.position = mul(output.position, viewProj);

	return output;
}

#else

#include "../../camera.hlsl"

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID, uint vertexId : SV_VertexID)
{
	PixelInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

	output.position = mul(float4(input.pos, 1), mat).xyzw;
	output.pos = output.position.xyz;
	output.tex = input.tex;
	output.normal = mul(input.normal, (float3x3)mat);
	output.tangent = mul(input.tangent, (float3x3)mat);
	output.position = mul(output.position, viewProj);

#if HasBakedLightMap || BAKE_PASS
	uint location = vertexId * 3;
	float4 sample0 = BakedVertexData.Load(location + 0);
	float4 sample1 = BakedVertexData.Load(location + 1);
	float4 sample2 = BakedVertexData.Load(location + 2);

	output.H0 = float3(sample0.x, sample1.x, sample2.x);
	output.H1 = float3(sample0.y, sample1.y, sample2.y);
	output.H2 = float3(sample0.z, sample1.z, sample2.z);
	output.H3 = float3(sample0.w, sample1.w, sample2.w);
#endif

	return output;
}
#endif