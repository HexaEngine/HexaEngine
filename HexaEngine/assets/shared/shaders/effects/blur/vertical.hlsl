#include "gaussian3x3.hlsl"

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