RWTexture2DArray<float4> inputTex;

cbuffer CBParams
{
	uint slice;
}

[numthreads(32, 32, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
	[branch]
		if ((slice & (1 << id.z)) != 0)
		{
			inputTex[int3(id.xy, id.z)] = 0;
		}
}