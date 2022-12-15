struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

Texture2D baseTexture : register(t0);
Texture2D textureA : register(t1);
Texture2D textureB : register(t2);
Texture2D textureX : register(t3);

SamplerState samplerState;

cbuffer Params
{
	bool enabled;
	float3 padd;
};

float4 main(VSOut input) : SV_Target
{
	float2 texCoord = input.Tex;
	float4 color = baseTexture.Sample(samplerState, texCoord);
	if (!enabled)
	{
		return color;
	}

	float4 a = textureA.Sample(samplerState, texCoord);
	float4 b = textureB.Sample(samplerState, texCoord);
	float x = textureX.Sample(samplerState, texCoord).w;

	float4 mix = lerp(a, b, x * 5 / 2);

	return color + mix;
}