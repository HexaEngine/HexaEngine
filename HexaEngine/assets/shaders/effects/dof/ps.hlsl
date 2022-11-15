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
	float minDistance;
	float maxDistance;
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

	float4 wPosition = positionTex.Sample(samplerState, texCoord);

	if (wPosition.a <= 0)
	{
		return float4(1, 1, 1, 1);
	}

	float3 position = mul(float4(wPosition.xyz,1), view).xyz;

	float4 outOfFocusColor = outOfFocusTex.Sample(samplerState, texCoord);
	float3 focusPoint;

	if (autoFocusEnabled && autoFocusSamples > 0)
	{
		float width;
		float heigth;
		positionTex.GetDimensions(width, heigth);

		float2 texelSize = 1.0 / float2(width, heigth);

		float4 result = 0.0;
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
				float4 pos = positionTex.Sample(samplerState, focusCenter + offset);
				result += pos;
				count++;
#if DEBUG
				if (pos.w > maxZ)
					maxZ = pos.w;
				float2 diff = input.Tex - (focusCenter + offset);
				if (diff.x < 2.5f * texelSize.x && diff.y < 2.5f * texelSize.y && diff.x > -2.5f * texelSize.x && diff.y > -2.5f * texelSize.y)
				{
					isFocus = true;
					currentZ = pos.w;
				}
#endif
			}
		}

		result /= count;
		focusPoint = mul(float4(result.xyz, 1), view).xyz;

#if DEBUG
		if (isFocus)
		{
			return float4(0,currentZ / maxZ,0,1);
		}
#endif
	}
	else
	{
		focusPoint = mul(float4(positionTex.Sample(samplerState, focusCenter).xyz, 1), view).xyz;
	}

	float blur = smoothstep(minDistance, maxDistance, length(position - focusPoint));

	float4 color = lerp(focusColor, outOfFocusColor, blur);

	return color;
}