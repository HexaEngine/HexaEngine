#include "defs.hlsl"

float4 main(PixelInput input) : SV_Target
{
	float depth = input.pos.z / 100;
	return float4(depth, depth, depth, 1.0f);
}