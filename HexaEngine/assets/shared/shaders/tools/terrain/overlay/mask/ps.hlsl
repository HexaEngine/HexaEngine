#include "defs.hlsl"

cbuffer poit
{
    float3 location;
    float range;
};

Texture2D tex;
SamplerState smp;

float4 main(PixelInput input) : SV_TARGET
{
    float3 pos = input.pos;

    float x = pos.x - location.x;
    float z = pos.z - location.z;
    //
    float distance = sqrt(x * x + z * z);
    //
    if (distance > range)
    {
        float4 col = tex.Sample(smp, input.ctex);
        return float4(col.xyz, 1);
    }
    return float4(1, 0, 0, 1);
}