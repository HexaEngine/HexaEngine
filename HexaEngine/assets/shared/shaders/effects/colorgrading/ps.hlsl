#include "../../colorUtils.hlsl"
Texture2D hdrTexture : register(t0);
Texture2D lutTexture : register(t1);
SamplerState linearClampSampler : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer TonemapParams
{
	float BlackIn; 		// Inner control point for the black point.
	float WhiteIn; 		// Inner control point for the white point.
	float BlackOut;		// Outer control point for the black point.
	float WhiteOut;		// Outer control point for the white point.
	float WhiteLevel;	// Pre - curve white point adjustment.
	float WhiteClip;	// Post - curve white point adjustment.
	float PostExposure;  // Adjusts overall exposure in EV units.
	float Temperature;   // Sets the white balance to a custom color temperature.
	float Tint;          // Sets the white balance to compensate for tint (green or magenta).
	float HueShift;      // Shift the hue of all colors.
	float Saturation;    // Adjusts saturation (color intensity).
	float Contrast;      // Adjusts the contrast.
};

#define GAMMA 2.2

float3 NoneTonemap(float3 x)
{
	return x;
}

float3 HableHejlTonemap(float3 color)
{
	// Calculate linear scale and offset for black and white points
	float scale = (WhiteOut - BlackOut) / (WhiteIn - BlackIn);
	float offset = BlackOut - (BlackIn * scale);

	// Apply linear scale and offset
	color = color * scale + offset;

	// Apply gamma correction (2.2)
	color = pow(color, 1.0 / 2.2);

	// Pre-curve white point adjustment
	color = color * (WhiteLevel / max(color.r, max(color.g, color.b)));

	// Post-curve white point adjustment
	color = (color * (1.0 + color / (WhiteClip * WhiteClip))) / (1.0 + color);

	return color;
}

float3 ACESFilmTonemap(float3 x)
{
	return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 OECF_sRGBFast(float3 color)
{
	return pow(color.rgb, float3(1.0 / GAMMA, 1.0 / GAMMA, 1.0 / GAMMA));
}

float3 AdjustWhiteBalance(float3 color)
{
	float t1 = Temperature * 10 / 6;
    float t2 = Tint * 10 / 6;

    // Get the CIE xy chromaticity of the reference white point.
    // Note: 0.31271 = x value on the D65 white point
    float x = 0.31271 - t1 * (t1 < 0 ? 0.1 : 0.05);
    float standardIlluminantY = 2.87 * x - 3 * x * x - 0.27509507;
    float y = standardIlluminantY + t2 * 0.05;

    // Calculate the coefficients in the LMS space.
    float3 w1 = float3(0.949237, 1.03542, 1.08728); // D65 white point

    // CIExyToLMS
    float Y = 1;
    float X = Y * x / y;
    float Z = Y * (1 - x - y) / y;
    float L = 0.7328 * X + 0.4296 * Y - 0.1624 * Z;
    float M = -0.7036 * X + 1.6975 * Y + 0.0061 * Z;
    float S = 0.0030 * X + 0.0136 * Y + 0.9834 * Z;
    float3 w2 = float3(L, M, S);

    float3 balance = float3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);

	float3 lms = LinearToLMS(color);
	lms *= balance;
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

float4 main(VSOut vs) : SV_Target
{
	float4 color = hdrTexture.Sample(linearClampSampler, vs.Tex);

	color.rgb = ACESFilmTonemap(color.rgb);

	color.rgb = color.rgb * PostExposure;
	color.rgb = AdjustWhiteBalance(color.rgb);
	color.rgb = AdjustHueSaturation(color.rgb, HueShift, Saturation);
	color.rgb = AdjustContrast(color.rgb);

	color.rgb = OECF_sRGBFast(color.rgb);

	return color;
}