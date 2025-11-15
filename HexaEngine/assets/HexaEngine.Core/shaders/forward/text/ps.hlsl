Texture2D textAtlasTexture : register(t0);
SamplerState textAtlasSampler : register(s0);

cbuffer ColorBuffer
{
	float4 color;
};

struct PSInput
{
	float4 pos : SV_POSITION;
	float2 tex : TexCoord;
};

float4 main(PSInput pin) : SV_TARGET
{
	float4 textColor = textAtlasTexture.SampleLevel(textAtlasSampler, pin.tex, 0) * color;
	return textColor;
}