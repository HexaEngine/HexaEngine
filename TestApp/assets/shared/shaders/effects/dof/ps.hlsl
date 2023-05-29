#include "../../camera.hlsl"


struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D positionTex : register(t0);
Texture2D noiseTex : register(t1);
Texture2D focusTex : register(t2);
Texture2D outOfFocusTex : register(t3);

SamplerState samplerState
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};

cbuffer Params
{
    float focusRange;
    float padding;
    float2 focusCenter;
    bool enabled;
    bool autoFocusEnabled;
    int autoFocusSamples;
    float autoFocusRadius;
};



float4 main(VSOut input) : SV_TARGET
{
    float2 texCoord = input.Tex;
    float4 focusColor = focusTex.Sample(samplerState, texCoord);
    if (!enabled)
    {
        return focusColor;
    }

    float depth = SampleLinearDepth(positionTex, samplerState, texCoord);

    if (depth <= 0)
    {
        return focusColor;
    }

    float4 outOfFocusColor = outOfFocusTex.Sample(samplerState, texCoord);
    float focusPoint;

    if (autoFocusEnabled && autoFocusSamples > 0)
    {
        float width;
        float heigth;
        positionTex.GetDimensions(width, heigth);

        float2 texelSize = 1.0 / float2(width, heigth);

        float result = 0.0;
        float count = 0.0;
#if DEBUG
		bool isFocus;
		float maxZ = 0;
		float currentZ = 0;
#endif
		[unroll(8)]
        for (int x = -autoFocusSamples; x <= autoFocusSamples; ++x)
        {
			[unroll(8)]
            for (int y = -autoFocusSamples; y <= autoFocusSamples; ++y)
            {
                float2 offset = float2(float(x), float(y)) * autoFocusRadius * texelSize;
                float d = SampleLinearDepth(positionTex, samplerState, focusCenter + offset);
                result += d;
                count++;
#if DEBUG
				if (d > maxZ)
					maxZ = d;
				float2 diff = input.Tex - (focusCenter + offset);
				if (diff.x < 2.5f * texelSize.x && diff.y < 2.5f * texelSize.y && diff.x > -2.5f * texelSize.x && diff.y > -2.5f * texelSize.y)
				{
					isFocus = true;
					currentZ = d;
				}
#endif
            }
        }

        result /= count;
        focusPoint = result;

#if DEBUG
		if (isFocus)
		{
			return float4(0, currentZ / maxZ, 0, 1);
		}
#endif
    }
    else
    {
        focusPoint = SampleLinearDepth(positionTex, samplerState, focusCenter);
    }


    float blur = (depth - focusPoint) / focusRange;
    blur = clamp(blur, -1, 1);
    if (blur < 0)
    {
        blur = blur * -half4(1, 0, 0, 1);
    }

    float4 color = lerp(focusColor, outOfFocusColor, blur);

    return color;
}