#include "../common.hlsl"

float4 main(VSOut input) : SV_TARGET
{
	float2 distance = input.Tex - float2(0.5, 0.5);

	if (dot(distance, distance) <= 0.25)
	{
		return brushColor;
	}
	else
	{
		discard;
	}
}