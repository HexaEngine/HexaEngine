#include "HexaEngine.Core:shaders/common.hlsl"

Texture2D inputTex;
Texture2D<float> depthTex;
Texture2D normalTex;

SamplerState linearClampSampler;

cbuffer SSGIParams : register(b0)
{
	float intensity;
	float distance;
	int SSGI_RAY_COUNT;
	int SSGI_RAY_STEPS;
	float SSGI_RAY_STEP;
	float SSGI_DEPTH_BIAS;
};

#define NUM_SAMPLES 16
#define MAX_MARCH_DISTANCE 0.5f
#define RAY_MARCH_STEP 0.1f

float3 GetViewPos(float2 coord)
{
	float depth = depthTex.Sample(linearClampSampler, coord).r;
	return GetPositionVS(coord, depth);
}

float3 GetViewNormal(float2 coord)
{
	float3 normal = normalTex.Sample(linearClampSampler, coord).rgb;
	return normalize(mul(UnpackNormal(normal), (float3x3)view));
}

float lenSq(float3 v)
{
	return pow(v.x, 2.0) + pow(v.y, 2.0) + pow(v.z, 2.0);
}

float3 rand3(float3 c)
{
	float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
	float3 r;
	r.z = frac(512.0 * j);
	j *= .125;
	r.x = frac(512.0 * j);
	j *= .125;
	r.y = frac(512.0 * j);
	return (r - 0.5);
}

// ----------------------------------------------------------------------------
// http://holger.dammertz.org/stuff/notes_HammersleyOnHemisphere.html
// efficient VanDerCorpus calculation.
float RadicalInverse_VdC(uint bits)
{
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}
// ----------------------------------------------------------------------------
float2 Hammersley(uint i, uint N)
{
	return float2(float(i) / float(N), RadicalInverse_VdC(i));
}

float3 SampleHemisphere(float3 normal, float2 random)
{
	// Create a local coordinate system based on the normal
	float3 up = abs(normal.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
	float3 tangent = normalize(cross(up, normal));
	float3 bitangent = cross(normal, tangent);

	// Cosine-weighted hemisphere sampling
	float phi = 2.0 * PI * random.x;
	float cosTheta = sqrt(random.y);
	float sinTheta = sqrt(1.0 - cosTheta * cosTheta);

	float3 sampleDir = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);

	// Transform to world space
	return tangent * sampleDir.x + bitangent * sampleDir.y + normal * sampleDir.z;
}

float3 ComputeScreenSpaceGI(float2 uv, float3 viewPos, float3 normal)
{
	float3 accumulatedGI = float3(0.0f, 0.0f, 0.0f);
	int numSamples = 0;

	[loop]
	for (int step = 0; step < SSGI_RAY_COUNT; ++step)
	{
		float marchDistance = step * SSGI_RAY_STEP;

        float2 randomValues = Hammersley(step, SSGI_RAY_COUNT);
		float3 rayDir = SampleHemisphere(normal, randomValues);

		float3 newViewPos = viewPos + rayDir * marchDistance;
		float2 sampleUV = ProjectUV(newViewPos);

		sampleUV = clamp(sampleUV, float2(0.0f, 0.0f), float2(1.0f, 1.0f));

		float depthSample = depthTex.Sample(linearClampSampler, sampleUV);

        if (depthSample > 1.0f || marchDistance > distance)
		{
			continue;
		}

		float3 lightColor = inputTex.Sample(linearClampSampler, sampleUV).rgb;
		float3 lightNormal = GetViewNormal(sampleUV);
		float3 lightPos = GetViewPos(sampleUV);

		float3 lightPath = lightPos - viewPos;
		float3 lightDir = normalize(lightPath);

		float cosemit = clamp(dot(lightDir, -lightNormal), 0.0, 1.0);
		float coscatch = clamp(dot(lightDir, normal) * 0.5 + 0.5, 0.0, 1.0);
		float distfall = pow(lenSq(lightPath), 0.1) + 1.0;
		accumulatedGI += (lightColor * cosemit * coscatch / distfall) * (length(lightPos) / 20);
		numSamples++;
	}

	return accumulatedGI / numSamples;
}

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float4 main(VertexOut pin) : SV_Target
{
	float2 uv = pin.Tex;
	float depth = depthTex.Sample(linearClampSampler, uv);
	float3 position = GetPositionVS(uv, depth);
	float3 normal = GetViewNormal(uv);
	float3 gi = ComputeScreenSpaceGI(uv, position, normal);
	return float4(gi, 1.0f);
}