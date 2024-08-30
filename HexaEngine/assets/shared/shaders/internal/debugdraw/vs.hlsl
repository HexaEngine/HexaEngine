struct VS_INPUT
{
	float3 position : POSITION;
    float2 tex : TEXCOORD;
	float4 color : COLOR;
};
struct PS_INPUT
{
	float4 position : SV_POSITION;
    float2 tex : TEXCOORD;
	float4 color : COLOR;
};
cbuffer matrixBuffer
{
    float4x4 ProjectionMatrix;
};
PS_INPUT main(VS_INPUT input)
{
    PS_INPUT output;
    output.position = mul(float4(input.position, 1), ProjectionMatrix);
    output.tex = input.tex;
	output.color = input.color;
	return output;
}