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
    float2 _padd0;

    float3 WhiteBalance;
    float _padd1;

    float3 ChannelMaskRed;
    float _padd2;
    float3 ChannelMaskGreen;
    float _padd3;
    float3 ChannelMaskBlue;
    float _padd4;
};

float3 NeutralCurve(float3 x)
{
    const float A = ShoulderStrength;
    const float B = LinearStrength;
    const float C = LinearAngle;
    const float D = ToeStrength;

    // Not exposed as settings
    const float E = 0.02f;
    const float F = 0.3f;

    return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

float3 NeutralTonemap(float3 x)
{
    float3 whiteScale = 1 / NeutralCurve(WhiteLevel);
    x = NeutralCurve(whiteScale * x);
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

float3 OECF_sRGBFast(float3 color)
{
    return pow(color.rgb, float3(1.0 / GAMMA, 1.0 / GAMMA, 1.0 / GAMMA));
}

float3 AdjustWhiteBalance(float3 color)
{
    float3 lms = LinearToLMS(color);
    lms *= WhiteBalance;
    return LMSToLinear(lms);
}

float3 AdjustHueSaturation(float3 color, float hueShift, float saturation)
{
	// Convert RGB to HSL (Hue, Saturation, Lightness) color space.
    float3 hsl = RGBtoHSL(color);

	// Adjust Hue and Saturation.
    hsl.x += hueShift;
    hsl.y = saturate(hsl.y * saturation);

	// Convert back to RGB color space.
    return HSLtoRGB(hsl);
}

float3 AdjustContrast(float3 color)
{
    return ((color - 0.5f) * Contrast) + 0.5f;
}

float3 ChannelMix(float3 color)
{
    float4 output;
    output.r = dot(color, ChannelMaskRed);
    output.g = dot(color, ChannelMaskGreen);
    output.b = dot(color, ChannelMaskBlue);
    return output;
}

float4 main(VSOut vs) : SV_Target
{
    float4 color = hdrTexture.Sample(linearClampSampler, vs.Tex);

    if (color.a == 0)
        discard;

    color.rgb = Tonemap(color.rgb);

    color.rgb = color.rgb * PostExposure;
    color.rgb = AdjustWhiteBalance(color.rgb);
    color.rgb = AdjustHueSaturation(color.rgb, HueShift, Saturation);
    color.rgb = AdjustContrast(color.rgb);
    color.rgb = ChannelMix(color.rgb);

    color.rgb = OECF_sRGBFast(color.rgb);

    return color;
}

#ifndef LUT_TileSizeXY
#define LUT_TileSizeXY 32
#endif
#ifndef LUT_TileAmount
#define LUT_TileAmount 32
#endif

cbuffer BakeCBuffer
{
    uint tileIndex;
};

float4 bake(VSOut vs)
{
    float2 tileCoord = vs.Tex;
    float r = tileCoord.x;
    float g = tileCoord.y;
    float b = (float) tileIndex / (float) LUT_TileAmount;
    float3 color = float3(r, g, b);

    color.rgb = AdjustWhiteBalance(color.rgb);
    color.rgb = AdjustHueSaturation(color.rgb, HueShift, Saturation);
    color.rgb = AdjustContrast(color.rgb);
    color.rgb = ChannelMix(color.rgb);

    return float4(color, 1);
}