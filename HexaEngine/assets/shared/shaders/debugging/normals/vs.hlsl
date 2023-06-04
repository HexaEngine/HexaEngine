#include "defs.hlsl"

cbuffer worldBuffer : register(b2)
{
    float4x4 world;
}

GeometryInput main(VertexInput input)
{
    GeometryInput output;

    output.pos = mul(float4(input.pos, 1), world).xyz;
    output.normal = normalize(mul(input.normal, (float3x3)world));
#if VtxTangent
    output.tangent = normalize(mul(input.tangent, (float3x3)world));
#endif
#if VtxBitangent
    output.bitangent = normalize(mul(input.bitangent, (float3x3)world));
#endif

	return output;
}