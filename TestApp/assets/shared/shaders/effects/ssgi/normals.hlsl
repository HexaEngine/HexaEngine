#include "../../camera.hlsl"
struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

// Get a cosine-weighted random vector centered around a specified normal direction.
float3 GetCosHemisphereSample(float rand1, float rand2, float3 hitNorm)
{
	// Get 2 random numbers to select our sample with
	float2 randVal = float2(rand1, rand2);

	// Cosine weighted hemisphere sample from RNG
	float3 bitangent = GetPerpendicularVector(hitNorm);
	float3 tangent = cross(bitangent, hitNorm);
	float r = sqrt(randVal.x);
	float phi = 2.0f * 3.14159265f * randVal.y;

	// Get our cosine-weighted hemisphere lobe sample direction
	return tangent * (r * cos(phi).x) + bitangent * (r * sin(phi)) + hitNorm.xyz * sqrt(max(0.0, 1.0f - randVal.x));
}
float4 CreateCustomNormals(VertexOutput i) : SV_Target
{
	float2 uv = i.uv;
	int2 pos = uv * screenResolution.xy;

	float4 worldNormal = GetNormal(uv);
	float normalLength = length(worldNormal);

	float noise = IGN(pos.x,pos.y, frameCount); //Animated Interleaved Gradint Noise
	float3 stochasticNormal = GetCosHemisphereSample(noise, noise, worldNormal);

	return normalize(float4(stochasticNormal,1));
}