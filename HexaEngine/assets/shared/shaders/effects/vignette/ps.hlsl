Texture2D hdrTexture : register(t0);
SamplerState linearClampSampler : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer Params
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
	float3 color = hdrTexture.Sample(linearClampSampler, vs.Tex).rgb;

	float2 texCoord = vs.Tex - VignetteCenter;

	texCoord *= float2((1.0 / VignetteRadius), VignetteRatio);

	float vignette = dot(texCoord, texCoord);

	float vignetteFactor = VignetteIntensity * (1.0 + pow(vignette, VignetteSlope * 0.5));

	color.rgb *= (1.0 - vignetteFactor * VignetteColor);

	return float4(color, 1);
}