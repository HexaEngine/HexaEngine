struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

Texture2D srcTexture : register(t0);
SamplerState linearClampSampler;

float4 main(VSOut input) : SV_Target
{
	float2 texCoord = input.Tex;
	
    float2 srcResolution;
    srcTexture.GetDimensions(srcResolution.x, srcResolution.y);

	float2 srcTexelSize = 1.0 / srcResolution;
	float x = srcTexelSize.x;
	float y = srcTexelSize.y;

	// Take 13 samples around current texel:
	// a - b - c
	// - j - k -
	// d - e - f
	// - l - m -
	// g - h - i
	// === ('e' is the current texel) ===

	float3 a = srcTexture.Sample(linearClampSampler, float2(texCoord.x - 2 * x, texCoord.y + 2 * y)).rgb;
	float3 b = srcTexture.Sample(linearClampSampler, float2(texCoord.x, texCoord.y + 2 * y)).rgb;
	float3 c = srcTexture.Sample(linearClampSampler, float2(texCoord.x + 2 * x, texCoord.y + 2 * y)).rgb;

	float3 d = srcTexture.Sample(linearClampSampler, float2(texCoord.x - 2 * x, texCoord.y)).rgb;
	float3 e = srcTexture.Sample(linearClampSampler, float2(texCoord.x, texCoord.y)).rgb;
	float3 f = srcTexture.Sample(linearClampSampler, float2(texCoord.x + 2 * x, texCoord.y)).rgb;

	float3 g = srcTexture.Sample(linearClampSampler, float2(texCoord.x - 2 * x, texCoord.y - 2 * y)).rgb;
	float3 h = srcTexture.Sample(linearClampSampler, float2(texCoord.x, texCoord.y - 2 * y)).rgb;
	float3 i = srcTexture.Sample(linearClampSampler, float2(texCoord.x + 2 * x, texCoord.y - 2 * y)).rgb;

	float3 j = srcTexture.Sample(linearClampSampler, float2(texCoord.x - x, texCoord.y + y)).rgb;
	float3 k = srcTexture.Sample(linearClampSampler, float2(texCoord.x + x, texCoord.y + y)).rgb;
	float3 l = srcTexture.Sample(linearClampSampler, float2(texCoord.x - x, texCoord.y - y)).rgb;
	float3 m = srcTexture.Sample(linearClampSampler, float2(texCoord.x + x, texCoord.y - y)).rgb;

	// Apply weighted distribution:
	// 0.5 + 0.125 + 0.125 + 0.125 + 0.125 = 1
	// a,b,d,e * 0.125
	// b,c,e,f * 0.125
	// d,e,g,h * 0.125
	// e,f,h,i * 0.125
	// j,k,l,m * 0.5
	// This shows 5 square areas that are being sampled. But some of them overlap,
	// so to have an energy preserving downsample we need to make some adjustments.
	// The weights are the distributed, so that the sum of j,k,l,m (e.g.)
	// contribute 0.5 to the final color output. The code below is written
	// to effectively yield this sum. We get:
	// 0.125*5 + 0.03125*4 + 0.0625*4 = 1
	float3 downsample = e * 0.125;
	downsample += (a + c + g + i) * 0.03125;
	downsample += (b + d + f + h) * 0.0625;
	downsample += (j + k + l + m) * 0.125;

	downsample = max(downsample, 0.0001f);

	return float4(downsample, 1);
}