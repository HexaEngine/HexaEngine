struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D tex : register(t0);

SamplerState samplerState
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 main(VSOut input) : SV_TARGET
{
    return tex.Sample(samplerState, input.Tex.xy);
}
