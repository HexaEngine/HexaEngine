Texture2D txDiffuse : register(t0);
SamplerState linear_wrap_sampler : register(s0);

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEX;
};

float4 main(PS_INPUT IN) : SV_TARGET
{
    return txDiffuse.Sample(linear_wrap_sampler, IN.TexCoord);
}