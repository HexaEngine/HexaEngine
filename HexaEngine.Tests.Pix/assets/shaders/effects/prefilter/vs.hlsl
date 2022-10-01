cbuffer mvp
{
    matrix view;
    matrix projection;
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
    output.Pos = mul(float4(pos, 1), view);
    output.Pos = mul(output.Pos, projection);
    return output;
}

