#include "../common.hlsl"

cbuffer OpacityBuffer : register(b1)
{
	float opacity;
};

float4 main(VSOut input) : SV_TARGET
{
	float4 color = float4(0, 0, 0, opacity);
	return ApplyMask(color, input);
}