struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

VSOut main(uint id : SV_VertexID)
{
    int2 texcoord = int2(id & 1, id >> 1);

    VSOut output;

    output.Tex = float2(texcoord);
    output.Pos = float4(2 * (texcoord.x - 0.5f), -2 * (texcoord.y - 0.5f), 0.0, 1);

    return output;
}