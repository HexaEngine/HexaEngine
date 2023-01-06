struct SelectionData
{
	uint InstanceId;
	uint TypeId;
};

cbuffer MouseBuffer
{
	float2 Position;
	float2 Padding;
};

Texture2D<uint4> idTexture : register(t0);
RWStructuredBuffer<SelectionData> output : register(u0);

[numthreads(1, 1, 1)]
void main(uint groupIndex : SV_GroupIndex)
{
	uint4 inData = idTexture.Load(int3((int2)Position, 0));
	SelectionData outData;
	outData.InstanceId = inData.x;
	outData.TypeId = inData.y;
	output[0] = outData;
}