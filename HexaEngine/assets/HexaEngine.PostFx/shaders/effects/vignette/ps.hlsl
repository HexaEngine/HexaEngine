Texture2D inputTex : register(t0);
SamplerState linearClampSampler : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer VignetteParams
{
	float VignetteIntensity;
	float VignetteRatio;
	float VignetteRadius;
	float VignetteSlope;

	float2 VignetteCenter;
	float2 padd2;

	float3 VignetteColor;
	float padd3;
};

float4 main(VSOut vs) : SV_Target
{
	float3 color = inputTex.Sample(linearClampSampler, vs.Tex).rgb;

	float2 texCoord = vs.Tex - VignetteCenter;

	texCoord.x /= VignetteRadius * VignetteRatio;
	texCoord.y /= VignetteRadius;

	float vignette = 1.0 - dot(texCoord, texCoord);

	float vignetteFactor = 1.0f - saturate(pow(vignette, VignetteSlope * 0.5)) * VignetteIntensity;

	return float4(VignetteColor, vignetteFactor);
}