Texture2D hdrTexture : register(t0);
Texture2D<float> temporalNoise : register(t1);
SamplerState linearClampSampler : register(s0);

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer Params
{
    float3 FilmGrainColor;
    float Time;
    float FilmGrainIntensity;
    float3 padd0;
};

float4 main(VSOut vs) : SV_Target
{
    float3 color = hdrTexture.SampleLevel(linearClampSampler, vs.Tex, 0).rgb;
    float grain = temporalNoise.SampleLevel(linearClampSampler, vs.Tex, 0) * 2.0 - 1.0;
    color.rgb -= grain * FilmGrainIntensity * FilmGrainColor;
    return float4(color, 1);
}