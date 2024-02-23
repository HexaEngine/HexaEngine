#include "defs.hlsl"

cbuffer CBBrush
{
    float3 location;
    float radius;
    float edgeFadeStart;
    float edgeFadeEnd;
};

float4 main(PixelInput input) : SV_TARGET
{
    float3 pos = input.pos;

    float x = pos.x - location.x;
    float z = pos.z - location.z;

    float distance = sqrt(x * x + z * z);

    // outside of circle
    if (distance > radius)
        discard;

    float innerEdge = radius * 0.95;
    float edgeMidpoint = radius * 0.975;

    // inside of circle
    if (distance < innerEdge)
    {
        float edgeFade = saturate((edgeFadeEnd - distance) / (edgeFadeEnd - edgeFadeStart));
        return float4(1, 1, 1, 1) * 0.2 * edgeFade;
    }

    float distanceToMid = 1 - abs(radius - (distance - 0.25));

    float alpha = lerp(1, 0, distanceToMid);

    return float4(1, 1, 1, 1) * distanceToMid;
}