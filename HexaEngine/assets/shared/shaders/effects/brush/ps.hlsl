Texture2D maskTex;
SamplerState state;

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer BrushBuffer
{
    float4 color;
};

float4 main(VSOut input) : SV_TARGET
{	  
    float4 mask = maskTex.Sample(state, input.Tex);
    return color * mask;
}