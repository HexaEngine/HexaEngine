#include "defs.hlsl"
#include "../../camera.hlsl"

cbuffer cb
{
	uint offset;
}

StructuredBuffer<float4x4> instances;
StructuredBuffer<uint> offsets;

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    float4x4 mat = instances[instanceId + offsets[offset]];
    output.position = mul(float4(input.pos, 1), mat).xyzw;
    output.normal = mul(input.normal, (float3x3) mat);
    output.pos = output.position;
    output.position = mul(output.position, view);
    output.position = mul(output.position, proj);
    
    return output;
}