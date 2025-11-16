#ifndef DISPLAY_H_INCLUDED
#define DISPLAY_H_INCLUDED

#include "colorUtils.hlsl"

#define COLORSPACE_RGB_FULL_G22_None_P709 0
#define COLORSPACE_RGB_FULL_G2084_None_P2020 1

cbuffer GlobalDisplayBuffer
{
	float SDRContentWhitePoint;
	float DisplayMaxNits;
	uint ColorSpace;
	float padding;
};

inline float3 XYZToDisplayPrimaries(float3 XYZ)
{
	switch (ColorSpace)
	{
	case COLORSPACE_RGB_FULL_G2084_None_P2020:
		return mul(XYZ_2_REC2020_MAT, XYZ);

	case COLORSPACE_RGB_FULL_G22_None_P709:
	default:
		return mul(XYZ_2_REC709_MAT, XYZ);
	}
}

inline float3 LinearToDisplayGamma(float3 color)
{
	switch (ColorSpace)
	{
	case COLORSPACE_RGB_FULL_G2084_None_P2020:
		return LinearToPQ(color * (DisplayMaxNits / DEFAULT_MAX_PQ));

	case COLORSPACE_RGB_FULL_G22_None_P709:
	default:
		return LinearToGamma22(color);
	}
}

inline float3 Rec709ToDisplayPrimaries(float3 color)
{
	if (ColorSpace == COLORSPACE_RGB_FULL_G2084_None_P2020)
		return Rec709ToRec2020(color.rgb);
	return color;
}

inline float3 Rec2020ToDisplayPrimaries(float3 color)
{
	if (ColorSpace == COLORSPACE_RGB_FULL_G22_None_P709)
		return Rec2020ToRec709(color.rgb);
	return color;
}

#endif