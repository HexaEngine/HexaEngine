#include "defs.hlsl"

float4 main(PixelInput input) : SV_Target
{
	float depth = input.shadowCoord.z / input.shadowCoord.w;
	return float4(depth.xxxx);
}