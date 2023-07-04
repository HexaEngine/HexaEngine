#include "defs.hlsl"

cbuffer cb
{
    uint offset;
}

cbuffer LightBuffer : register(b1)
{
    matrix view;
    float3 position;
    float far;
};

StructuredBuffer<float4x4> worldMatrices;
StructuredBuffer<uint> worldMatrixOffsets;

#if Tessellation
HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	HullInput output;

	float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output.position = mul(totalPosition, mat);
    output.depth = length(output.position.xyz - position) / far;
    output.position = mul(output.position, view);

    output.TessFactor = TessellationFactor;
	return output;
}

#elif VtxSkinned

StructuredBuffer<float4x4> boneMatrices;
StructuredBuffer<uint> boneMatrixOffsets;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
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

    PixelInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output.position = mul(totalPosition, mat);
    output.depth = length(output.position.xyz - position) / far;
    output.position = mul(output.position, view);

    return output;
}

#else

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output.position = mul(float4(input.pos, 1), mat);
    output.depth = length(output.position.xyz - position) / far;
    output.position = mul(output.position, view);

    return output;
}
#endif