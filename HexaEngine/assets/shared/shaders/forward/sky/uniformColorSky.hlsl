#include "../../weather.hlsl"

struct VertexOut
{
    float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
};

float4 main(VertexOut pin) : SV_Target
{
    return sky_color;
}