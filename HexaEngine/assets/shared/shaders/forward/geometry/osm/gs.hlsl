#include "defs.hlsl"

cbuffer LIGHT_VIEW_PROJECTION : register(b1)
{
    matrix g_lightSpace[6];
    float3 Position;
    float FarPlane;
};

[maxvertexcount(3 * 6)]
void main(triangle GeometryInput input[3], inout TriangleStream<PixelInput> triStream)
{
    PixelInput output = (PixelInput) 0;

    for (int i = 0; i < 6; ++i)
    {
        for (int j = 0; j < 3; ++j)
        {
            output.depth = length(input[j].pos.xyz - Position) / FarPlane;
            output.position = mul(float4(input[j].pos, 1), g_lightSpace[i]);
            output.rtvIndex = i;
			
            triStream.Append(output);
        }
        triStream.RestartStrip();
    }
}