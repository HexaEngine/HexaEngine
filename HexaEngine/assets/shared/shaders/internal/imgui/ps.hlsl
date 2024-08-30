struct PS_INPUT
{
	float4 pos : SV_POSITION;
	float4 col : COLOR0;
	float2 uv  : TEXCOORD0;
};

SamplerState fontSampler;
Texture2D fontTex;

float4 main(PS_INPUT input) : SV_Target
{
	float4 out_col = input.col * fontTex.Sample(fontSampler, input.uv);
	return out_col;
}