Texture2D inputTex;
Texture2D indirectTex;

SamplerState linearClampSampler;

struct VertexOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VertexOut input) : SV_Target
{
    float4 sceneColor = inputTex.Sample(linearClampSampler, input.Tex);
    float4 indirectColor = indirectTex.Sample(linearClampSampler, input.Tex);
    float4 finalColor = sceneColor + indirectColor;
    return finalColor;
}