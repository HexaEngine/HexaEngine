Texture2D sunTex : register(t0);
SamplerState linearWrapSampler : register(s0);

struct PS_INPUT
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEX;
};

float4 main(PS_INPUT IN) : SV_TARGET
{
    return sunTex.Sample(linearWrapSampler, IN.TexCoord);
}