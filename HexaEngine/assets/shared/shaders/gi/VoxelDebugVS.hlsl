#include "common.hlsl"

cbuffer VoxelCbuf : register(b0)
{
    VoxelRadiance voxel_radiance;
}

struct VSOut
{
    float3 pos : POSITION;
    float4 col : COLOR;
};

Texture3D<float4> voxel_texture : register(t9);


VSOut main(uint vertexID : SV_VERTEXID)
{
    VSOut o;

    uint3 coord = Unflatten3D(vertexID, voxel_radiance.DataRes);
    float4 voxel = voxel_texture[coord];
    
    o.pos = coord;
    o.col = voxel;
    
    return o;
}