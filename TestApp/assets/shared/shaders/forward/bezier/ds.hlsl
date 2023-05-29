#include "defs.hlsl"

//--------------------------------------------------------------------------------------
// Constant Buffers
//--------------------------------------------------------------------------------------
cbuffer cbPerFrame : register(b0)
{
    matrix g_mViewProjection;
    float3 g_vCameraPosWorld;
    float g_fTessellationFactor;
};

// Domain Shader is invoked for each vertex created by the Tessellator

[domain("tri")]
DS_OUTPUT main(HS_CONSTANT_DATA_OUTPUT input,  float3 UVW : SV_DomainLocation, const OutputPatch<HS_OUTPUT, OUTPUT_PATCH_SIZE> quad)
{
    DS_OUTPUT Output;

	//baricentric interpolation
    float3 finalPos = UVW.x * quad[0].vPosition + UVW.y * quad[1].vPosition + UVW.z * quad[2].vPosition;
    
    Output.vPosition = mul(float4(finalPos, 1), g_mViewProjection);

    return Output;
}