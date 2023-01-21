struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

Texture2D baseTexture : register(t0);
Texture2D ssrtexture : register(t1);

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

	float4 ssr = ssrtexture.Sample(samplerState, texCoord);

	return color + ssr;
}