struct VertexInputType
{
	float3 position : POSITION;
    float4 color : COLOR;
};
struct PixelInputType
{
	float4 position : SV_POSITION;
    float4 color : COLOR;
};
cbuffer MVPBuffer
{
    matrix mvp;
};
PixelInputType main(VertexInputType input)
{
	PixelInputType output;
    output.position = mul(mvp, float4(input.position, 1));
    output.color = input.color;
	return output;
}