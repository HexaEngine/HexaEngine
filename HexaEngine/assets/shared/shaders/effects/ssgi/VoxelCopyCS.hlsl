#include "common.hlsl"

cbuffer VoxelCbuf : register(b0)
{
    VoxelRadiance voxel_radiance;
}

RWStructuredBuffer<VoxelType> VoxelGrid : register(u0);

RWTexture3D<float4> VoxelTexture : register(u1);


[numthreads(256, 1, 1)]
void main( uint3 DTid : SV_DispatchThreadID )
{
    VoxelType voxel = VoxelGrid[DTid.x];
    
    float4 color = DecodeColor(voxel.ColorMask);
    
    uint3 writecoord = Unflatten3D(DTid.x, voxel_radiance.DataRes); //add voxel cbuffer to CS
    
    [branch]
    if (color.a > 0)
    {
        VoxelTexture[writecoord] = float4(color.rgb, 1);
    }
    else
    {
        VoxelTexture[writecoord] = 0;
    }
    
    VoxelGrid[DTid.x].ColorMask = 0;
    
}