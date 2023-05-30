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
    float3 up = float3(0, 1, 0);
    float3 look = normalize(GetCameraPos() - input[0].pos);
    float3 right = cross(up, look);

    float halfS = 0.5f;

    float4 v[4];
    v[0] = float4(input[0].pos + halfS * right - halfS * up, 1.0f) + n;
    v[1] = float4(input[0].pos + halfS * right + halfS * up, 1.0f) + n;
    v[2] = float4(input[0].pos - halfS * right - halfS * up, 1.0f) + n;
    v[3] = float4(input[0].pos - halfS * right + halfS * up, 1.0f) + n;

    
    PixelInput output = (PixelInput) 0;
    output.color = float4(0, 0, 0, 1);
    float4 pos0 = mul(mul(v[0], view), proj);
    float4 pos1 = mul(mul(v[1], view), proj);
    float4 pos2 = mul(mul(v[3], view), proj);
    float4 pos3 = mul(mul(v[2], view), proj);
    
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