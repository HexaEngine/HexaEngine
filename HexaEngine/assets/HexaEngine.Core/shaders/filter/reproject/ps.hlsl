Texture2D inputTex : register(t0);
SamplerState linearClampSampler : register(s0);

cbuffer Params
{
	float2 texelShift; // shift (in texels) of a fixed world point from the old cascade frame to the new one
	float2 texelSize;
	float depthDelta; // constant depth offset from the old cascade frame to the new one
	uint vsm;
	float2 padding;
}
struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

// reprojects a cascade slice by a constant per-cascade texel shift + depth offset, valid because
// an orthographic cascade only translates (never rotates/rescales) relative to the light between updates.
float4 main(VSOut pin) : SV_TARGET
{
	float2 srcUV = pin.Tex - texelShift * texelSize;
	float2 moments = inputTex.SampleLevel(linearClampSampler, srcUV, 0).rg;
	float depth = moments.r;
	float depthNew = depth + depthDelta;

	float4 output = 0;
	output.r = depthNew;
	if (vsm)
	{
		// shift the existing (blurred) moment instead of collapsing variance to zero: E[(d+delta)^2] = E[d^2] + 2*delta*E[d] + delta^2
		output.g = moments.g + depthDelta * (2.0f * depth + depthDelta);
	}
	return output;
}