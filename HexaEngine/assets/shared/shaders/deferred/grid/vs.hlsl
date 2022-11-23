#include "defs.hlsl"
#include "../../camera.hlsl"

PixelInput main(VertexInput input)
{
	PixelInput output;
#if (INSTANCED == 1)
	float4x4 mat = float4x4(input.instance, input.instance1, input.instance2, input.instance3);
	output.pos = mul(float4(input.pos, 1), mat);
#else
	output.pos = float4(input.pos, 1);
#endif

	output.pos = mul(output.pos, view);
	output.pos = mul(output.pos, proj);
	output.color = input.color;
	return output;
}