#include "defs.hlsl"

cbuffer offsetBuffer
{
	uint offset;
}

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

void SkinningTransform(uint instanceId, StructuredBuffer<uint> boneMatrixOffsets, StructuredBuffer<float4x4> boneMatrices, float3 position, float3 normal, float3 tangent, int4 boneIds, float4 weights, out float3 totalPosition, out float3 totalNormal, out float3 totalTangent)
{
	totalPosition = 0;
	totalNormal = 0;
	totalTangent = 0;

	uint boneMatrixOffset = boneMatrixOffsets[instanceId + offset];

	[unroll(MaxBoneInfluence)]
		for (int i = 0; i < MaxBoneInfluence; i++)
		{
			if (boneIds[i] == -1)
				continue;
			if (boneIds[i] >= MaxBones)
			{
				totalPosition = position;
				totalNormal = normal;
				totalTangent = tangent;
				break;
			}

			float3 localPosition = mul(position, (float3x3)boneMatrices[boneIds[i] + boneMatrixOffset]);
			totalPosition += localPosition * weights[i];
			float3 localNormal = mul(normal, (float3x3)boneMatrices[boneIds[i] + boneMatrixOffset]);
			totalNormal += localNormal * weights[i];
			float3 localTangent = mul(tangent, (float3x3)boneMatrices[boneIds[i] + boneMatrixOffset]);
			totalTangent += localTangent * weights[i];
		}
}

#if HasBakedLightMap || BAKE_PASS
Buffer<float4> BakedVertexData : register(t2);
#endif

#if Tessellation
HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	HullInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
#if VtxColors
	output.color = input.color;
#endif
	output.pos = mul(float4(input.pos, 1), mat).xyz;
#if VtxUVs
	output.tex = input.tex;
#endif
#if VtxNormals
	output.normal = mul(input.normal, (float3x3)mat);
#endif
#if VtxTangents
	output.tangent = mul(input.tangent, (float3x3)mat);
#endif

	output.TessFactor = TessellationFactor;
	return output;
}

#elif VtxSkinned

#include "../../camera.hlsl"

StructuredBuffer<float4x4> boneMatrices;
StructuredBuffer<uint> boneMatrixOffsets;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	float3 totalPosition = 0;
	float3 totalNormal = 0;
	float3 totalTangent = 0;

	SkinningTransform(instanceId, boneMatrixOffsets, boneMatrices, input.position, input.normal, input.tangent, input.boneIds, input.weights, totalPosition, totalNormal, totalTangent);

	PixelInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
#if VtxColors
	output.color = input.color;
#endif
	output.position = mul(float4(totalPosition, 1), mat).xyzw;
	output.pos = output.position;
#if VtxUVs
	output.tex = input.tex;
#endif
#if VtxNormals
	output.normal = mul(totalNormal, (float3x3)mat);
#endif
#if VtxTangents
	output.tangent = mul(totalTangent, (float3x3)mat);
#endif
	output.position = mul(output.position, viewProj);

	return output;
}

#else

#include "../../camera.hlsl"

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID, uint vertexId : SV_VertexID)
{
	PixelInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
#if VtxColors
	output.color = input.color;
#endif
	output.position = mul(float4(input.position, 1), mat).xyzw;
	output.pos = output.position.xyz;
#if VtxUVs
	output.tex = input.tex;
#endif
#if VtxNormals
	output.normal = mul(input.normal, (float3x3)mat);
#endif
#if VtxTangents
	output.tangent = mul(input.tangent, (float3x3)mat);
#endif
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