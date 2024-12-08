struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

#ifndef LUT_TileSizeXY
#define LUT_TileSizeXY 32
#endif
#ifndef LUT_TileAmount
#define LUT_TileAmount 32
#endif

cbuffer LUTParams
{
	float LUTAmountChroma;
	float LUTAmountLuma;
};

Texture2D input : register(t0);
Texture2D lut : register(t1);

SamplerState samplerState;
SamplerState samplerLUT;

void main(float4 vpos : SV_Position, float2 texcoord : TEXCOORD, out float4 res : SV_Target0)
{
	float4 color = input.SampleLevel(samplerState, texcoord.xy, 0);
	float2 texelsize = 1.0 / LUT_TileSizeXY;
	texelsize.x /= LUT_TileAmount;

	float3 lutcoord = float3((color.xy * LUT_TileSizeXY - color.xy + 0.5) * texelsize.xy, color.z * LUT_TileSizeXY - color.z);
	float lerpfact = frac(lutcoord.z);
	lutcoord.x += (lutcoord.z - lerpfact) * texelsize.y;

	float3 lutcolor = lerp(lut.SampleLevel(samplerLUT, float2(lutcoord.x, lutcoord.y), 0).xyz, lut.SampleLevel(samplerLUT, float2(lutcoord.x + texelsize.y, lutcoord.y), 0).xyz, lerpfact);

	color.xyz = lerp(normalize(color.xyz), normalize(lutcolor.xyz), LUTAmountChroma) * lerp(length(color.xyz), length(lutcolor.xyz), LUTAmountLuma);

	res.xyz = color.xyz;
	res.w = 1.0;
}