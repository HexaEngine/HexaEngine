#include "defs.hlsl"


#ifndef Tessellation
#define Tessellation 0
#endif

cbuffer cb
{
	uint offset;
}

StructuredBuffer<float4x4> instances;
StructuredBuffer<uint> offsets;

#if Tessellation
HullInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
	HullInput output;

	float4x4 mat = instances[instanceId + offsets[offset]];
	output.pos = mul(float4(input.pos, 1), mat).xyz;
	output.tex = input.tex;
	output.normal = mul(input.normal, (float3x3)mat);

#if (DEPTH != 1)
	output.tangent = mul(input.tangent, (float3x3)mat);
#endif

	output.TessFactor = 1;
	return output;
}
#else

#include "../../camera.hlsl"

PixelInput main(VertexInput input, uint instanceId : SV_InstanceID)
{
    PixelInput output;

    float4x4 mat = instances[instanceId + offsets[offset]];
    output.position = mul(float4(input.pos, 1), mat).xyzw;
    output.tex = input.tex;
    output.normal = mul(input.normal, (float3x3) mat);

#if (DEPTH != 1)
    output.tangent = mul(input.tangent, (float3x3) mat);
#endif
	
#if (DEPTH != 1)
    output.pos = output.position;
#endif
    
    output.position = mul(output.position, view);
    output.depth = output.position.z / cam_far;
    output.position = mul(output.position, proj);
    
    return output;
}
#endif