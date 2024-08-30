struct VSIn
{
	float2 position : POSITION;
	float2 uv : TEXCOORD;
	uint bufferIndex : COLOR;
};

struct PSIn
{
	float4 position : SV_POSITION;
	float2 uv : TEXCOORD;
	uint bufferIndex : COLOR;
};

cbuffer matrixBuffer : register(b0)
{
	float4x4 view;
}

PSIn main(VSIn input)
{
	PSIn output;
	output.position = mul(float4(input.position, 0, 1), view);
	output.uv = input.uv;
	output.bufferIndex = input.bufferIndex;
	return output;
}