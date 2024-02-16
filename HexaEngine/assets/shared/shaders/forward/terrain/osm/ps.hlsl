#include "defs.hlsl"

float main(PixelInput input) : SV_DEPTH
{
    clip(input.clip);

    return input.depth;
}