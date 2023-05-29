#include "defs.hlsl"

GeometryInput main(VertexInput input)
{
    GeometryInput output;

    output.pos = float4(input.pos, 1);
    output.normal = input.normal;
    output.tangent = input.tangent;

	return output;
}