#include "../../camera.hlsl"

Texture2D hdrTexture : register(t0);
Texture2D<float> depthTexture : register(t1);
SamplerState linearClampSampler : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer Params
{
	float FogIntensity;
	float FogStart;
	float FogEnd;
	float padd0;

	float3 FogColor;
	float padd1;
};

inline float ComputeFogFactor(float d)
{
	//d is the distance to the geometry sampling from the camera
	//this simply returns a value that interpolates from 0 to 1
	//with 0 starting at FogStart and 1 at FogEnd
	return clamp((d - FogStart) / (FogEnd - FogStart), 0, 1) * FogIntensity;
}

float4 main(VSOut vs) : SV_Target
{
	float3 color = hdrTexture.Sample(linearClampSampler, vs.Tex).rgb;

	float depth = depthTexture.Sample(linearClampSampler, vs.Tex);
	float3 position = GetPositionWS(vs.Tex, depth);

	float d = distance(position, GetCameraPos());
	float factor = ComputeFogFactor(d);

	return float4(lerp(color, FogColor, factor), 1);
}