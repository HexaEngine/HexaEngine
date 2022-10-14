struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

Texture2D hdrTexture : register(t0);
Texture2D bloomTexture : register(t1);

SamplerState samplerState;

cbuffer Params
{
	float bloomStrength;
	float3 padd;
};

float4 main(VSOut input) : SV_Target
{
	float2 texCoord = input.Tex;
	float3 hdr = hdrTexture.Sample(samplerState, texCoord).rgb;
	float3 blm = bloomTexture.Sample(samplerState, texCoord).rgb;
	float3 drt = float3(0, 0, 0);
	float3 col = mix(hdr, blm + blm * drt, float3(bloomStrength, bloomStrength, bloomStrength));
	return col;
}