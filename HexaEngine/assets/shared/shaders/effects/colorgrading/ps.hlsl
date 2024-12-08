#include "../../colorUtils.hlsl"
Texture2D inputTex : register(t0);
Texture1DArray curvesTex : register(t1);
SamplerState linearClampSampler : register(s0);

#ifndef TONEMAP
/*
 * 0: AECS Film
 * 1: Neutral
 * 2: None
 *
 *
 * fallback: None
*/
#define TONEMAP 2
#endif

#if TONEMAP == 0
#define UseACES 1
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

	float3 Lift;
	float _padd5;
	float3 GammaInv;
	float _padd6;
	float3 Gain;
	float _padd7;

	float3 Offset;
	float _padd8;
    float3 Power;
	float _padd9;
    float3 Slope;
	float _padd10;
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

float3 ACESFilmTonemap(const float3 x)
{
	const float a = 2.51;
	const float b = 0.03;
	const float c = 2.43;
	const float d = 0.59;
	const float e = 0.14;
	return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}

float3 ACESFilmRec2020(float3 x)
{
	const float a = 15.8f;
	const float b = 2.12f;
	const float c = 1.2f;
	const float d = 5.92f;
	const float e = 1.9f;
	return (x * (a * x + b)) / (x * (c * x + d) + e);
}

//
// Filmic tonemapping (ACES fitting, unless TONEMAPPING_USE_FULL_ACES is set to 1)
// Input is ACES2065-1 (AP0 w/ linear encoding)
//
half3 FilmicTonemap(half3 aces)
{
#if TONEMAPPING_USE_FULL_ACES

    half3 oces = RRT(aces);
    half3 odt = ODT_RGBmonitor_100nits_dim(oces);
    return odt;

#else

    // --- Glow module --- //
    half saturation = rgb_2_saturation(aces);
    half ycIn = rgb_2_yc(aces);
    half s = sigmoid_shaper((saturation - 0.4) / 0.2);
    half addedGlow = 1.0 + glow_fwd(ycIn, RRT_GLOW_GAIN * s, RRT_GLOW_MID);
    aces *= addedGlow;

    // --- Red modifier --- //
    half hue = rgb_2_hue(aces);
    half centeredHue = center_hue(hue, RRT_RED_HUE);
    half hueWeight;
    {
        //hueWeight = cubic_basis_shaper(centeredHue, RRT_RED_WIDTH);
        hueWeight = pow2(smoothstep(0.0, 1.0, 1.0 - abs(2.0 * centeredHue / RRT_RED_WIDTH)));
    }

    aces.r += hueWeight * saturation * (RRT_RED_PIVOT - aces.r) * (1.0 - RRT_RED_SCALE);

    // --- ACES to RGB rendering space --- //
    half3 acescg = max(0.0, ACES_to_ACEScg(aces));

    // --- Global desaturation --- //
    //acescg = mul(RRT_SAT_MAT, acescg);
    acescg = lerp(dot(acescg, AP1_RGB2Y).xxx, acescg, RRT_SAT_FACTOR.xxx);

    // Luminance fitting of *RRT.a1.0.3 + ODT.Academy.RGBmonitor_100nits_dim.a1.0.3*.
    // https://github.com/colour-science/colour-unity/blob/master/Assets/Colour/Notebooks/CIECAM02_Unity.ipynb
    // RMSE: 0.0012846272106
    const half a = 278.5085;
    const half b = 10.7772;
    const half c = 293.6045;
    const half d = 88.7122;
    const half e = 80.6889;
    half3 x = acescg;
    half3 rgbPost = (x * (a * x + b)) / (x * (c * x + d) + e);

    // Scale luminance to linear code value
    // half3 linearCV = Y_2_linCV(rgbPost, CINEMA_WHITE, CINEMA_BLACK);

    // Apply gamma adjustment to compensate for dim surround
    half3 linearCV = darkSurround_to_dimSurround(rgbPost);

    // Apply desaturation to compensate for luminance difference
    //linearCV = mul(ODT_SAT_MAT, color);
    linearCV = lerp(dot(linearCV, AP1_RGB2Y).xxx, linearCV, ODT_SAT_FACTOR.xxx);

    // Convert to display primary encoding
    // Rendering space RGB to XYZ
    half3 XYZ = mul(AP1_2_XYZ_MAT, linearCV);

    // Apply CAT from ACES white point to assumed observer adapted white point
    XYZ = mul(D60_2_D65_CAT, XYZ);

    // CIE XYZ to display primaries
    linearCV = mul(XYZ_2_REC709_MAT, XYZ);

    return linearCV;

#endif
}

