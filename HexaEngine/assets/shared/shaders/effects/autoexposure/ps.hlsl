Texture2D hdrTexture : register(t0);
Texture2D<float> lumaTexture : register(t1);

SamplerState linearClampSampler : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
	float avgLum = lumaTexture.Load(0);
	float keyValue = 1.03 - (2.0 / (2.0 + log2(avgLum + 1.0)));
	float exposure = keyValue / avgLum;

	float3 color = hdrTexture.Sample(linearClampSampler, vs.Tex).rgb * exposure;

	return float4(color, 1);
}