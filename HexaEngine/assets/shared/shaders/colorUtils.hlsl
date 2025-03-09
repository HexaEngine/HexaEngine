#ifndef COLOR_UTILS_H_INCLUDED
#define COLOR_UTILS_H_INCLUDED

#include "math.hlsl"
#include "aces.hlsl"

#ifndef DEFAULT_MAX_PQ
#define DEFAULT_MAX_PQ 10000.0
#endif

// Gamma20
float Gamma20ToLinear(float c)
{
	return c * c;
}

float3 Gamma20ToLinear(float3 c)
{
	return c.rgb * c.rgb;
}

float4 Gamma20ToLinear(float4 c)
{
	return float4(Gamma20ToLinear(c.rgb), c.a);
}

float LinearToGamma20(float c)
{
	return sqrt(c);
}

float3 LinearToGamma20(float3 c)
{
	return sqrt(c.rgb);
}

float4 LinearToGamma20(float4 c)
{
	return float4(LinearToGamma20(c.rgb), c.a);
}

// Gamma22
float Gamma22ToLinear(float c)
{
	return PositivePow(c, 2.2);
}

float3 Gamma22ToLinear(float3 c)
{
	return PositivePow(c.rgb, float3(2.2, 2.2, 2.2));
}

float4 Gamma22ToLinear(float4 c)
{
	return float4(Gamma22ToLinear(c.rgb), c.a);
}

float LinearToGamma22(float c)
{
	return PositivePow(c, 0.454545454545455);
}

float3 LinearToGamma22(float3 c)
{
	return PositivePow(c.rgb, float3(0.454545454545455, 0.454545454545455, 0.454545454545455));
}

float4 LinearToGamma22(float4 c)
{
	return float4(LinearToGamma22(c.rgb), c.a);
}

// sRGB
float SRGBToLinear(float c)
{
	float linearRGBLo = c / 12.92;
	float linearRGBHi = PositivePow((c + 0.055) / 1.055, 2.4);
	float linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
	return linearRGB;
}

float2 SRGBToLinear(float2 c)
{
	float2 linearRGBLo = c / 12.92;
	float2 linearRGBHi = PositivePow((c + 0.055) / 1.055, float2(2.4, 2.4));
	float2 linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
	return linearRGB;
}

float3 SRGBToLinear(float3 c)
{
	float3 linearRGBLo = c / 12.92;
	float3 linearRGBHi = PositivePow((c + 0.055) / 1.055, float3(2.4, 2.4, 2.4));
	float3 linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
	return linearRGB;
}

float4 SRGBToLinear(float4 c)
{
	return float4(SRGBToLinear(c.rgb), c.a);
}

float LinearToSRGB(float c)
{
	float sRGBLo = c * 12.92;
	float sRGBHi = (PositivePow(c, 1.0 / 2.4) * 1.055) - 0.055;
	float sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
	return sRGB;
}

