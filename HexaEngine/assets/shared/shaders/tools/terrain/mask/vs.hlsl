#include "defs.hlsl"
#include "../../../camera.hlsl"

#ifndef TILESIZE
#define TILESIZE float2(0, 0)
#endif

cbuffer WorldBuffer
{
    float4x4 world;
};

PixelInput main(VertexInput input)
{
    PixelInput output;
    
  
#if VtxPosition
    output.position = mul(float4(input.pos, 1), world).xyzw;
    output.pos = output.position.xyz;
#endif
    
#if VtxUV
    output.tex = input.tex;
    output.ctex = input.pos.xz / TILESIZE;
#endif
    
#if VtxNormal
    output.normal = mul(input.normal, (float3x3) world);
#endif

#if VtxTangent
    output.tangent = mul(input.tangent, (float3x3) world);
#endif

#if VtxBitangent
    output.bitangent = mul(input.bitangent, (float3x3) world);
#endif

#if VtxPosition
    output.position = mul(output.position, viewProj);
#endif

    return output;
}