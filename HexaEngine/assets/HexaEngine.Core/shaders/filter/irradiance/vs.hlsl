cbuffer mvp
{
    matrix viewProj;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float3 WorldPos : TEXCOORD;
};

VSOut main(float3 pos : POSITION)
{
    VSOut output;
    output.WorldPos = pos;
    output.Pos = mul(float4(pos, 1), viewProj);
    return output;
}