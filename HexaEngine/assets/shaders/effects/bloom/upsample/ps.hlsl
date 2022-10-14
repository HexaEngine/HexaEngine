struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

// This shader performs upsampling on a texture,
// as taken from Call Of Duty method, presented at ACM Siggraph 2014.

// Remember to add bilinear minification filter for this texture!
// Remember to use a floating-point texture format (for HDR)!
// Remember to use edge clamping for this texture!
Texture2D srcTexture : register(t0);
SamplerState samplerState;

cbuffer Params
{
	float filterRadius;
	float3 padd;
};

float4 main(VSOut input) : SV_Target
{
	float2 texCoord = input.Tex;

	// The filter kernel is applied with a radius, specified in texture
	// coordinates, so that the radius will vary across mip resolutions.
	float x = filterRadius;
	float y = filterRadius;

	// Take 9 samples around current texel:
	// a - b - c
	// d - e - f
	// g - h - i
	// === ('e' is the current texel) ===
	float3 a = srcTexture.Sample(samplerState, float2(texCoord.x - x, texCoord.y + y)).rgb;
	float3 b = srcTexture.Sample(samplerState, float2(texCoord.x, texCoord.y + y)).rgb;
	float3 c = srcTexture.Sample(samplerState, float2(texCoord.x + x, texCoord.y + y)).rgb;

	float3 d = srcTexture.Sample(samplerState, float2(texCoord.x - x, texCoord.y)).rgb;
	float3 e = srcTexture.Sample(samplerState, float2(texCoord.x, texCoord.y)).rgb;
	float3 f = srcTexture.Sample(samplerState, float2(texCoord.x + x, texCoord.y)).rgb;

	float3 g = srcTexture.Sample(samplerState, float2(texCoord.x - x, texCoord.y - y)).rgb;
	float3 h = srcTexture.Sample(samplerState, float2(texCoord.x, texCoord.y - y)).rgb;
	float3 i = srcTexture.Sample(samplerState, float2(texCoord.x + x, texCoord.y - y)).rgb;

	// Apply weighted distribution, by using a 3x3 tent filter:
	//  1   | 1 2 1 |
	// -- * | 2 4 2 |
	// 16   | 1 2 1 |
	float3 upsample = e * 4.0;
	upsample += (b + d + f + h) * 2.0;
	upsample += (a + c + g + i);
	upsample *= 1.0 / 16.0;

	return float4(upsample, 1);
}