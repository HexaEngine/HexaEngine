Texture2D hdrTexture : register(t0);
Texture2D bloomTexture : register(t1);
SamplerState state;

#define FXAA

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer Params
{
	float bloomStrength;
	float3 padd;
};

float3 ACESFilm(float3 x)
{
	return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0);
}

float3 OECF_sRGBFast(float3 color)
{
	float gamma = 2.2;
	return pow(color.rgb, float3(1.0 / gamma, 1.0 / gamma, 1.0 / gamma));
}

float3 BloomMix(float2 texCoord, float3 hdr)
{
	float3 blm = bloomTexture.Sample(state, texCoord).rgb;
	float3 drt = float3(0, 0, 0);
	return lerp(hdr, blm + blm * drt, float3(bloomStrength, bloomStrength, bloomStrength));
}

float4 main(VSOut vs) : SV_Target
{
	float4 color = hdrTexture.Sample(state, vs.Tex);
	color.rgb = BloomMix(vs.Tex, color.rgb);
	color.rgb = ACESFilm(color.rgb);
	color.rgb = OECF_sRGBFast(color.rgb);
#ifdef FXAA
	color.a = dot(color.rgb, float3(0.299, 0.587, 0.114));
#endif
	return color;
}