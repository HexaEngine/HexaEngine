#include "defs.hlsl"
#include "../../camera.hlsl"
#include "../../tessellation.hlsl"

HullInput main(VertexInput input)
{
	HullInput output;
	output.pos = input.pos;
	output.tex = input.tex;
	output.ctex = input.ctex;

	output.TessFactor = GetTessAndDisplFactor(GetCameraPos(), output.pos).x;
	return output;
}