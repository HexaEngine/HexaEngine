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
	float2 focusPointTex;
	bool4 enabled;
};

float4 main(VSOut input) : SV_TARGET
{
	float2 texCoord = input.Tex;
	float4 focusColor = focusTex.Sample(samplerState, texCoord);
	if (!enabled.x)
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
	float4 wFocusPoint = positionTex.Sample(samplerState, focusPointTex);
	float3 focusPoint = mul(float4(wFocusPoint.xyz,1), view).xyz;

	float blur = smoothstep(minDistance, maxDistance, length(position - focusPoint));

	float4 color = lerp(focusColor, outOfFocusColor, blur);

	return color;
}