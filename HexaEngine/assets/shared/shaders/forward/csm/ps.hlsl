#include "defs.hlsl"

float4 main(PixelInput input) : SV_Target
{
	float depth = input.shadowCoord.x / input.shadowCoord.y;
	return float4(depth.xxxx);
}