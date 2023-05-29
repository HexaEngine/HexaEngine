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

#if VtxTangent
output.tangent = mul(input.tangent, (float3x3) mat);
#endif

#if VtxBitangent
    output.bitangent = mul(input.bitangent, (float3x3) mat);
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
#ifdef VtxPosition
    float4 totalPosition = 0;
#endif
#ifdef VtxNormal
    float3 totalNormal = 0;
#endif
#ifdef VtxTangent
    float3 totalTangent = 0;
#endif
#ifdef VtxBitangent
    float3 totalBitangent = 0;
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
#ifdef VtxNormal
            totalNormal = input.normal;
#endif
#ifdef VtxTangent
            totalTangent = input.tangent;
#endif
#ifdef VtxBitangent
            totalBitangent = input.bitangent;
#endif
            break;
        }
        
#ifdef VtxPosition    
        float4 localPosition = mul(float4(input.pos, 1.0f), boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalPosition += localPosition * input.weights[i];
#endif    
#ifdef VtxNormal
        float3 localNormal = mul(input.normal, (float3x3) boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalNormal += localNormal * input.weights[i];
#endif
#ifdef VtxTangent
        float3 localTangent = mul(input.tangent, (float3x3) boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalTangent += localTangent * input.weights[i];
#endif
#ifdef VtxBitangent
        float3 localBitangent = mul(input.bitangent, (float3x3) boneMatrices[input.boneIds[i] + boneMatrixOffset]);
        totalBitangent += localBitangent * input.weights[i];
#endif
    }
    
    PixelInput output;
    
    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
		
#if VtxColor
    output.color = input.color;
#endif
    
#if VtxPosition
    output.position = mul(totalPosition, mat).xyzw;
    output.pos = output.position;
#endif
    
#if VtxUV
    output.tex = input.tex;
#endif
    
#if VtxNormal
    output.normal = mul(totalNormal, (float3x3) mat);
#endif

#if VtxTangent
    output.tangent = mul(totalTangent, (float3x3) mat);
#endif

#if VtxBitangent
    output.bitangent = mul(totalBitangent, (float3x3) mat);
#endif

#if VtxPosition
    output.position = mul(output.position, view);
    output.depth = output.position.z / cam_far;
    output.position = mul(output.position, proj);
#endif
 
    return output;
}

#else

#include "../../camera.hlsl"

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];
    
#if VtxColor
    output.color = input.color;
#endif
    
#if VtxPosition
    output.position = mul(float4(input.pos, 1), mat).xyzw;
    output.pos = output.position.xyz;
#endif
    
#if VtxUV
    output.tex = input.tex;
#endif
    
#if VtxNormal
    output.normal = mul(input.normal, (float3x3) mat);
#endif

#if VtxTangent
output.tangent = mul(input.tangent, (float3x3) mat);
#endif

#if VtxBitangent
    output.bitangent = mul(input.bitangent, (float3x3) mat);
#endif

#if VtxPosition
    output.position = mul(output.position, view);
    output.depth = output.position.z / cam_far;
    output.position = mul(output.position, proj);
#endif
    
    return output;
}
#endif



