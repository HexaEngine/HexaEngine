////////////////////////////////////////////////////////////////////////////////
// Filename: deferred.ps
////////////////////////////////////////////////////////////////////////////////

//////////////
// TYPEDEFS //
//////////////
struct HullInputType
{
	float3 pos : POSITION;
	float3 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PatchTess
{
	float edges[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

struct HullOutputType
{
	float3 pos : POSITION;
	float3 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

////////////////////////////////////////////////////////////////////////////////
// Patch Constant Function
////////////////////////////////////////////////////////////////////////////////
PatchTess ColorPatchConstantFunction(InputPatch<HullInputType, 3> inputPatch, uint patchId : SV_PrimitiveID)
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

////////////////////////////////////////////////////////////////////////////////
// Hull Shader
////////////////////////////////////////////////////////////////////////////////
[domain("tri")]
[partitioning("integer")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("ColorPatchConstantFunction")]
HullOutputType main(InputPatch<HullInputType, 3> patch, uint pointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
	HullOutputType output;

	output.pos = patch[pointId].pos;
	output.tex = patch[pointId].tex;
	output.normal = patch[pointId].normal;

	return output;
}