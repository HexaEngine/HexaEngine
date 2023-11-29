struct PS_INPUT
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD;
    float4 color : COLOR;
};

SamplerState samplerState;
Texture2D tex;

float4 main(PS_INPUT pixel) : SV_TARGET
{
    float4 color = pixel.color * tex.Sample(samplerState, pixel.tex);
    return color;
}