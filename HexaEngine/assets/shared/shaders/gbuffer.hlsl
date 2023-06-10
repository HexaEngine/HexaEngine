struct GeometryData
{
	float4 albedo : SV_TARGET0;
	float4 position : SV_TARGET1;
	float4 normal : SV_TARGET2;
	float4 tangent : SV_TARGET3;
	float4 emission : SV_TARGET4;
    float4 misc0 : SV_TARGET5;
    float4 misc1 : SV_TARGET6;
    float4 misc2 : SV_TARGET7;
};

struct GeometryAttributes
{
	float3 albedo;
	float opacity;
	float3 pos;
	float depth;
	float3 normal;
	float roughness;
	float metalness;
	float3 tangent;
	float3 emission;
	float emissionstrength;
	float specular;
	float speculartint;
	float ao;
	float ior;
	float anisotropic;
	float anisotropicrotation;
	float clearcoat;
    float clearcoatGloss;
	float subsurface;
	float sheen;
	float sheentint;
};

/*
albedo	    float3
pos		    float3
opacity		float
normal	    float3
roughness	float
metalness	float
emissive	float3
ao		    float

Tex.	float32	    float32	    float32	    float32
		albedo      albedo      albedo      opacity
		position	position	position	roughness
		normal	    normal	    normal	    metalness
		emissive	emissive	emissive	ao
*/

GeometryData PackGeometryData(
	in float3 albedo,
	in float opacity,
	in float3 pos,
	in float depth,
	in float3 normal,
	in float roughness,
	in float metalness,
	in float3 tangent,
	in float3 emission,
	in float emissionstrength,
	in float specular,
	in float speculartint,
	in float ao,
	in float ior,
	in float anisotropic,
	in float anisotropicrotation,
	in float clearcoat,
	in float clearcoatGloss,
	in float transmission,
	in float transmissionroughness,
	in float sheen,
	in float sheentint
)
{
	GeometryData data;
	data.albedo.rgb = albedo;
	data.albedo.a = opacity;
	data.position.xyz = pos.rgb;
	data.position.w = depth;
	data.normal.xyz = normal;
	data.normal.w = roughness;
	data.tangent.xyz = tangent;
    data.tangent.w = metalness;
	data.emission.xyz = emission;
	data.emission.w = emissionstrength;
	data.misc0.r = specular;
    data.misc0.g = speculartint;
    data.misc0.b = ao;
    data.misc0.a = ior;
    data.misc1.r = anisotropic;
    data.misc1.g = anisotropicrotation;
    data.misc1.b = clearcoat;
    data.misc1.a = clearcoatGloss;
    data.misc2.r = transmission;
    data.misc2.g = transmissionroughness;
    data.misc2.b = sheen;
    data.misc2.a = sheentint;
	return data;
}

void ExtractGeometryData(
	in float2 tex,
	in Texture2D albedoTex,
	in Texture2D positionTex,
	in Texture2D normalTex,
	in Texture2D tangentTex,
	in Texture2D emissionTex,
	in Texture2D misc0Tex,
	in Texture2D misc1Tex,
	in Texture2D misc2Tex,
	SamplerState samplerState,
	out GeometryAttributes attrs)
{
    float4 albedo = albedoTex.Sample(samplerState, tex);
    float4 position = positionTex.Sample(samplerState, tex);
    float4 normal = normalTex.Sample(samplerState, tex);
    float4 tangent = tangentTex.Sample(samplerState, tex);
    float4 emission = emissionTex.Sample(samplerState, tex);
    float4 misc0 = misc0Tex.Sample(samplerState, tex);
    float4 misc1 = misc1Tex.Sample(samplerState, tex);
    float4 misc2 = misc2Tex.Sample(samplerState, tex);

	attrs.albedo = albedo.rgb;
	attrs.opacity = albedo.a;
	attrs.pos.rgb = position.xyz;
	attrs.depth = position.w;
	attrs.normal = normal.xyz;
	attrs.roughness = normal.w;
    attrs.tangent = tangent.xyz;
    attrs.metalness = tangent.w;
	attrs.emission = emission.xyz;
	attrs.emissionstrength = emission.w;
    attrs.specular = misc0.r;
    attrs.speculartint = misc0.g;
    attrs.ao = misc0.b;
    attrs.ior = misc0.a;
    attrs.anisotropic = misc1.r;
    attrs.anisotropicrotation = misc1.g;
    attrs.clearcoat = misc1.b;
    attrs.clearcoatGloss = misc1.a;
    attrs.subsurface = misc2.r;
    attrs.sheen = misc2.b;
    attrs.sheentint = misc2.a;
}