float3 Tonemap(float3 x)
{
#if UseACES
	x = ACEScg_to_ACES(x);
#endif

#if TONEMAP == 0
#if HDR
	return FilmicTonemap(x);
#else
	return ACESFilmTonemap(x);
#endif
#elif TONEMAP == 1
	x = ACEScg_to_sRGB(x);
	return NeutralTonemap(x);
#elif TONEMAP == 2
	return ACEScg_to_sRGB(x);
#else
	return ACEScg_to_sRGB(x);
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
#if UseACES
	float luma = AcesLuminance(c);
#else
	float luma = Luminance(c);
#endif
	return luma.xxx + sat.xxx * (c - luma.xxx);
}

float3 AdjustContrast(float3 c, float contrast)
{
	return (c - ACEScc_MIDGRAY) * contrast + ACEScc_MIDGRAY;
}

//
// Offset, Power, Slope (ASC-CDL)
// Works in Log & Linear. Results will be different but still correct.
//
half3 OffsetPowerSlope(half3 c, half3 offset, half3 power, half3 slope)
{
    half3 so = c * slope + offset;
    so = so > (0.0).xxx ? pow(so, power) : so;
    return so;
}

float3 LiftGammaGain(float3 c, float3 lift, float3 invgamma, float3 gain)
{
    //return gain * (lift * (1.0 - c) + pow(max(c, kEpsilon), invgamma));
    //return pow(gain * (c + lift * (1.0 - c)), invgamma);

    half3 power = invgamma;
    half3 offset = lift * gain;
    half3 slope = ((1.0).xxx - lift) * gain;
    return OffsetPowerSlope(c, offset, power, slope);
}

float3 ChannelMix(float3 color)
{
	return float3(
		dot(color, ChannelMaskRed),
		dot(color, ChannelMaskGreen),
		dot(color, ChannelMaskBlue)
	);
}

float3 ApplyYrgbCurve(float3 c)
{
	const float kHalfPixel = (1.0 / 128.0) / 2.0;
	c += kHalfPixel.xxx;

	float mr = curvesTex.SampleLevel(linearClampSampler, float2(c.r, 3), 0);
	float mg = curvesTex.SampleLevel(linearClampSampler, float2(c.g, 3), 0);
	float mb = curvesTex.SampleLevel(linearClampSampler, float2(c.b, 3), 0);
	c = saturate(float3(mr, mg, mb));

	float r = curvesTex.SampleLevel(linearClampSampler, float2(c.r, 0), 0);
	float g = curvesTex.SampleLevel(linearClampSampler, float2(c.g, 1), 0);
	float b = curvesTex.SampleLevel(linearClampSampler, float2(c.b, 2), 0);

	return saturate(float3(r, g, b));
}

float CurveHueHue(float hue)
{
    float offset = saturate(curvesTex.SampleLevel(linearClampSampler, float2(hue, 4), 0)) - 0.5;
    hue += offset;
    hue = RotateHue(hue, 0.0, 1.0);
    return hue;
}

float CurveHueSat(float hue)
{
    return saturate(curvesTex.SampleLevel(linearClampSampler, float2(hue, 5), 0)) * 2.0;
}

float CurveSatSat(float sat)
{
    return saturate(curvesTex.SampleLevel(linearClampSampler, float2(sat, 6), 0)) * 2.0;
}

float CurveLumSat(float lum)
{
    return saturate(curvesTex.SampleLevel(linearClampSampler, float2(lum, 7), 0)) * 2.0;
}

float4 main(VSOut vs) : SV_Target
{
	float4 color = inputTex.Sample(linearClampSampler, vs.Tex); // linear space.

	if (color.a == 0)
		discard;

	color = LinearToSRGB(color);

	float3 aces = sRGB_to_ACES(color.rgb);

	float3 acescc = ACES_to_ACEScc(aces);

	acescc = OffsetPowerSlope(acescc, Offset, Power, Slope);

	float2 hs = RgbToHsv(acescc).xy;
	float satMultiplier = CurveHueSat(hs.x);
	satMultiplier *= CurveSatSat(hs.y);
    satMultiplier *= CurveLumSat(AcesLuminance(acescc));

	acescc = AdjustSaturation(acescc, Saturation * satMultiplier);
	acescc = AdjustContrast(acescc, Contrast);

	aces = ACEScc_to_ACES(acescc);

	float3 acescg = ACES_to_ACEScg(aces);

	acescg = AdjustWhiteBalance(acescg);
	acescg = LiftGammaGain(acescg, Lift, GammaInv, Gain);

	float3 hsv = RgbToHsv(max(acescg, 0.0));
	hsv.x = CurveHueHue(hsv.x + HueShift);
    acescg = HsvToRgb(hsv);

	acescg = ChannelMix(acescg);
	
	color.rgb = Tonemap(acescg.rgb);

	color.rgb = ApplyYrgbCurve(color.rgb);

	color.a = 1;

	return color;
}