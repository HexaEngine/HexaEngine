TextureCube cubeMap : register(t0);
SamplerState linearWrapSampler : register(s0);

struct PixelInputType
{
    float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
};

float4 main(PixelInputType input) : SV_Target
{
    return cubeMap.Sample(linearWrapSampler, input.tex);
}