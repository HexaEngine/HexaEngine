#include "defs.hlsl"

cbuffer PointLightParams
{
	float3 Position;
	float FarPlane;
};

float main(PixelInput input) : SV_DEPTH
{
    return input.depth;
}