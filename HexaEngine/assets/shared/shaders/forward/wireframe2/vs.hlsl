#include "defs.hlsl"
#include "../../camera.hlsl"

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID, uint vertexId : SV_VertexID)
{
    PixelInput output;
    output.position = float4(input.pos, 1);
#if VtxNormal
    output.position += float4(input.normal * 0.00005f, 0);
#endif
    output.position = mul(output.position, view);
    output.position = mul(output.position, proj);
    output.vertexId = vertexId;
    
    return output;
}