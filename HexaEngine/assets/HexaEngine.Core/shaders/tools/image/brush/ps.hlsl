#include "../common.hlsl"

float4 main(VSOut input) : SV_TARGET
{
	return ApplyMask(brushColor, input);
}