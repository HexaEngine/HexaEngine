Texture2D sourceTex;

#if SAMPLED
SamplerState samplerState;
#endif

#if SAMPLED
cbuffer CB
{
	float2 offset;
	float2 size;
};
#endif

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};
#if SAMPLED
float4 main(VSOut input) : SV_TARGET
{
	float2 texSize = float2(0, 0);
	sourceTex.GetDimensions(texSize.x, texSize.y);
	const float2 texScale = size / texSize;
	const float2 texOffset = offset / texSize;
	return sourceTex.SampleLevel(samplerState, texOffset + input.Tex * scale, 0);
}
#else
float4 main(VSOut input) : SV_TARGET
{
	float2 texSize = float2(0,0);
	sourceTex.GetDimensions(texSize.x, texSize.y);
	return sourceTex.Load(int3((int2)(input.Tex * texSize), 0));
}
#endif