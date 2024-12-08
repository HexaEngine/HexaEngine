struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer CBBrush
{
    float3 location;
    float radius;
    float edgeFadeStart;
    float edgeFadeEnd;
};

cbuffer MaskBuffer
{
    float4 colorMask;
};

Texture2D maskTex;
SamplerState samplerLinearClamp;

float4 main(VSOut input) : SV_TARGET
{
    float2 uv = input.Tex;

    float2 center = uv - location.xz;

    float distance = sqrt(abs(center.x * center.x + center.y * center.y));

    if (distance > radius)
        discard;

    float4 color = maskTex.SampleLevel(samplerLinearClamp, uv, 0);

    float edgeFade = saturate((edgeFadeEnd - distance) / (edgeFadeEnd - edgeFadeStart));

    // Increase each channel of the color by the corresponding value of the mask and decrease it by the inverse value of the mask
    float4 newColor = color + colorMask;

    newColor = lerp(color, newColor, edgeFade);

    // Make sure that the color values remain in the range from 0 to 1
    return saturate(newColor);
}