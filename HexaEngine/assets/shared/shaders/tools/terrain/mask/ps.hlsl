#include "defs.hlsl"

Texture2D mask;
SamplerState state;

float4 main(PixelInput input) : SV_TARGET
{
    return mask.Sample(state, input.tex.xy);
}