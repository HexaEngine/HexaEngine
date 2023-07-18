struct GeometryData
{
	float4 GBufferA : SV_TARGET0;
    float4 GBufferB : SV_TARGET1;
    float4 GBufferC : SV_TARGET2;
    float4 GBufferD : SV_TARGET3;
};

struct GeometryAttributes
{
	float3 baseColor;
	float3 normal;
    float3 emission;
    float3 emissionStrength;
    int materialID;
	float roughness;
	float metallic;
    float reflectance;
	float ao;
    float materialData;
};

float3 PackNormal(float3 normal)
{
    return 0.5 * normal + 0.5;
}

float3 UnpackNormal(float3 normal)
{
    return 2 * normal - 1;
}

GeometryData PackGeometryData(
	in float materialID,
	in float3 baseColor,
	in float3 normal,
	in float roughness,
	in float metallic,
	in float reflectance,
	in float ao,
	in float materialData,
	in float3 emission,
	in float emissionStrength
)
{
	GeometryData data;
    data.GBufferA.rgb = baseColor;
    data.GBufferA.a = materialID;
    data.GBufferB.xyz = PackNormal(normal);
    data.GBufferB.w = roughness;
    data.GBufferC.x = metallic;
    data.GBufferC.y = reflectance;
    data.GBufferC.z = ao;
    data.GBufferC.w = materialData;
    data.GBufferD.rgb = emission;
    data.GBufferD.a = emissionStrength;
	return data;
}

void ExtractGeometryData(
	in float2 tex,
	in Texture2D GBufferA,
	in Texture2D GBufferB,
	in Texture2D GBufferC,
	in Texture2D GBufferD,
	SamplerState state,
	out GeometryAttributes attrs)
{
    float4 a = GBufferA.Sample(state, tex);
    float4 b = GBufferB.Sample(state, tex);
    float4 c = GBufferC.Sample(state, tex);
    float4 d = GBufferD.Sample(state, tex);

	attrs.baseColor = a.rgb;
	attrs.materialID = a.a;
    attrs.normal = UnpackNormal(b.xyz);
    attrs.roughness = b.w;
    attrs.metallic = c.x;
    attrs.reflectance = c.y;
    attrs.ao = c.z;
    attrs.materialData = c.w;
    attrs.emission = d.xyz;
    attrs.emissionStrength = d.a;
}

