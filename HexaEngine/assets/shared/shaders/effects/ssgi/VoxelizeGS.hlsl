#include "common.hlsl"

cbuffer VoxelCbuf : register(b0)
{
    VoxelRadiance voxel_radiance;
}

struct GS_INPUT
{
    float4 PositionWS   : POSITION;
    float2 Uvs          : TEX;
    float3 NormalWS     : NORMAL0;
};


struct GS_OUTPUT
{
    float4 Position     : SV_POSITION;
    float4 PositionWS   : POSITION;
    float2 Uvs          : TEX;
    float3 NormalWS     : NORMAL0;
};


[maxvertexcount(3)]
void main(
	triangle GS_INPUT input[3] : SV_POSITION,
	inout TriangleStream<GS_OUTPUT> outputStream
)
{
    float3 facenormal = abs(input[0].NormalWS + input[1].NormalWS + input[2].NormalWS);
    uint maxi = facenormal[1] > facenormal[0] ? 1 : 0;
    maxi = facenormal[2] > facenormal[maxi] ? 2 : maxi;

    for (uint i = 0; i < 3; ++i)
    {
        GS_OUTPUT output;

		// World space -> Voxel grid space:
        output.Position.xyz = (input[i].PositionWS.xyz - voxel_radiance.GridCenter) * voxel_radiance.DataSizeRCP;

		// Project onto dominant axis:
		[flatten]
        if (maxi == 0)
        {
            output.Position.xyz = output.Position.zyx;
        }
        else if (maxi == 1)
        {
            output.Position.xyz = output.Position.xzy;
        }

		// Voxel grid space -> Clip space
        output.Position.xy *= voxel_radiance.DataResRCP;
        output.Position.zw = float2(0, 1);

		
        output.Uvs = input[i].Uvs;
        output.NormalWS = input[i].NormalWS;
        output.PositionWS = input[i].PositionWS;

        outputStream.Append(output);
        
    }
}