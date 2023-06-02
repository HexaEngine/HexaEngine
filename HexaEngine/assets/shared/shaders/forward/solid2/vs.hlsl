#include "defs.hlsl"
#include "../../camera.hlsl"

cbuffer worldBuffer : register(b2)
{
    float4x4 world;
}

#if VtxSkinned

StructuredBuffer<float4x4> boneMatrices;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID, uint vertexId : SV_VertexID)
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
    
    uint boneMatrixOffset = 0;
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
        float4 localPosition = mul(float4(input.pos, 1.0f), boneMatrices[input.boneIds[i]]);
        totalPosition += localPosition * input.weights[i];
#endif    
#ifdef VtxNormal
        float3 localNormal = mul(input.normal, (float3x3) boneMatrices[input.boneIds[i]]);
        totalNormal += localNormal * input.weights[i];
#endif
#ifdef VtxTangent
        float3 localTangent = mul(input.tangent, (float3x3) boneMatrices[input.boneIds[i]]);
        totalTangent += localTangent * input.weights[i];
#endif
#ifdef VtxBitangent
        float3 localBitangent = mul(input.bitangent, (float3x3) boneMatrices[input.boneIds[i]]);
        totalBitangent += localBitangent * input.weights[i];
#endif
    }
    
    PixelInput output;
    
    float4x4 mat = world;
		
#if VtxColor
    output.color = input.color;
#endif
    
#if VtxPosition
    output.position = mul(totalPosition, mat).xyzw;
    output.pos = output.position;
#endif 
    
#if VtxNormal
    output.normal = mul(totalNormal, (float3x3) mat);
#endif

#if VtxPosition
    output.position = mul(output.position, view);
    output.position = mul(output.position, proj);
#endif

    output.vertexId = vertexId;
 
    return output;
}

#else

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID, uint vertexId : SV_VertexID)
{
    PixelInput output;
    output.position = float4(input.pos, 1);
    
#if VtxColor
    output.color = input.color;
#endif
    
#if VtxNormal
    output.normal = normalize(mul(input.normal, (float3x3)world));
#endif
    
    output.pos = output.position;
    output.position = mul(output.position, world);
    output.position = mul(output.position, view);
    output.position = mul(output.position, proj);
    output.vertexId = vertexId;
    
    return output;
}

#endif