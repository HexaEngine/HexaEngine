#include "defs.hlsl"
#include "../../gbuffer.hlsl"
#include "../../material.hlsl"

#if (DEPTH != 1)
Texture2D albedoTexture : register(t0);
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
    float3 albedo = material.Color.rgb;
	float3 pos = (float3)input.pos;
    float3 normal = input.normal;
    float3 emissive = material.Emissive;
	float opacity = 1;

    float ao = material.Ao;
    float specular = material.Specular;
    float specularTint = material.SpecularTint;
    float sheen = material.Sheen;
    float sheenTint = material.SheenTint;
    float clearcoat = material.Clearcoat;
    float clearcoatGloss = material.ClearcoatGloss;
    float anisotropic = material.Anisotropic;
    float subsurface = material.Subsurface;
    float roughness = material.Roughness;
    float metalness = material.Metalness;

	if (material.DANR.y)
	{
		float4 color = albedoTexture.Sample(materialSamplerState, (float2) input.tex);
		albedo = color.rgb * color.a;
		opacity = color.a;
	}
	if (material.DANR.z)
	{
		normal = NormalSampleToWorldSpace(normalTexture.Sample(materialSamplerState, (float2) input.tex).rgb, input.normal, input.tangent);
	}
	if (material.DANR.w)
	{
		roughness = roughnessTexture.Sample(materialSamplerState, (float2) input.tex).r;
	}
	if (material.MEAoRM.x)
	{
		metalness = metalnessTexture.Sample(materialSamplerState, (float2) input.tex).r;
	}
	if (material.MEAoRM.y)
	{
		emissive = emissiveTexture.Sample(materialSamplerState, (float2) input.tex).rgb;
	}
	if (material.MEAoRM.z)
	{
		ao = aoTexture.Sample(materialSamplerState, (float2) input.tex).r;
	}

	if (material.MEAoRM.w)
	{
		roughness = rmTexture.Sample(materialSamplerState, (float2) input.tex).g;
		metalness = rmTexture.Sample(materialSamplerState, (float2) input.tex).b;
	}

	if (opacity == 0)
		discard;

	return PackGeometryData(albedo, opacity, pos, input.depth, normal, roughness, metalness, input.tangent, emissive, 0, specular, specularTint, ao, 1, anisotropic, 0, clearcoat, clearcoatGloss, 0, 0, sheen, sheenTint);
}

#endif