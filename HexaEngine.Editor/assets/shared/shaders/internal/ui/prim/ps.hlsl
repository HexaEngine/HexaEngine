cbuffer CBSolidColorBrush
{
	float4 color;
	float4 primDimensions;
	uint brushType;
}

Texture2D brushTex : register(t2);
SamplerState fontSampler : register(s0);

struct PSIn
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

float4 main(PSIn input) : SV_TARGET
{
	float2 primDimRange = (primDimensions.zw - primDimensions.xy);
	float2 uv = float2(saturate((input.position.x - primDimensions.x) / (primDimRange.x)), saturate((input.position.y - primDimensions.y) / (primDimRange.y)));
	switch (brushType)
	{
		case 0:
			return color;
		case 1:
			return color * brushTex.Sample(fontSampler, uv);
	}

	float4 startColor = float4(1.0f, 0.0f, 0.0f, 1.0f);

	float4 endColor = float4(0.0f, 0.0f, 1.0f, 1.0f);

	float t = (uv.y + uv.x) / 2;
	float4 gradientColor = lerp(startColor, endColor, t);

	return gradientColor;

	return color;
}