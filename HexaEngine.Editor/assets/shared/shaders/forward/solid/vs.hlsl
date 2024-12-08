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

    float4 totalPosition = 0;

    float3 totalNormal = 0;

    float3 totalTangent = 0;

    float3 totalBitangent = 0;

    uint boneMatrixOffset = 0;
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

        float4 localPosition = mul(float4(input.pos, 1.0f), boneMatrices[input.boneIds[i]]);
        totalPosition += localPosition * input.weights[i];

        float3 localNormal = mul(input.normal, (float3x3) boneMatrices[input.boneIds[i]]);
        totalNormal += localNormal * input.weights[i];

        float3 localTangent = mul(input.tangent, (float3x3) boneMatrices[input.boneIds[i]]);
        totalTangent += localTangent * input.weights[i];

        float3 localBitangent = mul(input.bitangent, (float3x3) boneMatrices[input.boneIds[i]]);
        totalBitangent += localBitangent * input.weights[i];

    }

    PixelInput output;

    float4x4 mat = world;

    output.position = mul(totalPosition, mat).xyzw;
    output.pos = output.position;

    output.normal = normalize(mul(totalNormal, (float3x3) mat));

    output.position = mul(output.position, viewProj);

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

    output.normal = normalize(mul(input.normal, (float3x3) world));

    output.pos = output.position;
    output.position = mul(output.position, world);
    output.position = mul(output.position, viewProj);
    output.vertexId = vertexId;

    return output;
}

#endif