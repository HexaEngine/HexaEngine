#include "common.hlsl"
#include "../camera.hlsl"

cbuffer VoxelCbuf : register(b0)
{
    VoxelRadiance voxel_radiance;
}

struct GSInput
{
    float3 pos : POSITION;
    float4 col : COLOR;
};

struct GSOutput
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
};

// Creates a unit cube triangle strip from just vertex ID (14 vertices)
inline float3 CreateCube(in uint vertexID)
{
    uint b = 1u << vertexID;
    return float3((0x287a & b) != 0, (0x02af & b) != 0, (0x31e3 & b) != 0);
}

[maxvertexcount(14)]
void main(
	point GSInput input[1],
	inout TriangleStream<GSOutput> output
)
{
	[branch]
    if (input[0].col.a > 0)
    {
        for (uint i = 0; i < 14; i++)
        {
            GSOutput element = (GSOutput) 0;

            element.pos = float4(input[0].pos, 1.0f);
            element.col = input[0].col;

            element.pos.xyz = element.pos.xyz * voxel_radiance.DataResRCP * 2 - 1;
            element.pos.y = -element.pos.y;
            element.pos.xyz *= voxel_radiance.DataRes;
            element.pos.xyz += (CreateCube(i) - float3(0, 1, 0)) * 2;
            element.pos.xyz *= voxel_radiance.DataRes * voxel_radiance.DataSize / voxel_radiance.DataRes;

            element.pos.xyz += voxel_radiance.GridCenter;

            element.pos = mul(element.pos, viewProj);

            output.Append(element);
        }
        output.RestartStrip();
    }
}