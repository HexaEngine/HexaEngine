#include "defs.hlsl"
#include "HexaEngine.Core:shaders/camera.hlsl"

cbuffer WorldBuffer
{
    float4x4 world;
};

VertexOut main(VertexIn input)
{

    float4 pos = mul(float4(input.position, 1.0f), world);
    pos = mul(pos, viewProj);
    
    VertexOut output;
    output.position = pos;
    return output;
}