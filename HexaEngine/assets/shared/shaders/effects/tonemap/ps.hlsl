Texture2D hdrTexture : register(t0);

SamplerState linearClampSampler : register(s0);

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

#define GAMMA 2.2

float ColorToLuminance(float3 color)
{
    return dot(color, float3(0.2126f, 0.7152f, 0.0722f));
}

float3 LinearTonemap(float3 color)
{
    color = clamp(color, 0., 1.);
    color = pow(color, 1. / GAMMA);
    return color;
}

float3 ReinhardTonemap(float3 color)
{
    float luma = ColorToLuminance(color);
    float toneMappedLuma = luma / (1. + luma);
    if (luma > 1e-6)
        color *= toneMappedLuma / luma;

    color = pow(color, 1. / GAMMA);
    return color;
}

float3 ACESFilmTonemap(float3 x)
{
    return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 Uncharted2Tonemap(float3 x)
{
    float A = 0.15;
    float B = 0.50;
    float C = 0.10;
    float D = 0.20;
    float E = 0.02;
    float F = 0.30;
    float W = 11.2;
    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

float3 OECF_sRGBFast(float3 color)
{
    return pow(color.rgb, float3(1.0 / GAMMA, 1.0 / GAMMA, 1.0 / GAMMA));
}

float4 main(VSOut vs) : SV_Target
{
    float4 color = hdrTexture.Sample(linearClampSampler, vs.Tex);

    color.rgb = ACESFilmTonemap(color.rgb);
    color.rgb = OECF_sRGBFast(color.rgb);
    color.a = dot(color.rgb, float3(0.299, 0.587, 0.114));

    return color;
}