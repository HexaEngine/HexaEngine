cbuffer DownsampleParams : register(b0)
{
    uint uGhosts;
    float uGhostDispersal;
    float2 textureSize;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D inputTexture : register(t0);
Texture1D lensColorTexture : register(t1);
SamplerState linearClampSampler : register(s0);

float4 main(VSOut pin) : SV_Target
{

    float2 texcoord = -pin.Tex + 1.0;
    float2 texelSize = 1.0 / textureSize;

    // ghost vector to image centre:
    float2 ghostVec = (0.5 - texcoord) * uGhostDispersal;

    // sample ghosts:
    float4 result = 0.0;
    for (uint i = 0; i < uGhosts; ++i)
    {
        float2 offset = frac(texcoord + ghostVec * float(i));

        float weight = length(0.5.xx - offset) / length(0.5.xx);
        weight = pow(1.0 - weight, 10.0);

        result += inputTexture.SampleLevel(linearClampSampler, offset, 0) * weight;
    }

    result *= lensColorTexture.SampleLevel(linearClampSampler, length(0.5.xx - texcoord) / length((0.5.xx)), 0);

    return result;
}