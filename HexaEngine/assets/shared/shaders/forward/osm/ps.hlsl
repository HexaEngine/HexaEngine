#include "defs.hlsl"

cbuffer PointLightParams
{
	float3 Position;
	float FarPlane;
};

float4 main(PixelInput input) : SV_Target
{
	float distance = length(input.shadowCoord.xyz - Position);
	float depth = distance / FarPlane;
	return float4(depth.xxxx);
}