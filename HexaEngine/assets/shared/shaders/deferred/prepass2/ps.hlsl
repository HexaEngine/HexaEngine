#include "defs.hlsl"
#include "../../gbuffer.hlsl"
#include "../../material.hlsl"

#if (DEPTH != 1)
Texture2D colorTexture : register(t0);
Texture2D normalTexture : register(t1);
Texture2D roughnessTexture : register(t2);
Texture2D metalnessTexture : register(t3);
Texture2D emissiveTexture : register(t4);
Texture2D aoTexture : register(t5);
Texture2D rmTexture : register(t6);

SamplerState materialSamplerState : register(s0);

cbuffer MaterialBuffer : register(b2)
{
	MaterialNG material;
};

float3 NormalSampleToWorldSpace(float3 normalMapSample, float3 unitNormalW, float3 tangentW)
{
	// Uncompress each component from [0,1] to [-1,1].
	float3 normalT = 2.0f * normalMapSample - 1.0f;

	// Build orthonormal basis.
	float3 N = unitNormalW;
	float3 T = normalize(tangentW - dot(tangentW, N) * N);
	float3 B = cross(N, T);

	float3x3 TBN = float3x3(T, B, N);

	// Transform from tangent space to world space.
	float3 bumpedNormalW = mul(normalT, TBN);

	return bumpedNormalW;
}

#endif

#if (DEPTH == 1)

float4 main(PixelInput input) : SV_Target
{
	return float4(input.depth, input.depth, input.depth, 1.0f);
}

#else

GeometryData main(PixelInput input)
{
	float3 color;
	float3 pos = (float3)input.pos;
	float3 normal;
	float3 emissive;
	float opacity = 1;
	float metalness;
	float roughness;
	float ao;

#if (BASE_COLOR_TEX)
	color = colorTexture.Sample(materialSamplerState, (float2) input.tex).rgb;
#else
	color = material.Color.rgb;
#endif

#if (NORMAL_TEX)
	normal = NormalSampleToWorldSpace(normalTexture.Sample(materialSamplerState, (float2) input.tex).rgb, input.normal, input.tangent);
#else
	normal = input.normal;
#endif

#if (ROUGHNESS_TEX)
	roughness = roughnessTexture.Sample(materialSamplerState, (float2) input.tex).r;
#else
	roughness = material.RoughnessMetalnessAo.x;
#endif

#if (METALNESS_TEX)
	metalness = metalnessTexture.Sample(materialSamplerState, (float2) input.tex).r;
#else
	metalness = material.RoughnessMetalnessAo.y;
#endif

#if (EMISSIVE_TEX)
	emissive = emissiveTexture.Sample(materialSamplerState, (float2) input.tex).rgb;
#else
	emissive = material.Emissive.xyz;
#endif

#if (AO_TEX)
	ao = aoTexture.Sample(materialSamplerState, (float2) input.tex).r;
#else
	ao = material.RoughnessMetalnessAo.z;
#endif

#if (ROUGHNESS_METALNESS_TEX)
	roughness = rmTexture.Sample(materialSamplerState, (float2) input.tex).g;
	metalness = rmTexture.Sample(materialSamplerState, (float2) input.tex).b;
#endif

	return PackGeometryData(color, opacity, pos, input.depth, normal, roughness, metalness, float3(0, 0, 0), float3(0, 0, 0), 0, 1, 0.5f, ao, 1, 0, 0, 0, 0, 0, 0, 0, 0);
}

#endif