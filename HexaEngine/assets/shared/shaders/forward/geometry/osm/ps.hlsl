#include "defs.hlsl"

float main(PixelInput input) : SV_DEPTH
{
    return input.depth;
}