#include "defs.hlsl"

struct PixelOutput
{
	float4 color : SV_TARGET;
	float depth : SV_DEPTH;
};

#ifndef ESM_EXPONENT
#define ESM_EXPONENT 50
#endif

cbuffer lightBuffer
{
	float4x4 views[4];
	float lightNear;
	float lightFar;
	float2 padd;
	float3 lightPosition;
};

PixelOutput main(PixelInput input)
{
	float depth = input.depth;

	PixelOutput output;
#if SOFT_SHADOWS == 4
	output.color = float4(depth, depth * depth, 0, 0);
#endif

#if SOFT_SHADOWS == 3
	output.color = float4(exp(-ESM_EXPONENT * depth), 0, 0, 0);
#endif

#if SOFT_SHADOWS < 3
	output.color = float4(depth, 0, 0, 0);
#endif

	output.depth = depth;
	return output;
}