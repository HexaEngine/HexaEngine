struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer Params
{
	float g_Sharpness;
	float2 g_InvResolutionDirection;
	float padd;
};

Texture2D tex;
SamplerState state;

#ifndef AO_BLUR_PRESENT
#define AO_BLUR_PRESENT 1
#endif

#define KERNEL_RADIUS 3

float BlurFunction(float2 uv, float r, float center_c, float center_d, inout float w_total)
{
	float2  aoz = tex.Sample(state, uv).xy;
	float c = aoz.x;
	float d = 1;

	float BlurSigma = KERNEL_RADIUS * 0.5;
	float BlurFalloff = 1.0 / (2.0 * BlurSigma * BlurSigma);

	float ddiff = (d - center_d) * g_Sharpness;
	float w = exp2(-r * r * BlurFalloff - ddiff * ddiff);
	w_total += w;

	return c * w;
}

float4 main(VSOut vs) : SV_Target
{
	float2 texCoord = vs.Tex;
	float2 aoz = tex.Sample(state, texCoord).xy;
	float center_c = aoz.x;
	float center_d = 1;

	float c_total = center_c;
	float w_total = 1.0;

	for (float r = 1; r <= KERNEL_RADIUS; ++r)
	{
		float2 uv = texCoord + g_InvResolutionDirection * r;
		c_total += BlurFunction(uv, r, center_c, center_d, w_total);
	}

	for (float r = 1; r <= KERNEL_RADIUS; ++r)
	{
		float2 uv = texCoord - g_InvResolutionDirection * r;
		c_total += BlurFunction(uv, r, center_c, center_d, w_total);
	}

	#if AO_BLUR_PRESENT
	return float4(c_total / w_total, c_total / w_total, c_total / w_total, c_total / w_total);
	#else
	return float4(c_total / w_total, center_d, 0, 0);
	#endif
}