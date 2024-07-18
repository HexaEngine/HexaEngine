#include "defs.hlsl"

struct PixelOutput
{
	float4 color : SV_TARGET;
	float depth : SV_DEPTH;
};

#ifndef ESM_EXPONENT
#define ESM_EXPONENT 50
#endif

PixelOutput main(PixelInput input)
{
	clip(input.clip);

	PixelOutput output;
#if SOFT_SHADOWS == 4
	output.color = float4(input.depth, input.depth * input.depth, 0, 0);
#endif

#if SOFT_SHADOWS == 3
	output.color = float4(exp(-ESM_EXPONENT * input.depth), 0, 0, 0);
#endif

#if SOFT_SHADOWS < 3
	output.color = float4(input.depth, 0, 0, 0);
#endif

	output.depth = input.depth;
	return output;
}