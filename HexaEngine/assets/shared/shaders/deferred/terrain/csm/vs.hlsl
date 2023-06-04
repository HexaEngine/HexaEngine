#include "defs.hlsl"

cbuffer WorldBuffer
{
    float4x4 world;
};

GeometryInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    GeometryInput output;
    
#if VtxPosition
    output.pos = mul(float4(input.pos, 1), world).xyz;
#endif
    
    return output;
}