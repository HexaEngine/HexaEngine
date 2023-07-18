TextureCube cubeMap : register(t0);
SamplerState linear_wrap_sampler : register(s0);

struct PixelInputType
{
	float4 position : SV_POSITION;
    float3 tex : POSITION;
};

float4 main(PixelInputType input) : SV_Target
{
    return cubeMap.Sample(linear_wrap_sampler, input.tex);
}