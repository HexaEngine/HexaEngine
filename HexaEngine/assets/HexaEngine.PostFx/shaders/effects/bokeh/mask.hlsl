struct BokehGSOutput
{
    float4 PositionCS   : SV_Position;
    float2 TexCoord     : TEXCOORD;
    float3 Color        : COLOR;
    float Depth         : DEPTH;
};

Texture2D bokehTexture         : register(t0);
SamplerState linearWrapSampler : register(s0);

float4 main(BokehGSOutput input) : SV_TARGET
{
    float bokehFactor = bokehTexture.Sample(linearWrapSampler, input.TexCoord).r;
    
    return float4(input.Color * bokehFactor, 1.0f);
}