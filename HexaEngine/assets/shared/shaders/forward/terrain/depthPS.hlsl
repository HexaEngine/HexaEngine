Texture2D maskTex : register(t0);

SamplerState linearClampSampler : register(s0);

struct PixelInput
{
    float4 position : SV_POSITION;
    float2 ctex : TEXCOORD1;
};

void main(PixelInput input)
{
    float4 mask = maskTex.Sample(linearClampSampler, input.ctex).xyzw;
    float opacity = mask.x + mask.y + mask.z + mask.w;
    if (opacity < 0.1f)
        discard;
}