float2 LinearToSRGB(float2 c)
{
	float2 sRGBLo = c * 12.92;
	float2 sRGBHi = (PositivePow(c, float2(1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
	float2 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
	return sRGB;
}

float3 LinearToSRGB(float3 c)
{
	float3 sRGBLo = c * 12.92;
	float3 sRGBHi = (PositivePow(c, float3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
	float3 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
	return sRGB;
}

float4 LinearToSRGB(float4 c)
{
	return float4(LinearToSRGB(c.rgb), c.a);
}

// TODO: Seb - To verify and refit!
// Ref: http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
float FastSRGBToLinear(float c)
{
	return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
}

float2 FastSRGBToLinear(float2 c)
{
	return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
}

float3 FastSRGBToLinear(float3 c)
{
	return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
}

float4 FastSRGBToLinear(float4 c)
{
	return float4(FastSRGBToLinear(c.rgb), c.a);
}

float FastLinearToSRGB(float c)
{
	return saturate(1.055 * PositivePow(c, 0.416666667) - 0.055);
}

float2 FastLinearToSRGB(float2 c)
{
	return saturate(1.055 * PositivePow(c, 0.416666667) - 0.055);
}

float3 FastLinearToSRGB(float3 c)
{
	return saturate(1.055 * PositivePow(c, 0.416666667) - 0.055);
}

float4 FastLinearToSRGB(float4 c)
{
	return float4(FastLinearToSRGB(c.rgb), c.a);
}

static float3 HUEtoRGB(in float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return saturate(float3(R, G, B));
}

static const float Epsilon = 1e-10;

float3 RGBtoHCV(in float3 RGB)
{
	// Based on work by Sam Hocevar and Emil Persson
	float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
	float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
	float C = Q.x - min(Q.w, Q.y);
	float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
	return float3(H, C, Q.x);
}

float3 HSVtoRGB(in float3 HSV)
{
	float3 RGB = HUEtoRGB(HSV.x);
	return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

float3 HSLtoRGB(in float3 HSL)
{
	float3 RGB = HUEtoRGB(HSL.x);
	float C = (1 - abs(2 * HSL.z - 1)) * HSL.y;
	return (RGB - 0.5) * C + HSL.z;
}

// The weights of RGB contributions to luminance.
// Should sum to sRGB.
static const float3 HCYwts = float3(0.299, 0.587, 0.114);

float3 HCYtoRGB(in float3 HCY)
{
	float3 RGB = HUEtoRGB(HCY.x);
	float Z = dot(RGB, HCYwts);
	if (HCY.z < Z)
	{
		HCY.y *= HCY.z / Z;
	}
	else if (Z < 1)
	{
		HCY.y *= (1 - HCY.z) / (1 - Z);
	}
	return (RGB - Z) * HCY.y + HCY.z;
}

static const float HCLgamma = 3;
static const float HCLy0 = 100;
static const float HCLmaxL = 0.530454533953517; // == exp(HCLgamma / HCLy0) - 0.5

float3 HCLtoRGB(in float3 HCL)
{
	float3 RGB = 0;
	if (HCL.z != 0)
	{
		float H = HCL.x;
		float C = HCL.y;
		float L = HCL.z * HCLmaxL;
		float Q = exp((1 - C / (2 * L)) * (HCLgamma / HCLy0));
		float U = (2 * L - C) / (2 * Q - 1);
		float V = C / Q;
		float A = (H + min(frac(2 * H) / 4, frac(-2 * H) / 8)) * PI * 2;
		float T;
		H *= 6;
		if (H <= 0.999)
		{
			T = tan(A);
			RGB.r = 1;
			RGB.g = T / (1 + T);
		}
		else if (H <= 1.001)
		{
			RGB.r = 1;
			RGB.g = 1;
		}
		else if (H <= 2)
		{
			T = tan(A);
			RGB.r = (1 + T) / T;
			RGB.g = 1;
		}
		else if (H <= 3)
		{
			T = tan(A);
			RGB.g = 1;
			RGB.b = 1 + T;
		}
		else if (H <= 3.999)
		{
			T = tan(A);
			RGB.g = 1 / (1 + T);
			RGB.b = 1;
		}
		else if (H <= 4.001)
		{
			RGB.g = 0;
			RGB.b = 1;
		}
		else if (H <= 5)
		{
			T = tan(A);
			RGB.r = -1 / T;
			RGB.b = 1;
		}
		else
		{
			T = tan(A);
			RGB.r = 1;
			RGB.b = -T;
		}
		RGB = RGB * V + U;
	}
	return RGB;
}

float3 RGBtoHSV(in float3 RGB)
{
	float3 HCV = RGBtoHCV(RGB);
	float S = HCV.y / (HCV.z + Epsilon);
	return float3(HCV.x, S, HCV.z);
}

float3 RGBtoHSL(in float3 RGB)
{
	float3 HCV = RGBtoHCV(RGB);
	float L = HCV.z - HCV.y * 0.5;
	float S = HCV.y / (1 - abs(L * 2 - 1) + Epsilon);
	return float3(HCV.x, S, L);
}

float3 RGBtoHCY(in float3 RGB)
{
	// Corrected by David Schaeffer
	float3 HCV = RGBtoHCV(RGB);
	float Y = dot(RGB, HCYwts);
	float Z = dot(HUEtoRGB(HCV.x), HCYwts);
	if (Y < Z)
	{
		HCV.y *= Z / (Epsilon + Y);
	}
	else
	{
		HCV.y *= (1 - Z) / (Epsilon + 1 - Y);
	}
	return float3(HCV.x, HCV.y, Y);
}

float3 RGBtoHCL(in float3 RGB)
{
	float3 HCL;
	float H = 0;
	float U = min(RGB.r, min(RGB.g, RGB.b));
	float V = max(RGB.r, max(RGB.g, RGB.b));
	float Q = HCLgamma / HCLy0;
	HCL.y = V - U;
	if (HCL.y != 0)
	{
		H = atan2(RGB.g - RGB.b, RGB.r - RGB.g) / PI;
		Q *= U / V;
	}
	Q = exp(Q);
	HCL.x = frac(H / 2 - min(frac(H), frac(-H)) / 6);
	HCL.y *= Q;
	HCL.z = lerp(-U, V, Q) / (HCLmaxL * 2);
	return HCL;
}

float3 RgbToHsv(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
	float d = q.x - min(q.w, q.y);
	float e = Epsilon;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HsvToRgb(float3 c)
{
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float RotateHue(float value, float low, float hi)
{
	return (value < low)
		? value + hi
		: (value > hi)
		? value - hi
		: value;
}

// Converts linear RGB to LMS
float3 LinearToLMS(float3 x)
{
	const float3x3 LIN_2_LMS_MAT =
	{
		3.90405e-1, 5.49941e-1, 8.92632e-3,
		7.08416e-2, 9.63172e-1, 1.35775e-3,
		2.31082e-2, 1.28021e-1, 9.36245e-1
	};

	return mul(x, LIN_2_LMS_MAT);
}

float3 LMSToLinear(float3 x)
{
	const float3x3 LMS_2_LIN_MAT =
	{
		2.85847e+0, -1.62879e+0, -2.48910e-2,
		-2.10182e-1, 1.15820e+0, 3.24281e-4,
		-4.18120e-2, -1.18169e-1, 1.06867e+0
	};

	return mul(x, LMS_2_LIN_MAT);
}

inline float Luminance(float3 c)
{
	const float3 lum = float3(0.2126, 0.7152, 0.0722);
	return dot(c, lum);
}

#define ACEScc_MAX      1.4679964
#define ACEScc_MIDGRAY  0.4135884

float AcesLuminance(float3 linearRgb)
{
	return dot(linearRgb, AP1_RGB2Y);
}

float3 LinearToACEScg(float3 color)
{
	float3x3 ACES_Matrix = float3x3(
		0.59719, 0.07600, 0.02840,
		0.35458, 0.90834, 0.13383,
		0.04823, 0.01566, 0.83777
	);
	return mul(color, ACES_Matrix);
}

float3 ACEScgToLinear(float3 color)
{
	float3x3 ACES_InverseMatrix = float3x3(
		1.60475, -0.10208, -0.00327,
		-0.53108, 1.10813, -0.07276,
		-0.07367, -0.00605, 1.07602
	);
	return mul(color, ACES_InverseMatrix);
}

// Alexa LogC converters (El 1000)
// See http://www.vocas.nl/webfm_send/964
// Max range is ~58.85666

// Set to 1 to use more precise but more expensive log/linear conversions. I haven't found a proper
// use case for the high precision version yet so I'm leaving this to 0.

#define USE_PRECISE_LOGC 0

struct ParamsLogC
{
	float cut;
	float a, b, c, d, e, f;
};

static const ParamsLogC LogC =
{
	0.011361, // cut
	5.555556, // a
	0.047996, // b
	0.244161, // c
	0.386036, // d
	5.301883, // e
	0.092819  // f
};

float LinearToLogC_Precise(float x)
{
	float o;
	if (x > LogC.cut)
		o = LogC.c * log10(LogC.a * x + LogC.b) + LogC.d;
	else
		o = LogC.e * x + LogC.f;
	return o;
}

float3 LinearToLogC(float3 x)
{
#if USE_PRECISE_LOGC
	return float3(
		LinearToLogC_Precise(x.x),
		LinearToLogC_Precise(x.y),
		LinearToLogC_Precise(x.z)
	);
#else
	return LogC.c * log10(LogC.a * x + LogC.b) + LogC.d;
#endif
}

float LogCToLinear_Precise(float x)
{
	float o;
	if (x > LogC.e * LogC.cut + LogC.f)
		o = (pow(10.0, (x - LogC.d) / LogC.c) - LogC.b) / LogC.a;
	else
		o = (x - LogC.f) / LogC.e;
	return o;
}

float3 LogCToLinear(float3 x)
{
#if USE_PRECISE_LOGC
	return float3(
		LogCToLinear_Precise(x.x),
		LogCToLinear_Precise(x.y),
		LogCToLinear_Precise(x.z)
	);
#else
	return (pow(10.0, (x - LogC.d) / LogC.c) - LogC.b) / LogC.a;
#endif
}

//
// SMPTE ST.2084 (PQ) transfer functions
// Used for HDR Lut storage, max range depends on the maxPQValue parameter
//
struct ParamsPQ
{
	float N, M;
	float C1, C2, C3;
};

static const ParamsPQ PQ =
{
	2610.0 / 4096.0 / 4.0,   // N
	2523.0 / 4096.0 * 128.0, // M
	3424.0 / 4096.0,         // C1
	2413.0 / 4096.0 * 32.0,  // C2
	2392.0 / 4096.0 * 32.0,  // C3
};

float3 LinearToPQ(float3 x)
{
	x = PositivePow(x, PQ.N);
	float3 nd = (PQ.C1 + PQ.C2 * x) / (1.0 + PQ.C3 * x);
	return PositivePow(nd, PQ.M);
}

float3 PQToLinear(float3 x)
{
	x = PositivePow(x, rcp(PQ.M));
	float3 nd = max(x - PQ.C1, 0.0) / (PQ.C2 - (PQ.C3 * x));
	return PositivePow(nd, rcp(PQ.N));
}

float3 Rec709ToRec2020(float3 color)
{
	static const float3x3 conversion =
	{
		0.627402, 0.329292, 0.043306,
		0.069095, 0.919544, 0.011360,
		0.016394, 0.088028, 0.895578
	};
	return mul(conversion, color);
}

float3 Rec2020ToRec709(float3 color)
{
	static const float3x3 conversion =
	{
		1.660496, -0.587656, -0.072840,
		-0.124547, 1.132895, -0.008348,
		-0.018154, -0.100597, 1.118751
	};
	return mul(conversion, color);
}

#endif