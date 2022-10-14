TextureCube SkyMap : register(t0);
SamplerState SkyMapSampler : register(s0);

struct PixelInputType
{
	float4 position : SV_POSITION;
	float3 tex : TEXCOORD0;
};

float4 main(PixelInputType input) : SV_Target
{
	return SkyMap.Sample(SkyMapSampler, input.tex);
}