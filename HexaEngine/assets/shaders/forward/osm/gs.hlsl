#include "defs.hlsl"

cbuffer LIGHT_VIEW_PROJECTION : register(b0)
{
    matrix g_lightSpace[6];
    uint offset;
    float3 padd;
};

[maxvertexcount(3 * 6)]
void main(triangle GeometryInput input[3], inout TriangleStream<PixelInput> triStream)
{
    PixelInput output = (PixelInput)0;
	
    for (int i = 0; i < 6; ++i)
    {
        for (int j = 0; j < 3; ++j)
        {
            output.shadowCoord = input[j].position;
            output.position = mul(input[j].position, g_lightSpace[i]);
            output.rtIndex = i + offset * 6;
            triStream.Append(output);
        }
        triStream.RestartStrip();
    }
}