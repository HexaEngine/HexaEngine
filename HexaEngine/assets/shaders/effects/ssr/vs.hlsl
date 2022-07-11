struct VSIn
{
    float3 Pos : POSITION;
    float2 Tex : TEXCOORD;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

VSOut main(VSIn input)
{ 
    VSOut output;
    output.Pos = float4(input.Pos, 1);
    output.Tex = input.Tex;
    return output;
}

