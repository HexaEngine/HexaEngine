#include "defs.hlsl"
#include "../../camera.hlsl"

#ifndef BaseColor
#define BaseColor float4(0.8,0.8,0.8,1)
#endif

cbuffer overlay
{
	bool ShowWeights;
	int WeightMask;
	float2 pad;
};

float4 main(PixelInput input, uint primitiveId : SV_PrimitiveID) : SV_Target
{
	float4 baseColor = BaseColor;

	float amient = baseColor.rgb * 0.2;

	float3 L = normalize(GetCameraPos() - input.pos.xyz);
	float3 N = input.normal;
	float diffuse = dot(L, N);

	float3 shadeColor = baseColor.rgb * diffuse + amient;

	if (ShowWeights)
	{
#if VtxSkinned
		return float4(input.weightColor, 1);
#else
		return float4(shadeColor, 1);
#endif
	}
	else
	{
		return float4(shadeColor, 1);
	}
}