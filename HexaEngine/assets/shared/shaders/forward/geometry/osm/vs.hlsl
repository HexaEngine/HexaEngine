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

    output.TessFactor = TessellationFactor;
	return output;
}

#elif VtxSkinned

StructuredBuffer<float4x4> boneMatrices;
StructuredBuffer<uint> boneMatrixOffsets;

GeometryInput main(VertexInput input, uint instanceId : SV_InstanceID)
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
    
    GeometryInput output;
    
    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
    
    output.pos = mul(totalPosition, mat).xyz;
 
    return output;
}

#else

GeometryInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    GeometryInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
    
    output.pos = mul(float4(input.pos, 1), mat).xyz;
    
    return output;
}
#endif