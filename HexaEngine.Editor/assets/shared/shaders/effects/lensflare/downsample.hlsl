cbuffer DownsampleParams : register(b0)
{
    float4 uScale;
    float4 uBias;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D inputTexture : register(t0);
SamplerState linearClampSampler : register(s0);

float4 main(VSOut pin) : SV_Target
{
    return max(0.0, inputTexture.SampleLevel(linearClampSampler, pin.Tex, 0) + uBias) * uScale;
}