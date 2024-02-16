struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer MaskBuffer
{
    float4 colorMask;
    float2 location;
    float range;
    float fadeStart;
};

Texture2D maskTex;
SamplerState samplerLinearClamp;

float4 main(VSOut input) : SV_TARGET
{
    float2 uv = input.Tex;

    float2 center = uv - location;

    float distance = sqrt(center.x * center.x + center.y * center.y);

    if (distance > range)
        discard;

    float4 color = maskTex.SampleLevel(samplerLinearClamp, uv, 0);

    float edgeFade = saturate(1 - distance / range);

    // Increase each channel of the colour by the corresponding value of the mask and decrease it by the inverse value of the mask
    float4 newColor = color + colorMask;

    newColor = lerp(color, newColor, edgeFade);

    // Make sure that the colour values remain in the range from 0 to 1
    return clamp(newColor, 0, 1);

    return color;
}