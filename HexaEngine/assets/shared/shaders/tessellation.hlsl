#ifndef TESSELLATION_H_INCLUDED
#define TESSELLATION_H_INCLUDED

cbuffer TessellationBuffer : register(b2)
{
    float MinFactor;
    float MaxFactor;
    float MinDistance;
    float MaxDistance;
};

float2 GetTessAndDisplFactor(float3 cameraPos, float3 vertexPos)
{
    float d = distance(vertexPos, cameraPos);
    float s = saturate((d - MinDistance) / (MaxDistance - MinDistance));

    return float2(pow(2, (lerp(MaxFactor, MinFactor, s))), 0);
}

#endif