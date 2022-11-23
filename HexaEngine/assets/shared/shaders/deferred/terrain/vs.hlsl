#include "defs.hlsl"
#include "../../camera.hlsl"
#include "../../tessellation.hlsl"

HullInput main(VertexInput input)
{
	HullInput output;
#if (INSTANCED == 1)
	float4x4 mat = float4x4(input.instance, input.instance1, input.instance2, input.instance3);
	output.pos = mul(float4(input.pos, 1), mat).xyz;
#else
	output.pos = input.pos;
#endif

	output.tex = input.tex;
	output.ctex = input.ctex;

	output.TessFactor = GetTessAndDisplFactor(GetCameraPos(), output.pos).x;
	return output;
}