#include "../../colorUtils.hlsl"
Texture2D hdrTexture : register(t0);
Texture2D lutTexture : register(t1);
SamplerState linearClampSampler : register(s0);

#ifndef TONEMAP
/*
 * 0: None
 * 1: Neutral
 * 2: AECS Film
 * fallback: None
*/
#define TONEMAP 1
#endif

#ifndef GAMMA
#define GAMMA 2.2
#endif

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer TonemapParams
{
    float ShoulderStrength;
    float LinearStrength;
    float LinearAngle;
    float ToeStrength;

    float WhiteLevel; // Pre - curve white point adjustment.
    float WhiteClip; // Post - curve white point adjustment.
    float PostExposure; // Adjusts overall exposure in EV units.
    float HueShift; // Shift the hue of all colors.

    float Saturation; // Adjusts saturation (color intensity).
    float Contrast; // Adjusts the contrast.
    float ContrastMidpoint;
    float _padd0;

    float3 WhiteBalance;
    float _padd1;

    float3 ChannelMaskRed;
    float _padd2;
    float3 ChannelMaskGreen;
    float _padd3;
    float3 ChannelMaskBlue;
    float _padd4;

    float Lift;
    float GammaInv;
    float Gain;
};

float3 NeutralCurve(float3 x)
{
    const float a = ShoulderStrength;
    const float b = LinearStrength;
    const float c = LinearAngle;
    const float d = ToeStrength;

    // Not exposed as settings
    const float e = 0.02f;
    const float f = 0.3f;

    return ((x * (a * x + c * b) + d * e) / (x * (a * x + b) + d * f)) - e / f;
}

float3 NeutralTonemap(float3 x)
{
    x = max((0.0).xxx, x);

    float3 whiteScale = (1.0).xxx / NeutralCurve(WhiteLevel);
    x = NeutralCurve(x * whiteScale);
    x *= whiteScale;

    // Post-curve white point adjustment
    x /= WhiteClip.xxx;

    return x;
}

float3 ACESFilmTonemap(float3 x)
{
    return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 Tonemap(float3 x)
{
#if TONEMAP == 0
    return x;
#elif TONEMAP == 1
    return NeutralTonemap(x);
#elif TONEMAP == 2
    return ACESFilmTonemap(x);
#else
    return x;
#endif
}

float3 AdjustWhiteBalance(float3 color)
{
    float3 lms = LinearToLMS(color);
    lms *= WhiteBalance;
    return LMSToLinear(lms);
}

float3 AdjustHue(float3 color, float hueShift)
{
    float3 hsl = RgbToHsv(color);
    hsl.x += RotateHue(hueShift, 0, 1);
    return HsvToRgb(hsl);
}

float3 AdjustSaturation(float3 c, float sat)
{
    float luma = Luminance(c);
    return luma.xxx + sat.xxx * (c - luma.xxx);
}

float3 AdjustContrast(float3 c, float midpoint, float contrast)
{
    return (c - midpoint) * contrast + midpoint;
}

float3 LiftGammaGainHDR(float3 c, float3 lift, float3 invgamma, float3 gain)
{
    c = c * gain + lift;

    // ACEScg will output negative values, as clamping to 0 will lose precious information we'll
    // mirror the gamma function instead
    return sign(c) * pow(abs(c), invgamma);
}

float3 LiftGammaGainLDR(float3 c, float3 lift, float3 invgamma, float3 gain)
{
    c = saturate(pow(saturate(c), invgamma));
    return gain * c + lift * (1.0 - c);
}

float3 ChannelMix(float3 color)
{
    return float3(
        dot(color, ChannelMaskRed),
        dot(color, ChannelMaskGreen),
        dot(color, ChannelMaskBlue)
    );
}

float4 main(VSOut vs) : SV_Target
{
    float4 color = hdrTexture.Sample(linearClampSampler, vs.Tex);

    if (color.a == 0)
        discard;

    color.rgb = color.rgb * PostExposure;
    color.rgb = AdjustWhiteBalance(color.rgb);
    color.rgb = AdjustHue(color.rgb, HueShift);
    color.rgb = AdjustSaturation(color.rgb, Saturation);
    color.rgb = AdjustContrast(color.rgb, ContrastMidpoint, Contrast);
    color.rgb = ChannelMix(color.rgb);
    color.rgb = Tonemap(color.rgb);
    color.rgb = LiftGammaGainLDR(color.rgb, Lift, GammaInv, Gain);

    color.a = 1;

    return color;
}