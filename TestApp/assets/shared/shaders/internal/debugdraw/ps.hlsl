struct PS_INPUT
{
    float4 position : SV_POSITION;
    float2 tex : TEXCOORD;
    float4 color : COLOR;
};

float4 main(PS_INPUT pixel) : SV_TARGET
{
	return pixel.color;
}