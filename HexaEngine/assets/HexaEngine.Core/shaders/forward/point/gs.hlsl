#include "defs.hlsl"
#include "../../camera.hlsl"

cbuffer options : register(b0)
{
    bool Normal;
    bool Tangent;
    bool Bitangent;
    float size;
};

[maxvertexcount(2 * 3)]
void main(point GeometryInput input[1], inout TriangleStream<PixelInput> triStream)
{
    float4 n = float4(input[0].normal * 0.00005f, 0);
    float3 up = input[0].bitangent;
    float3 look = input[0].normal;
    float3 right = cross(up, look);

    float halfS = 0.5f * size;

    float4 v[4];
    v[0] = float4(input[0].pos + halfS * right - halfS * up, 1.0f) + n;
    v[1] = float4(input[0].pos + halfS * right + halfS * up, 1.0f) + n;
    v[2] = float4(input[0].pos - halfS * right - halfS * up, 1.0f) + n;
    v[3] = float4(input[0].pos - halfS * right + halfS * up, 1.0f) + n;

    
    PixelInput output = (PixelInput) 0;
    output.color = float4(0, 0, 0, 1);
    float4 pos0 = mul(v[0], viewProj);
    float4 pos1 = mul(v[1], viewProj);
    float4 pos2 = mul(v[3], viewProj);
    float4 pos3 = mul(v[2], viewProj);
    
    output.position = pos0;
    triStream.Append(output);
    output.position = pos1;
    triStream.Append(output);
    output.position = pos2;
    triStream.Append(output);
    triStream.RestartStrip();
    output.position = pos0;
    triStream.Append(output);
    output.position = pos2;
    triStream.Append(output);
    output.position = pos3;
    triStream.Append(output);
    triStream.RestartStrip();
}