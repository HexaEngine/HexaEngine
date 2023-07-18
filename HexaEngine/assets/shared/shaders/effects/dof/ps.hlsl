#include "../../camera.hlsl"

Texture2D scene_texture : register(t0);
Texture2D blurred_texture : register(t1);
Texture2D depth_map : register(t2);

SamplerState linear_wrap_sampler : register(s0);

cbuffer DofParams
{
    float focusRange;
    float2 focusCenter;
    bool autofocus;
    float autofocusSamples;
    float autofocusRadius;
};

float FocusPoint()
{
    float focusPoint;

    if (autofocusRadius && autofocusSamples > 0)
    {
        float2 texelSize = 1.0 / screenDim;

        float result = 0.0;
        float count = 0.0;

		[unroll(8)]
        for (int x = -autofocusSamples; x <= autofocusSamples; ++x)
        {
			[unroll(8)]
            for (int y = -autofocusSamples; y <= autofocusSamples; ++y)
            {
                float2 offset = float2(x, y) * autofocusRadius * texelSize;
                float d = SampleLinearDepth(depth_map, linear_wrap_sampler, focusCenter + offset);
                result += d;
                count++;
            }
        }

        result /= count;
        focusPoint = result;
    }
    else
    {
        focusPoint = SampleLinearDepth(depth_map, linear_wrap_sampler, focusCenter);
    }

    return focusPoint;
}

float BlurFactor(in float depth)
{
    float focusPoint = FocusPoint();
    float blur = (depth - focusPoint) / focusRange;
    blur = clamp(blur, -1, 1);
    if (blur < 0)
    {
        blur = blur * -half4(1, 0, 0, 1);
    }
    return blur;
}

float3 DistanceDOF(float3 colorFocus, float3 colorBlurred, float depth)
{
    float blurFactor = BlurFactor(depth);
    return lerp(colorFocus, colorBlurred, blurFactor);
}

struct VertexOut
{
    float4 PosH : SV_POSITION;
    float2 Tex : TEXCOORD;
};

float4 main(VertexOut pin) : SV_TARGET
{
    float4 color = scene_texture.Sample(linear_wrap_sampler, pin.Tex);

    float depth = depth_map.Sample(linear_wrap_sampler, pin.Tex);

    float3 colorBlurred = blurred_texture.Sample(linear_wrap_sampler, pin.Tex).xyz;

    depth = GetLinearDepth(depth);

    color = float4(DistanceDOF(color.xyz, colorBlurred, depth), 1.0);

    return color;
}