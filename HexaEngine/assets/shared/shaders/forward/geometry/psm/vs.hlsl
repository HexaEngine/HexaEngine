#include "defs.hlsl"

cbuffer cb
{
    uint offset;
}

cbuffer LightView : register(b1)
{
    matrix view;
};

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

float4 main(VertexInput input, uint instanceId : SV_InstanceID) : SV_Position
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

    float4 output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output = mul(totalPosition, mat).xyzw;
    output = mul(output, view);

    return output;
}

#else

float4 main(VertexInput input, uint instanceId : SV_InstanceID) : SV_Position
{
    float4 output;

    float4x4 mat = worldMatrices[instanceId + worldMatrixOffsets[offset]];

    output = mul(float4(input.pos, 1), mat).xyzw;
    output = mul(output, view);

    return output;
}
#endif