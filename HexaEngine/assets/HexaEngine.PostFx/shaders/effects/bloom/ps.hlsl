Texture2D hdrTexture : register(t0);
Texture2D bloomTexture : register(t1);
Texture2D lensDirtTexture : register(t2);

SamplerState linearClampSampler : register(s0);

cbuffer CBBloom
{
	float BloomIntensity;
	float BloomThreshold;
	float LensDirtIntensity;
}

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
	float3 color = hdrTexture.Sample(linearClampSampler, vs.Tex).rgb;

	float luminance = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));

	if (luminance > BloomThreshold)
	{
		float3 bloom = bloomTexture.Sample(linearClampSampler, vs.Tex).rgb;
#if LensDirtTex
		float3 dirt = lensDirtTexture.Sample(linearClampSampler, vs.Tex).rgb * LensDirtIntensity;
#else
		float3 dirt = float3(0, 0, 0);
#endif

		color = lerp(color, bloom + bloom * dirt, BloomIntensity.xxx);
	}

	return float4(color, 1);
}