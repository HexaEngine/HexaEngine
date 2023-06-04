#include "defs.hlsl"

#ifndef MAX_CASCADED_NUM
#define MAX_CASCADED_NUM 3
#endif

cbuffer LIGHT_VIEW_PROJECTION : register(b1)
{
	float4x4 g_lightSpace[MAX_CASCADED_NUM];
};

[maxvertexcount(3 * MAX_CASCADED_NUM)]
void main(triangle GeometryInput input[3], inout TriangleStream<PixelInput> triStream)
{
	PixelInput output = (PixelInput)0;

	for (int i = 0; i < MAX_CASCADED_NUM; ++i)
	{
		for (int j = 0; j < 3; ++j)
		{
            output.position = mul(float4(input[j].pos, 1), g_lightSpace[i]);
			output.rtvIndex = i;
			triStream.Append(output);
		}
		triStream.RestartStrip();
	}
}