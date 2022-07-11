#include "defs.hlsl"

PatchTess ColorPatchConstantFunction(InputPatch<HullInput, 3> inputPatch, uint patchId : SV_PrimitiveID)
{
	PatchTess output;

	// Set the tessellation factors for the three edges of the triangle.
	output.edges[0] = 8;
	output.edges[1] = 8;
	output.edges[2] = 8;

	// Set the tessellation factor for tessallating inside the triangle.
	output.inside = 8;

	return output;
}

[domain("tri")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("ColorPatchConstantFunction")]
DomainInput main(InputPatch<HullInput, 3> patch, uint pointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
	DomainInput output;

	output.pos = patch[pointId].pos;
	output.tex = patch[pointId].tex;
	output.normal = patch[pointId].normal;

	return output;
}