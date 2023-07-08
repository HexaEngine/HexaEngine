struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer MaskBuffer
{
    float4 colorMask;
};

float4 main(VSOut input) : SV_TARGET
{
    return colorMask;
}