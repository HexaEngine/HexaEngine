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
    
#if VtxColor
    output.color = input.color;
#endif
    
#if VtxPosition
    output.pos = mul(float4(input.pos, 1), mat).xyz;
#endif
    
#if VtxUV
    output.tex = input.tex;
#endif
    
#if VtxNormal
    output.normal = mul(input.normal, (float3x3) mat);
#endif

    output.TessFactor = TessellationFactor;
	return output;
}

#elif VtxSkinned

StructuredBuffer<float4x4> boneMatrices;
StructuredBuffer<uint> boneMatrixOffsets;

GeometryInput main(VertexInput input, uint instanceId : SV_InstanceID)
{   
#ifdef VtxPosition
    float4 totalPosition = 0;
#endif
    
    uint boneMatrixOffset = boneMatrixOffsets[instanceId + offset];
    for (int i = 0; i < MaxBoneInfluence; i++)
    {
        if (input.boneIds[i] == -1) 
            continue;
        if (input.boneIds[i] >= MaxBones)
        {
#ifdef VtxPosition
            totalPosition = float4(input.pos, 1.0f);
#endif
            break;
        }
        
#ifdef VtxPosition    
        float4 localPosition = mul(float4(input.pos, 1.0f), boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalPosition += localPosition * input.weights[i];
#endif    
    }
    
    GeometryInput output;
    
    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
    
#if VtxPosition
    output.pos = mul(totalPosition, mat).xyz;
#endif
 
    return output;
}

#else

GeometryInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    GeometryInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
    
#if VtxPosition
    output.pos = mul(float4(input.pos, 1), mat).xyz;
#endif
    
    return output;
}
#endif