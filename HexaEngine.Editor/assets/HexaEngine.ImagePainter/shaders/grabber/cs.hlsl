cbuffer PositionBuffer : register(b0)
{
	float2 mousePos;
};

Texture2D<float4> tex : register(t0);
RWStructuredBuffer<float4> output : register(u0);

[numthreads(1, 1, 1)]
void main(uint groupIndex : SV_GroupIndex)
{
	output[0] = tex.Load(int3((int2)mousePos, 0));
}