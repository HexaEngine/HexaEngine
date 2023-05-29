#include "defs.hlsl"

GeometryInput main(VertexInput input)
{
    GeometryInput output;

    output.pos = input.pos;
    output.normal = input.normal;
#if VtxTangent
    output.tangent = input.tangent;
#endif
#if VtxBitangent
    output.bitangent = input.bitangent;
#endif

	return output;
}