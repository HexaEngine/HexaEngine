#include "defs.hlsl"

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
    
    #if VtxColor
    output.color = input.color;
    #endif
    
#if VtxPosition
    output.position = mul(float4(input.pos, 1), mat).xyzw;
    output.pos = output.position;
#endif
    
    #if VtxUV
    output.tex = input.tex;
    #endif
    
    #if VtxNormal
    output.normal = mul(input.normal, (float3x3) mat);
    #endif
    #if VtxTangent
    output.tangent = mul(input.tangent, (float3x3) mat);
    #endif
    #if VtxBitangent
    output.bitangent = mul(input.bitangent, (float3x3) mat);
    #endif

#if VtxPosition
    output.position = mul(output.position, view);
    output.depth = output.position.z / cam_far;
    output.position = mul(output.position, proj);
#endif
    
    return output;
}
#endif

/*

// transform position by indexed matrix
float4 blendPos = float4(0,0,0,0);
int i;
for (i = 0; i < 4; ++i)
{
	blendPos += float4(mul(worldMatrix3x4Array[blendIdx[i]], position).xyz, 1.0) * blendWgt[i];
}

// transform normal
float3 norm = float3(0,0,0);
for (i = 0; i < 4; ++i)
{
	norm += mul((float3x3)worldMatrix3x4Array[blendIdx[i]], normal) * 
	blendWgt[i];
}
norm = normalize(norm);
*/