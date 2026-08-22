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
	float2 moments = inputTex[int3(id.xy, 0)].rg;
	float depth = moments.r;

	// +0.5 to sample the texel center, matching how the rasterizer produced this depth value.
	float2 uv = (id.xy + 0.5f) * texelSize;

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
		// shift the existing (blurred) moment instead of collapsing variance to zero: E[(d+delta)^2] = E[d^2] + 2*delta*E[d] + delta^2
		float delta = depthNew - depth;
		output.g = moments.g + delta * (2.0f * depth + delta);
	}

	inputTex[int3(id.xy, 0)] = output;
}