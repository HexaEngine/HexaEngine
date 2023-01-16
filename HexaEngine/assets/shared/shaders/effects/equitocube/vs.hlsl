cbuffer MatrixBuffer : register(b0)
{
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projectionMatrix;
};

struct VSOut
{
	float4 Pos : SV_POSITION;
	float3 WorldPos : TEXCOORD0;
};

VSOut main(float3 aPos : POSITION)
{
	VSOut outp;
	outp.WorldPos = aPos;
	outp.Pos = mul(float4(aPos, 1), worldMatrix);
	outp.Pos = mul(outp.Pos, viewMatrix);
	outp.Pos = mul(outp.Pos, projectionMatrix);
	return outp;
}