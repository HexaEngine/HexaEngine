cbuffer CBSolidColorBrush
{
	float4 color;
}

struct PSIn
{
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD;
	float4 color : COLOR;
};

Texture2D fontTex : register(t0);
SamplerState fontSampler : register(s0);

float4 main(PSIn input) : SV_TARGET
{
	float4 textureColor = fontTex.Sample(fontSampler, input.uv);
	return color * textureColor;
}