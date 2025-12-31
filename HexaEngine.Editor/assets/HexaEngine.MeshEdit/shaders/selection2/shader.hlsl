struct SelectionData
{
    uint InstanceId;
    uint TypeId;
    uint PrimitiveId;
    uint VertexId;
};

cbuffer MouseBuffer
{
    float2 Position;
    float2 Size;
};

Texture2D<float4> idTexture : register(t0);
RWStructuredBuffer<SelectionData> output : register(u0);

[numthreads(1, 1, 1)]
void main(uint groupIndex : SV_GroupIndex)
{
    float4 inData = 0;
    for (uint x = 0; x < 32; x++)
    {
        for (uint y = 0; y < 32; y++)
        {
            inData = max(inData, idTexture.Load(int3((int2) Position + int2(x, y), 0)));
        }
    }

    
	
    SelectionData outData;
    outData.InstanceId = inData.x;
    outData.TypeId = inData.y;
    outData.PrimitiveId = inData.z;
    outData.VertexId = inData.w;
    output[0] = outData;
	
}