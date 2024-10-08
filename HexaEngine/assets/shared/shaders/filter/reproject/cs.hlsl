RWTexture2DArray<float4> inputTex;

cbuffer CBParams
{
	float4x4 prevViewProjInv;
	float4x4 viewProj;
	float2 texelSize;
	bool vsm;
}

// reprojects a depth buffer.
[numthreads(32, 32, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
	float depth = inputTex[int3(id.xy, 0)].r;

	float2 uv = id.xy * texelSize;

	float4 ndc = float4(uv * 2.0f - 1.0f, depth, 1.0f);
	ndc.y *= -1;
	float4 wp = mul(ndc, prevViewProjInv);

	float3 positionWS = wp.xyz / wp.w;
	float4 position = mul(float4(positionWS, 1), viewProj);
	float depthNew = position.z / position.w;

	float4 output = 0;
	output.r = depthNew;
	if (vsm)
	{
		output.g = output.r * output.r;
	}

	inputTex[int3(id.xy, 0)] = output;
}