#include "defs.hlsl"

#ifndef BaseColor0
#define BaseColor0 float3(0.8,0.8,0.8)
#endif
#ifndef BaseColor1
#define BaseColor1 float3(0.8,0.8,0.8)
#endif
#ifndef BaseColor2
#define BaseColor2 float3(0.8,0.8,0.8)
#endif
#ifndef BaseColor3
#define BaseColor3 float3(0.8,0.8,0.8)
#endif

Texture2D maskTex : register(t0);
SamplerState maskSamplerState : register(s0);

#if HasBaseColorTex0
Texture2D baseColorTexture0;
SamplerState baseColorTextureSampler0;
#endif
#if HasBaseColorTex1
Texture2D baseColorTexture1;
SamplerState baseColorTextureSampler1;
#endif
#if HasBaseColorTex2
Texture2D baseColorTexture2;
SamplerState baseColorTextureSampler2;
#endif
#if HasBaseColorTex3
Texture2D baseColorTexture3;
SamplerState baseColorTextureSampler3;
#endif

struct Pixel
{
    float4 Color : SV_Target0;
};

Pixel main(PixelInput input)
{
    float3 baseColor0 = BaseColor0;
    float3 baseColor1 = BaseColor1;
    float3 baseColor2 = BaseColor2;
    float3 baseColor3 = BaseColor3;

#if HasBaseColorTex0
    float4 color0 = baseColorTexture0.Sample(baseColorTextureSampler0, float2(input.tex.xy));
    baseColor0 = color0.rgb * color0.a;
#endif
#if HasBaseColorTex1
    float4 color1 = baseColorTexture1.Sample(baseColorTextureSampler1, float2(input.tex.xy));
    baseColor1 = color1.rgb * color1.a;
#endif
#if HasBaseColorTex2
    float4 color2 = baseColorTexture2.Sample(baseColorTextureSampler2, float2(input.tex.xy));
    baseColor2 = color2.rgb * color2.a;
#endif
#if HasBaseColorTex3
    float4 color3 = baseColorTexture3.Sample(baseColorTextureSampler3, float2(input.tex.xy));
    baseColor3 = color3.rgb * color3.a;
#endif

    float4 mask = maskTex.Sample(maskSamplerState, input.ctex).xyzw;

    float opacity = mask.x + mask.y + mask.z + mask.w;

    int matID = -1;

    float3 baseColor = 0;
    baseColor = Mask(baseColor, baseColor0, mask.x);
    baseColor = Mask(baseColor, baseColor1, mask.y);
    baseColor = Mask(baseColor, baseColor2, mask.z);
    baseColor = Mask(baseColor, baseColor3, mask.w);

    if (opacity < 0.1f)
        discard;

    Pixel output;
    output.Color = float4(baseColor, opacity);

    return output;
}