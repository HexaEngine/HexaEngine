#include "defs.hlsl"

cbuffer cb
{
	uint offset;
}

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

#if Tessellation
HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	HullInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
    output.pos = mul(float4(input.pos, 1), mat).xyz;
    output.tex = input.tex;
    output.normal = mul(input.normal, (float3x3) mat);
    output.tangent = mul(input.tangent, (float3x3) mat);
    output.bitangent = cross(output.normal, output.tangent);

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
    float3 totalBitangent = 0;
    
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
            totalBitangent = input.bitangent;
            break;
        }
          
        float4 localPosition = mul(float4(input.pos, 1.0f), boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalPosition += localPosition * input.weights[i];
        float3 localNormal = mul(input.normal, (float3x3) boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalNormal += localNormal * input.weights[i];
        float3 localTangent = mul(input.tangent, (float3x3) boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalTangent += localTangent * input.weights[i];
        float3 localBitangent = mul(input.bitangent, (float3x3) boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalBitangent += localBitangent * input.weights[i];
    }
    
    PixelInput output;
    
    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
		
    output.position = mul(totalPosition, mat).xyzw;
    output.pos = output.position;
    output.tex = input.tex;
    output.normal = mul(totalNormal, (float3x3) mat);
    output.tangent = mul(totalTangent, (float3x3) mat);
    output.bitangent = mul(totalBitangent, (float3x3) mat);
    output.position = mul(output.position, viewProj);
 
    return output;
}

#else

#include "../../camera.hlsl"

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
    
    output.position = mul(float4(input.position, 1), mat).xyzw;
    output.tex = input.tex;
    output.normal = mul(input.normal, (float3x3) mat);
    output.tangent = mul(input.tangent, (float3x3) mat);
    output.bitangent = mul(input.bitangent, (float3x3) mat);
    output.position = mul(output.position, viewProj);

    return output;
}
#endif



