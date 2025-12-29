Texture2D<uint> inputTex;

cbuffer OutlineParams 
{
    float2 texSize;
    float edgeScale; 
    float padding;
    float4 outlineColor;
    float4 fillColor;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float SampleR8Uint(int2 p)
{
    return (float) (inputTex.Load(int3(p, 0)));
}

float4 main(VSOut input) : SV_Target
{
    int2 pixel = int2(input.Tex * texSize);
    uint center = inputTex.Load(int3(pixel, 0));
    
    float tl = SampleR8Uint(pixel + int2(-1, -1));
    float tc = SampleR8Uint(pixel + int2(0, -1));
    float tr = SampleR8Uint(pixel + int2(1, -1));

    float ml = SampleR8Uint(pixel + int2(-1, 0));
    float mr = SampleR8Uint(pixel + int2(1, 0));

    float bl = SampleR8Uint(pixel + int2(-1, 1));
    float bc = SampleR8Uint(pixel + int2(0, 1));
    float br = SampleR8Uint(pixel + int2(1, 1));
    
    float gx =
        -tl - 2.0 * ml - bl +
         tr + 2.0 * mr + br;

    float gy =
        -tl - 2.0 * tc - tr +
         bl + 2.0 * bc + br;

    float edge = sqrt(gx * gx + gy * gy) * edgeScale;
    
    edge = saturate(edge);
    float invEdge = 1.0 - edge;
    
    float edgeStrength = smoothstep(0.1, 0.5, edge);

    float4 color = fillColor * center * invEdge + outlineColor * edgeStrength;
    color.rgb *= color.a;
    return color;
}
