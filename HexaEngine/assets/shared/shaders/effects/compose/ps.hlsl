#include "../../camera.hlsl"

Texture2D hdrTexture : register(t0);
Texture2D bloomTexture : register(t1);
Texture2D lensDirt : register(t2);
Texture2D<float> depthTexture : register(t3);
Texture2D<float> lumaTexture : register(t4);
Texture2D lutTexture : register(t5);

SamplerState linearClampSampler : register(s0);

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer Params
{
    float BloomStrength;
    float FogEnabled;
    float FogStart;
    float FogEnd;
    float3 FogColor;
    float LUTAmountChroma;
    float LUTAmountLuma;
};

#define GAMMA 2.2

#ifndef LUT_TileSizeXY
#define LUT_TileSizeXY 32
#endif
#ifndef LUT_TileAmount
#define LUT_TileAmount 32
#endif

float GetExposure()
{
    float avgLum = lumaTexture.Load(0);
    float keyValue = 1.03 - (2.0 / (2.0 + log2(avgLum + 1.0)));
    float exposure = keyValue / avgLum;
    return exposure;
}

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

float3 BloomMix(float2 texCoord, float3 hdr)
{
    float3 blm = bloomTexture.Sample(linearClampSampler, texCoord).rgb;
    float3 drt = float3(0, 0, 0);
    return lerp(hdr, blm + blm * drt, float3(BloomStrength, BloomStrength, BloomStrength));
}

float ComputeFogFactor(float d)
{
    //d is the distance to the geometry sampling from the camera
    //this simply returns a value that interpolates from 0 to 1
    //with 0 starting at FogStart and 1 at FogEnd
    return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogEnabled;
}

float3 FogMix(float2 texCoord, float3 color)
{
    float depth = depthTexture.Sample(linearClampSampler, texCoord);
    float3 position = GetPositionWS(texCoord, depth);
    float d = distance(position, GetCameraPos());
    float factor = ComputeFogFactor(d);
    return lerp(color, FogColor, factor);
}

float3 LUT(float3 color)
{
    float2 texelsize = 1.0 / LUT_TileSizeXY;
    texelsize.x /= LUT_TileAmount;

    float3 lutcoord = float3((color.xy * LUT_TileSizeXY - color.xy + 0.5) * texelsize.xy, color.z * LUT_TileSizeXY - color.z);
    float lerpfact = frac(lutcoord.z);
    lutcoord.x += (lutcoord.z - lerpfact) * texelsize.y;

    float3 lutcolor = lerp(lutTexture.SampleLevel(linearClampSampler, float2(lutcoord.x, lutcoord.y), 0).xyz, lutTexture.SampleLevel(linearClampSampler, float2(lutcoord.x + texelsize.y, lutcoord.y), 0).xyz, lerpfact);

    return lerp(normalize(color.xyz), normalize(lutcolor.xyz), LUTAmountChroma) * lerp(length(color.xyz), length(lutcolor.xyz), LUTAmountLuma);
}

float4 main(VSOut vs) : SV_Target
{
    float exposure = GetExposure();
    float4 color = hdrTexture.Sample(linearClampSampler, vs.Tex);

    color.rgb = BloomMix(vs.Tex, color.rgb);
    color.rgb = FogMix(vs.Tex, color.rgb);
    color.rgb = ACESFilmTonemap(color.rgb * exposure);
    color.rgb = OECF_sRGBFast(color.rgb);
    color.rgb = LUT(color.rgb);
#if FXAA == 1
    color.a = dot(color.rgb, float3(0.299, 0.587, 0.114));
#endif

    return color;
}