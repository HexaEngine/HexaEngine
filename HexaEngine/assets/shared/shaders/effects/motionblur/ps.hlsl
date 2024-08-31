//https://developer.nvidia.com/gpugems/gpugems3/part-iv-image-effects/chapter-27-motion-blur-post-processing-effect

Texture2D inputTex : register(t0);
Texture2D<float2> velocityBufferTex : register(t1);

SamplerState linearWrapSampler : register(s0);

cbuffer MotionBlurCB : register(b0)
{
    float strength;
};

#ifndef MOTION_BLUR_QUALITY
#define MOTION_BLUR_QUALITY 0
#endif

// Low
#if MOTION_BLUR_QUALITY == 0
#define SAMPLE_COUNT 4
#endif

// Mid
#if MOTION_BLUR_QUALITY == 1
#define SAMPLE_COUNT 8
#endif

// High
#if MOTION_BLUR_QUALITY == 2
#define SAMPLE_COUNT 16
#endif

// Extreme
#if MOTION_BLUR_QUALITY == 3
#define SAMPLE_COUNT 32
#endif

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut pin) : SV_TARGET
{
    float2 velocity = velocityBufferTex.SampleLevel(linearWrapSampler, pin.Tex, 0) * strength;

    float2 tex_coord = pin.Tex;

    float4 color = inputTex.Sample(linearWrapSampler, tex_coord);

    tex_coord += velocity;

    for (int i = 1; i < SAMPLE_COUNT; ++i)
    {
        float4 currentColor = inputTex.Sample(linearWrapSampler, tex_coord);

        color += currentColor;

        tex_coord += velocity;
    }

    float4 final_color = color / SAMPLE_COUNT;

    return final_color;
}