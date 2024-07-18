Texture2D sourceTex;

#if SAMPLED
SamplerState samplerState;
#endif

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};
#if SAMPLED
float4 main(VSOut input) : SV_TARGET
{
    return sourceTex.SampleLevel(samplerState, input.Tex, 0);
}
#else
float4 main(VSOut input) : SV_TARGET
{
    float2 texSize = float2(0, 0);
    sourceTex.GetDimensions(texSize.x, texSize.y);
    return sourceTex.Load(int3((int2) (input.Tex * texSize), 0));
}
#endif