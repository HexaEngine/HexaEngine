struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer params
{
    int size;
    float3 padd;
};

Texture2D tex;
SamplerState state;

float4 main(VSOut vs) : SV_Target
{
    float width;
    float heigth;
    tex.GetDimensions(width, heigth);
  
    float Pi = 6.28318530718; // Pi*2
    
    // GAUSSIAN BLUR SETTINGS {{{
    float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
    float Quality = 3.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
    float Size = 8.0; // BLUR SIZE (Radius)
    // GAUSSIAN BLUR SETTINGS }}}
   
    float2 Radius = Size / float2(width, heigth);
    
    // Normalized pixel coordinates (from 0 to 1)
    float2 uv = vs.Tex;
    
    // Pixel colour
    float4 Color = tex.Sample(state, uv);
    
    // Blur calculations
    for (float d = 0.0; d < Pi; d += Pi / Directions)
    {
        for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
        {
            Color += tex.Sample(state, uv + float2(cos(d), sin(d)) * Radius * i);
        }
    }
    
    // Output to screen
    return Color / Quality * Directions - 15.0;
}