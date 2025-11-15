#include "defs.hlsl"

cbuffer lightBuffer : register(b0)
{
	float4x4 lightView;
	float4x4 lightViewProj;
	float3 lightPosition;
	float lightFar;
	float esmExponent;
};

struct PixelOutput
{
	float4 color : SV_TARGET;
};

#ifndef ESM_EXPONENT
#define ESM_EXPONENT 50
#endif

float2 ComputeMoments(float Depth)
{
	float2 Moments;
	// First moment is the depth itself.
	Moments.x = Depth;
	// Compute partial derivatives of depth.
	float dx = ddx(Depth);
	float dy = ddy(Depth);
	// Compute second moment over the pixel extents.
	Moments.y = Depth * Depth + 0.25 * (dx * dx + dy * dy);
	return Moments;
}

PixelOutput main(PixelInput input)
{
	float depth = length(input.pos) / lightFar;

	PixelOutput output;
#if SOFT_SHADOWS == 4
	output.color = 0;
	output.color.xy = ComputeMoments(depth);
#endif

#if SOFT_SHADOWS == 3
	output.color = float4(exp(-ESM_EXPONENT * depth), 0, 0, 0);
#endif

#if SOFT_SHADOWS < 3
	output.color = float4(depth, 0, 0, 0);
#endif

	return output;
}