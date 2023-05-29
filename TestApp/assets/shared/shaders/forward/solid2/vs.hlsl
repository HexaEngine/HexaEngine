#include "defs.hlsl"
#include "../../camera.hlsl"

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID, uint vertexId : SV_VertexID)
{
    PixelInput output;
    output.position = float4(input.pos, 1);
    
#if VtxColor
    output.color = input.color;
#endif
    
#if VtxNormal
    output.normal = input.normal;
#endif
    
    output.pos = output.position;
    output.position = mul(output.position, view);
    output.position = mul(output.position, proj);
    output.vertexId = vertexId;
    
    return output;
}