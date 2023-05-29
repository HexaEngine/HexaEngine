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

//--------------------------------------------------------------------------------------
// Solid color shading pixel shader (used for wireframe overlay)
//--------------------------------------------------------------------------------------
float4 main(DS_OUTPUT Input) : SV_TARGET
{
    // Return a solid green color
    return float4( 0, 1, 0, 1 );
}
