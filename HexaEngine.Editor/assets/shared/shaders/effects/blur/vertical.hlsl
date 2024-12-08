#ifndef GAUSSIAN_RADIUS
#define GAUSSIAN_RADIUS 3
#endif

#if GAUSSIAN_RADIUS == 3
#include "gaussian3x3.hlsl"
#elif GAUSSIAN_RADIUS == 5
#include "gaussian5x5.hlsl"
#elif GAUSSIAN_RADIUS == 7
#include "gaussian7x7.hlsl"
#else
#error Gaussian radius not supported.
#endif

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

// The input texture to blur.
Texture2D tex : register(t0);
SamplerState linearSampler : register(s0);

cbuffer GaussianBlurConstantBuffer : register(b0)
{
	float2 textureDimensions; // The render target width/height.
};

// Gaussian blur in the vertical direction.
float4 main(VSOut input) : SV_TARGET
{
	return blur(tex, linearSampler, float2(0, 1), input.Tex, textureDimensions);
}