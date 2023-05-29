Texture2D hdrTexture : register(t0);
Texture2D lumaTexture : register(t1);
SamplerState state;

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
	float4 color = hdrTexture.Sample(state, vs.Tex);

	float avgLum = lumaTexture.Sample(state, vs.Tex).r;
	float keyValue = 1.03 - (2.0 / (2.0 + log2(avgLum + 1.0)));
	float exposure = keyValue / avgLum;

	color.rgb *= exposure;
	return color;
}