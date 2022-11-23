#include "defs.hlsl"

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

#if (INSTANCED == 1)
	output.normal = mul(input.normal, (float3x3)mat);
#else
	output.normal = input.normal;
#endif

	output.TessFactor = 1;
	return output;
}