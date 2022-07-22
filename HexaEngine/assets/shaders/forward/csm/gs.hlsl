#include "defs.hlsl"
#define MAX_CASCADED_NUM 3

cbuffer LIGHT_VIEW_PROJECTION : register(b0)
{
    matrix g_lightSpace[MAX_CASCADED_NUM];
};

[maxvertexcount(3 * MAX_CASCADED_NUM)]
void main(triangle GeometryInput input[3], inout TriangleStream<PixelInput> triStream)
{
    PixelInput output = (PixelInput)0;
	
    for (int i = 0; i < MAX_CASCADED_NUM; ++i)
    {
        for (int j = 0; j < 3; ++j)
        {
            output.position = mul(input[j].position, g_lightSpace[i]);
            output.shadowCoord = output.position.zw;
            output.rtIndex = i;
            triStream.Append(output);
        }
        triStream.RestartStrip();
    }
}