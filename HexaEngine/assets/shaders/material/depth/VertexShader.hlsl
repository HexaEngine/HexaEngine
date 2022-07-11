#include "defs.hlsl"

HullInput main(VertexInput input)
{
	HullInput output;
	output.pos = input.position;
	output.tex = input.tex;
	output.normal = input.normal;
	return output;
}