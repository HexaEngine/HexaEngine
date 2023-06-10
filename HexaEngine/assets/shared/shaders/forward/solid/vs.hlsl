#include "defs.hlsl"
#include "../../camera.hlsl"

cbuffer overlay
{
    bool ShowWeights;
    int WeightMask;
    float2 pad;
};

cbuffer worldBuffer : register(b2)
{
    float4x4 world;
}

#if VtxSkinned

float3 colorRamp(in float value)
{
	float r;
	float g;
	float b;

	if (value <= 0.0f)
	{
		r = g = b = 1.0;
	}
	else if (value <= 0.25)
	{
		r = 0.0;
		b = 1.0;
		g = value / 0.25;
	}
	else if (value <= 0.5)
	{
		r = 0.0;
		g = 1.0;
		b = 1.0 + (-1.0) * (value - 0.25) / 0.25;
	}
	else if (value <= 0.75)
	{
		r = (value - 0.5) / 0.25;
		g = 1.0;
		b = 0.0;
	}
	else
	{
		r = 1.0;
		g = 1.0 + (-1.0) * (value - 0.75) / 0.25;
		b = 0.0;
	}

	return float3(r, g, b);
}

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
    output.normal = normalize(mul(totalNormal, (float3x3) mat));
#endif

#if VtxPosition
    output.position = mul(output.position, viewProj);
#endif

    output.vertexId = vertexId;
    output.weightColor = 0;
    if (ShowWeights)
    {
        if (WeightMask > -1)
        {
            for (uint i = 0; i < MaxBoneInfluence; i++)
            {
                if (input.boneIds[i] == -1) 
                    continue;
                if (input.boneIds[i] >= MaxBones)
                {
                    break;
                }

                if (input.boneIds[i] == WeightMask)
                {
                    output.weightColor = colorRamp(input.weights[i]);
                }
            }
        }
        else
        {
            for (uint i = 0; i < MaxBoneInfluence; i++)
            {
                if (input.boneIds[i] == -1) 
                    continue;

                if (input.boneIds[i] >= MaxBones)
                {
                    break;
                }
        
                output.weightColor += colorRamp(input.weights[i]);
            }
            output.weightColor = normalize(output.weightColor);
        }
    }
    

 
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
    output.position = mul(output.position, viewProj);
    output.vertexId = vertexId;
    
    return output;
}

#endif