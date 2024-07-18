cbuffer Params : register(b0)
{
	float4 min;
	float4 range;
	float2 texSize;
};

Texture2D inputTex : register(t0);

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float4 main(VSOut pin) : SV_Target
{
	int2 coords = (int2)(pin.Tex * texSize);

	float4 input = inputTex.Load(int3(coords, 0));

	return min + input * range;
}