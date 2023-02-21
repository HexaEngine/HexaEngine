#include "defs.hlsl"

float main(PixelInput input) : SV_DEPTH
{
    float depth = input.shadowCoord.z / input.shadowCoord.w;
    return depth;
}