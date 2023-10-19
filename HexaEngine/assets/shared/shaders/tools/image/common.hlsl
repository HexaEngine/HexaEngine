Texture2D brushMaskTex : register(t0);
SamplerState brushMaskSamplerState : register(s0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer BrushBuffer
{
	float4 brushColor;
};

float4 GetMask(VSOut input)
{
	return brushMaskTex.Sample(brushMaskSamplerState, input.Tex);
}

float4 ApplyMask(float4 color, VSOut pin)
{
	return color * GetMask(pin);
}