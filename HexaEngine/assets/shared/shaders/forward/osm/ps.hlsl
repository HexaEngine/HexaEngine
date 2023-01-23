#include "defs.hlsl"

cbuffer PointLightParams
{
	float3 Position;
	float FarPlane;
};

float main(PixelInput input) : SV_DEPTH
{
	float distance = length(input.shadowCoord.xyz - Position);
	float depth = distance / FarPlane;
    return depth;

}