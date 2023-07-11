struct BokehGSOutput
{
    float4 PositionCS : SV_Position;
    float2 TexCoord : TEXCOORD;
    float3 Color : COLOR;
    float Depth : DEPTH;
};

float3 hexablur(sampler2D tex, vec2 uv)
{
    float2 scale = 1.0 / iResolution.xy;
    float3 col = 0.0;
    float asum = 0.0;
    float coc = texture(tex, uv).a;
    for (float t = 0.0; t < 8.0 * 2.0 * 3.14; t += 3.14 / 32.0)
    {
        float r = cos(3.14 / 6.0) / cos(fmod(t, 2.0 * 3.14 / 6.0) - 3.14 / 6.0);

        // Tap filter once for coc
        float2 offset = float2(sin(t), cos(t)) * r * t * scale * coc;
        float4 samp = texture(tex, uv + offset * 1.0);

        // Tap filter with coc from texture
        offset = float2(sin(t), cos(t)) * r * t * scale * samp.a;
        samp = texture(tex, uv + offset * 1.0);

        // weigh and save
        col += samp.rgb * samp.a * t;
        asum += samp.a * t;

    }
    col = col / asum;
    return (col);
}

float4 main(BokehGSOutput input) : SV_TARGET
{
    float bokehFactor = BokehTexture.Sample(LinearWrapSampler, input.TexCoord).r;

    return float4(input.Color * bokehFactor, 1.0f);
}