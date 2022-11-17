#include "../../camera.hlsl"

struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

static const float g_FarPlaneDist = 100;
static const float g_nearPlaneDist = 0.001f;

cbuffer ConfigBuffer
{
	float2 g_targetSize = float2(200, 200);
	float2 g_invTargetSize = 1 / float2(200, 200);
	int g_maxRayStep = 70;
	float3 padd0;
	float g_depthbias = 0.0001f;
	float g_rayStepScale = 1.05f;
	float g_maxThickness = 1.8f;
	float g_maxRayLength = 200.f;
};

Texture2D colorTexture : register(t0);
Texture2D positionTexture : register(t1);
Texture2D normalTexture : register(t2);
Texture2D backdepthTexture : register(t3);

SamplerState samplerState;

void swap(inout float lhs, inout float rhs)
{
	float temp = lhs;
	lhs = rhs;
	rhs = temp;
}

float Noise(float2 seed)
{
	return frac(sin(dot(seed.xy, float2(12.9898, 78.233))) * 43758.5453);
}

bool TraceScreenSpaceRay(float3 dir, float3 viewPos, out float3 hitPixel_alpha)
{
	float rayLength = (viewPos.z + dir.z * g_maxRayLength) < g_nearPlaneDist ? (g_nearPlaneDist - viewPos.z) / dir.z : g_maxRayLength;

	hitPixel_alpha = float3(-1, -1, 1);

	float3 rayEnd = viewPos + dir * rayLength;

	float4 ssRayBegin = mul(float4(viewPos, 1.0), proj);
	float4 ssRayEnd = mul(float4(rayEnd, 1.0), proj);

	float k0 = 1 / ssRayBegin.w;
	float k1 = 1 / ssRayEnd.w;

	float2 P0 = ssRayBegin.xy * k0;
	float2 P1 = ssRayEnd.xy * k1;

	P0 = (P0 * 0.5 + 0.5) * g_targetSize;
	P1 = (P1 * 0.5 + 0.5) * g_targetSize;

	P1 += (dot(P1 - P0, P1 - P0) < 0.0001) ? float2(0.01, 0.01) : float2(0, 0);
	float2 delta = P1 - P0;

	bool permute = false;
	if (abs(delta.x) < abs(delta.y))
	{
		permute = true;
		delta = delta.yx;
		P0 = P0.yx;
		P1 = P1.yx;
	}

	float stepDir = sign(delta.x);
	float invdx = stepDir / delta.x;

	float stepCount = 0;
	float rayZ = viewPos.z;
	float sceneZMax = viewPos.z + cam_far;

	float end = stepDir * P1.x;

	float3 Pk = float3(P0, k0);
	float3 dPk = float3(float2(stepDir, delta.y * invdx), (k1 - k0) * invdx);

	dPk *= g_rayStepScale;

	//float jitter = Noise( Pk.xy );
	//Pk += dPk * jitter;

	float thickness = g_maxThickness;

	[loop]
	for (;
		((Pk.x * stepDir) <= end) &&
		(stepCount < g_maxRayStep) &&
		((rayZ < sceneZMax) ||
			((rayZ - thickness) > sceneZMax)) &&
		(sceneZMax != 0.0);
		Pk += dPk,
		stepCount += 1)
	{
		hitPixel_alpha.xy = permute ? Pk.yx : Pk.xy;
		hitPixel_alpha.xy *= g_invTargetSize;
		hitPixel_alpha.y = 1 - hitPixel_alpha.y;

		rayZ = 1 / Pk.z;

		sceneZMax = positionTexture.SampleLevel(samplerState, hitPixel_alpha.xy, 0).w;
		thickness = backdepthTexture.SampleLevel(samplerState, hitPixel_alpha.xy, 0).r * g_FarPlaneDist;
		sceneZMax *= g_FarPlaneDist;
		thickness -= sceneZMax;
		sceneZMax += g_depthbias;
	}

	float edgeFade = 1.f - pow(length(hitPixel_alpha.xy - 0.5) * 2.f, 3.0f);
	hitPixel_alpha.z = edgeFade;
	return (rayZ >= sceneZMax) && (rayZ - thickness <= sceneZMax);
}

float4 main(VSOut input) : SV_TARGET
{
	float4 gpos = positionTexture.SampleLevel(samplerState, input.Tex, 0);
	float4 gnormal = normalTexture.SampleLevel(samplerState, input.Tex, 0);

	if (gpos.w == 0)
		return float4(0, 0, 0, 0);

	if (gnormal.w > 0.4f)
		return float4(0, 0, 0, 0);

	float4 pos = float4(gpos.xyz, 1);
	float3 viewPos = mul(pos, view).xyz;

	float3 normal = mul(gnormal.xyz, (float3x3) view);

	float3 incidentVec = normalize(viewPos);
	float3 viewNormal = normalize(normal);

	float3 reflectVec = reflect(incidentVec, viewNormal);
	reflectVec = normalize(reflectVec);

	float3 hitPixel_alpha;
	bool isHit = TraceScreenSpaceRay(reflectVec, viewPos, hitPixel_alpha);

	float3 color = colorTexture.SampleLevel(samplerState, hitPixel_alpha.xy, 0).rgb;

	return float4(color, hitPixel_alpha.z) * isHit;
}