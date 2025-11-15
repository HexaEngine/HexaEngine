#include "HexaEngine.Core:shaders/camera.hlsl"

Texture2D<float> depthTex : register(t0);
SamplerState linearWrapSampler : register(s0);

cbuffer DofParams
{
	float focusDistance;
    float focusRange;
    float2 focusCenter;
    bool autofocus;
    int autofocusSamples;
    float autofocusRadius;
};

float FocusPoint()
{
    if (autofocusRadius && autofocusSamples > 0)
    {
        const float2 texelSize = 1.0 / screenDim;

        float result = 0.0;
        float count = 0.0;

		[unroll(8)]
        for (int x = -autofocusSamples; x <= autofocusSamples; ++x)
        {
			[unroll(8)]
            for (int y = -autofocusSamples; y <= autofocusSamples; ++y)
            {
                float2 offset = float2(x, y) * autofocusRadius * texelSize;

                float depth = depthTex.Sample(linearWrapSampler, focusCenter + offset);

                if (depth == 1)
                    continue;

                float d = GetLinearDepth(depth);
                result += d;
                count++;
            }
        }

        if (count == 0)
        {
            return GetLinearDepth(1);
        }
        else
        {
            return result / count;
        }
    }
    else
    {
        return focusDistance;
    }
}

float GetCircleOfConfusion(in float depth)
{
    float focusPoint = FocusPoint();
    float coc = (depth - focusPoint) / focusRange;
	coc = clamp(coc, -1, 1);
    return coc;
}

struct VertexOut
{
    float4 PosH : SV_POSITION;
    float2 Tex : TEXCOORD;
};

float4 main(VertexOut pin) : SV_TARGET
{
    float depth = depthTex.Sample(linearWrapSampler, pin.Tex);

    depth = GetLinearDepth(depth);

	float coc = GetCircleOfConfusion(depth);

	float remappedCoc = (coc + 1.0) / 2.0;

    return float4(remappedCoc, remappedCoc, remappedCoc, 1.0);
}