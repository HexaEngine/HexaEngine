#include "defs.hlsl"

PatchTess PatchHS(InputPatch<HullInput, 3> patch, uint patchId : SV_PrimitiveID)
{
    PatchTess output;

    output.EdgeTess[0] = 0.5f * (patch[1].TessFactor + patch[2].TessFactor);
    output.EdgeTess[1] = 0.5f * (patch[2].TessFactor + patch[0].TessFactor);
    output.EdgeTess[2] = 0.5f * (patch[0].TessFactor + patch[1].TessFactor);
    output.InsideTess = output.EdgeTess[0];

    return output;
}

[domain("tri")]
[partitioning("fractional_odd")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[patchconstantfunc("PatchHS")]
DomainInput main(InputPatch<HullInput, 3> patch, uint pointId : SV_OutputControlPointID, uint patchId : SV_PrimitiveID)
{
    DomainInput output;

#if VtxColors
    output.color = patch[patchId].color;
#endif

    output.position = patch[pointId].position;

#if VtxUVs
    output.tex = patch[pointId].tex;
#endif

#if VtxNormals
    output.normal = patch[pointId].normal;
#endif

#if VtxTangents
    output.tangent = patch[pointId].tangent;
#endif

    return output;
}