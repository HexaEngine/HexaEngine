#include "../material.hlsl"

////////////////////////////////////////////////////////////////////////////////
// Filename: deferred.ps
////////////////////////////////////////////////////////////////////////////////

//////////////
// TEXTURES //
//////////////
Texture2D ambientTexture : register(t0);
Texture2D diffuseTexture : register(t1);
Texture2D specularTexture : register(t2);
Texture2D specularHighlightTexture : register(t3);
Texture2D bumpTexture : register(t4);
Texture2D displacmentTexture : register(t5);
Texture2D stencilDecalTexture : register(t6);
Texture2D alphaTexture : register(t7);
Texture2D metallicTexture : register(t8);
Texture2D rougthnessTexture : register(t9);
TextureCube environmentTexture : register(t10);

cbuffer MaterialBuffer : register(b0)
{
	Material material;
};

cbuffer CamBuffer : register(b1)
{
	float3 viewPos;
	float reserved;
	matrix viewMatrix;
	matrix projectionMatrix;
};

///////////////////
// SAMPLE STATES //
///////////////////
SamplerState SampleTypeWrap : register(s0);

struct DomainOut
{
	float4 position : SV_POSITION;
	float4 pos : POSITION;
	float3 tex : TEXCOORD0;
	float3 normal : NORMAL;
};

struct PixelOutputType
{
	float4 color : SV_Target0;
	float4 position : SV_Target1;
	float4 normal : SV_Target2;
	float4 depth : SV_Target3;
};

////////////////////////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////////////////////////
PixelOutputType main(DomainOut input)
{
	PixelOutputType output;
	output.position = input.pos;

	if (material.HasDiffuseTextureMap)
	{
		output.color = diffuseTexture.Sample(SampleTypeWrap, (float2)input.tex);
	}
	else
	{
		output.color = float4(material.DiffuseColor, 1);
	}

	if (material.HasAlphaTextureMap)
	{
		output.color.w = alphaTexture.Sample(SampleTypeWrap, (float2)input.tex).r;
	}
	else
	{
		output.color.w = material.Transparency;
	}

	if (material.HasBumpMap)
	{
		output.normal = bumpTexture.Sample(SampleTypeWrap, (float2)input.tex) * float4(input.normal, 1.0f);
	}
	else
	{
		output.normal = float4(input.normal, 1.0f);
	}

	if (material.HasMetallicMap)
	{
		if (material.HasRoughnessMap)
		{
		}
	}

	float roughness = 0;
	roughness = rougthnessTexture.Sample(SampleTypeWrap, (float2)input.tex).r;
	float3 refvector = reflect(viewPos, (float3)output.normal);
	refvector = -refvector;
	float4 ref = environmentTexture.SampleLevel(SampleTypeWrap, refvector, roughness * 5);
	float metallic = 0;
	metallic = metallicTexture.Sample(SampleTypeWrap, (float2)input.tex).r;
	output.color = lerp(output.color, ref, metallic);

	float depth = input.position.z / input.position.w;
	output.depth = float4(depth, depth, depth, depth);
	return output;
}