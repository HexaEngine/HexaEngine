struct GeometryData
{
	float4 albedo : SV_TARGET0;
	float4 position : SV_TARGET1;
	float4 normal : SV_TARGET2;
	float4 emissive : SV_TARGET3;
};

struct GeometryAttributes
{
	float3 albedo;
	float3 pos;
	float3 normal;
	float3 emissive;
	float opacity;
	float metalness;
	float roughness;
	float ao;
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
	in float3 pos,
	in float3 normal,
	in float3 emissive,
	in float opacity,
	in float metalness,
	in float roughness,
	in float ao
)
{
	GeometryData data;
	data.albedo.rgb = albedo;
	data.albedo.a = opacity;
	data.position.xyz = pos.rgb;
	data.position.w = roughness;
	data.normal.xyz = normal;
	data.normal.w = metalness;
	data.emissive.rgb = emissive;
	data.emissive.w = ao;
	return data;
}

void ExtractGeometryData(
	in float2 tex,
	in Texture2D<float4> color,
	in Texture2D<float4> pos,
	in Texture2D<float4> normal,
	in Texture2D<float4> emissive,
	SamplerState samplerState,
	out GeometryAttributes attrs)
{
	float4 a = (float4) color.Sample(samplerState, tex);
	float4 b = (float4) pos.Sample(samplerState, tex);
	float4 c = (float4) normal.Sample(samplerState, tex);
	float4 d = (float4) emissive.Sample(samplerState, tex);

	attrs.albedo = a.rgb;
	attrs.opacity = a.a;
	attrs.pos.rgb = b.xyz;
	attrs.roughness = b.w;
	attrs.normal = c.xyz;
	attrs.metalness = c.w;
	attrs.emissive = d.rgb;
	attrs.ao = d.w;
}