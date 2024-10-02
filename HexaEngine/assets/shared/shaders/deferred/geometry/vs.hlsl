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

#if Tessellation

#if VtxSkinned

StructuredBuffer<float4x4> boneMatrices;
StructuredBuffer<uint> boneMatrixOffsets;

HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	float3 totalPosition = 0;
	float3 totalNormal = 0;
	float3 totalTangent = 0;

	SkinningTransform(instanceId, boneMatrixOffsets, boneMatrices, input.position, input.normal, input.tangent, input.boneIds, input.weights, totalPosition, totalNormal, totalTangent);

	HullInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
#if VtxColors
	output.color = input.color;
#endif
	output.position = mul(float4(totalPosition, 1), mat).xyzw;
#if VtxUVs	
	output.tex = input.tex;
#endif
#if VtxNormals
	output.normal = mul(totalNormal, (float3x3)mat);
#endif
#if VtxTangents
	output.tangent = mul(totalTangent, (float3x3)mat);
#endif

	output.TessFactor = ComputeTessFactor(output.position);
	return output;
}

#else

HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	HullInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
#if VtxColors
	output.color = input.color;
#endif
	output.position = mul(float4(input.position, 1), mat).xyz;
#if VtxUVs	
	output.tex = input.tex;
#endif
#if VtxNormals
	output.normal = mul(input.normal, (float3x3)mat);
#endif
#if VtxTangents
	output.tangent = mul(input.tangent, (float3x3)mat);
#endif
	output.TessFactor = ComputeTessFactor(output.position);

	return output;
}

#endif

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
	output.pos = output.position.xyz;
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

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
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

	float3 N = output.normal;
    float3 T = normalize(output.tangent - dot(output.tangent, N) * N);
    float3 B = cross(N, T);
	output.binormal = B;
	float3x3 TBN = float3x3(T, B, N);
	output.tangentViewPos = mul(camPos, TBN);
	output.tangentPos = mul(output.pos, TBN);

#endif
	output.position = mul(output.position, viewProj);



	return output;
}
#endif