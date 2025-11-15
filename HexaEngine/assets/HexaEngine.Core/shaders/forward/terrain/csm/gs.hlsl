#include "defs.hlsl"

#ifndef MAX_CASCADED_NUM
#define MAX_CASCADED_NUM 8
#endif

cbuffer lightBuffer : register(b0)
{
    float4x4 views[MAX_CASCADED_NUM];
    uint cascadeCount;
};

[maxvertexcount(3 * MAX_CASCADED_NUM)]
void main(triangle GeometryInput input[3], inout TriangleStream<PixelInput> triStream)
{
    PixelInput output;

    [unroll(MAX_CASCADED_NUM)]
    for (uint i = 0; i < cascadeCount; i++)
    {
        [unroll(3)]
        for (uint j = 0; j < 3; j++)
        {
            output.position = mul(float4(input[j].pos, 1), views[i]);
            output.rtvIndex = i;
            output.depth = output.position.z / output.position.w;
            triStream.Append(output);
        }
        triStream.RestartStrip();
    }
}