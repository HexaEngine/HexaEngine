#include "defs.hlsl"

HullInput main(VertexInput input)
{
	HullInput output;
	output.pos = input.position;
    output.tex = input.tex;
	output.normal = input.normal;
#if (DEPTH != 1)
	output.tangent = input.tangent;
#endif
    output.TessFactor = 1;
	return output;
}