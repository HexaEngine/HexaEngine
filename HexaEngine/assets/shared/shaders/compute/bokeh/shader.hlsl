#include "../../camera.hlsl"

struct DofParams
{
    float focusRange;
    float2 focusCenter;
    bool autofocus;
    float autofocusSamples;
    float autofocusRadius;
    float2 padd;
};

cbuffer BokehParams
{
    DofParams dof_params;
    float bokeh_fallout;
    float bokeh_radius_scale;
    float bokeh_color_scale;
    float bokeh_blur_threshold;
    float bokeh_lum_threshold;
};

static const float4 LUM_FACTOR = float4(0.299, 0.587, 0.114, 0);

Texture2D<float4> HDRTex : register(t0);
Texture2D<float> DepthTex : register(t1);

struct Bokeh
{
    float3 Position;
    float2 Size;
    float3 Color;
};

AppendStructuredBuffer<Bokeh> BokehStack : register(u0);

float FocusPoint()
{
    float focusPoint;
    float2 center = dof_params.focusCenter * screenDim;
    if (dof_params.autofocusRadius && dof_params.autofocusSamples > 0)
    {
        float result = 0.0;
        float count = 0.0;

		[unroll(8)]
        for (int x = -dof_params.autofocusSamples; x <= dof_params.autofocusSamples; ++x)
        {
			[unroll(8)]
            for (int y = -dof_params.autofocusSamples; y <= dof_params.autofocusSamples; ++y)
            {
                float2 offset = float2(x, y) * dof_params.autofocusRadius;
                float d = LoadLinearDepth(DepthTex, int3((int2) (center + offset), 0));
                result += d;
                count++;
            }
        }

        result /= count;
        focusPoint = result;
    }
    else
    {
        focusPoint = LoadLinearDepth(DepthTex, int3((int2) center, 0));
    }

    return focusPoint;
}

float BlurFactor(in float depth)
{
    float focusPoint = FocusPoint();
    float blur = (depth - focusPoint) / dof_params.focusRange;
    blur = clamp(blur, -1, 1);
    if (blur < 0)
    {
        blur = blur * -half4(1, 0, 0, 1);
    }
    return blur;
}

[numthreads(32, 32, 1)]
void main(uint3 dispatchThreadId : SV_DispatchThreadID)
{
    uint2 CurPixel = dispatchThreadId.xy;

    uint width, height, levels;
    HDRTex.GetDimensions(0, width, height, levels);

    float2 uv = float2(CurPixel.x, CurPixel.y) / float2(width - 1, height - 1);

    float depth = DepthTex.Load(int3(CurPixel, 0));

    float centerDepth = GetLinearDepth(depth);

    if (depth < 1.0f)
    {

        float centerBlur = BlurFactor(centerDepth);

        const uint NumSamples = 9;
        const uint2 SamplePoints[NumSamples] =
        {
            uint2(-1, -1), uint2(0, -1), uint2(1, -1),
		    uint2(-1, 0), uint2(0, 0), uint2(1, 0),
		    uint2(-1, 1), uint2(0, 1), uint2(1, 1)
        };

        float3 centerColor = HDRTex.Load(int3(CurPixel, 0)).rgb;

        float3 averageColor = 0.0f;
        for (uint i = 0; i < NumSamples; ++i)
        {
            float3 sample = HDRTex.Load(int3(CurPixel + SamplePoints[i], 0)).rgb;

            averageColor += sample;
        }
        averageColor /= NumSamples;

	    // Calculate the difference between the current texel and the average
        float averageBrightness = dot(averageColor, 1.0f);
        float centerBrightness = dot(centerColor, 1.0f);
        float brightnessDiff = max(centerBrightness - averageBrightness, 0.0f);

        [branch]
        if (brightnessDiff >= bokeh_lum_threshold && centerBlur > bokeh_blur_threshold)
        {
            Bokeh bPoint;
            bPoint.Position = float3(uv, centerDepth);
            bPoint.Size = centerBlur * bokeh_radius_scale / float2(width, height);

            float cocRadius = centerBlur * bokeh_radius_scale * 0.45f;
            float cocArea = cocRadius * cocRadius * 3.14159f;
            float falloff = pow(saturate(1.0f / cocArea), bokeh_fallout);
            bPoint.Color = centerColor * falloff;

            BokehStack.Append(bPoint);
        }
    }
}