Texture2D inputTex : register(t0);
SamplerState linearClampSampler : register(s0);

cbuffer Params
{
	float4x4 prevViewProjInv;
	float4x4 viewProj;
	bool vsm;
}
struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

// reprojects a depth buffer.
float4 main(VSOut pin) : SV_TARGET
{
	float depth = inputTex.SampleLevel(linearClampSampler, pin.Tex, 0).r;

	float4 ndc = float4(pin.Tex * 2.0f - 1.0f, depth, 1.0f);
	ndc.y *= -1;
	float4 wp = mul(ndc, prevViewProjInv);

	float3 positionWS = wp.xyz / wp.w;
	float4 position = mul(float4(positionWS, 1), viewProj);
	float depthNew = position.z / position.w;

	float4 output = 0;
	output.r = depthNew;
	if (vsm)
	{
		output.g = depthNew * depthNew;
	}
	return output;
}