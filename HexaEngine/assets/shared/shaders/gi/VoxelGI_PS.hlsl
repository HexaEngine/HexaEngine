//https://github.com/turanszkij/WickedEngine/blob/master/WickedEngine/

#include "common.hlsl"

cbuffer VoxelCbuf : register(b0)
{
	VoxelRadiance voxel_radiance;
}

static const float3 CONES[] =
{
	float3(0.57735, 0.57735, 0.57735),
	float3(0.57735, -0.57735, -0.57735),
	float3(-0.57735, 0.57735, -0.57735),
	float3(-0.57735, -0.57735, 0.57735),
	float3(-0.903007, -0.182696, -0.388844),
	float3(-0.903007, 0.182696, 0.388844),
	float3(0.903007, -0.182696, 0.388844),
	float3(0.903007, 0.182696, -0.388844),
	float3(-0.388844, -0.903007, -0.182696),
	float3(0.388844, -0.903007, 0.182696),
	float3(0.388844, 0.903007, -0.182696),
	float3(-0.388844, 0.903007, 0.182696),
	float3(-0.182696, -0.388844, -0.903007),
	float3(0.182696, 0.388844, -0.903007),
	float3(-0.182696, 0.388844, 0.903007),
	float3(0.182696, -0.388844, 0.903007)
};
static const float sqrt2 = 1.414213562;
inline float4 ConeTrace(in Texture3D<float4> voxels, in float3 P, in float3 N, in float3 coneDirection, in float coneAperture)
{
	float3 color = 0;
	float alpha = 0;

	// We need to offset the cone start position to avoid sampling its own voxel (self-occlusion):
	//	Unfortunately, it will result in disconnection between nearby surfaces :(
	float dist = voxel_radiance.DataSize; // offset by cone dir so that first sample of all cones are not the same
	float3 startPos = P + N * voxel_radiance.DataSize * 2 * sqrt2; // sqrt2 is diagonal voxel half-extent

	// We will break off the loop if the sampling distance is too far for performance reasons:
	while (dist < voxel_radiance.MaxDistance && alpha < 1)
	{
		float diameter = max(voxel_radiance.DataSize, 2 * coneAperture * dist);
		float mip = log2(diameter * voxel_radiance.DataSizeRCP);

		// Because we do the ray-marching in world space, we need to remap into 3d texture space before sampling:
		//	todo: optimization could be doing ray-marching in texture space
		float3 tc = startPos + coneDirection * dist;
		tc = (tc - voxel_radiance.GridCenter) * voxel_radiance.DataSizeRCP;
		tc *= voxel_radiance.DataResRCP;
		tc = tc * float3(0.5f, -0.5f, 0.5f) + 0.5f;

		// break if the ray exits the voxel grid, or we sample from the last mip:
		if (any(tc < 0) || any(tc > 1) || mip >= (float)voxel_radiance.Mips)
			break;

		float4 sam = voxels.SampleLevel(linear_clamp_sampler, tc, mip);

		// this is the correct blending to avoid black-staircase artifact (ray stepped front-to back, so blend front to back):
		float a = 1 - alpha;
		color += a * sam.rgb;
		alpha += a * sam.a;

		// step along ray:
		dist += diameter * voxel_radiance.RayStepSize;
	}

	return float4(color, alpha);
}
inline float4 ConeTraceRadiance(in Texture3D<float4> voxels, in float3 P, in float3 N)
{
	float4 radiance = 0;

	for (uint cone = 0; cone < voxel_radiance.NumCones; ++cone) // quality is between 1 and 16 cones
	{
		// approximate a hemisphere from random points inside a sphere:
		//  (and modulate cone with surface normal, no banding this way)
		float3 coneDirection = normalize(CONES[cone] + N);
		// if point on sphere is facing below normal (so it's located on bottom hemisphere), put it on the opposite hemisphere instead:
		coneDirection *= dot(coneDirection, N) < 0 ? -1 : 1;

		radiance += ConeTrace(voxels, P, N, coneDirection, tan(PI * 0.5f * 0.33f));
	}

	// final radiance is average of all the cones radiances
	radiance *= voxel_radiance.NumConesRCP;
	radiance.a = saturate(radiance.a);

	return max(0, radiance);
}
inline float4 ConeTraceReflection(in Texture3D<float4> voxels, in float3 P, in float3 N, in float3 V, in float roughness)
{
	float aperture = tan(roughness * PI * 0.5f * 0.1f);
	float3 coneDirection = reflect(-V, N);

	float4 reflection = ConeTrace(voxels, P, N, coneDirection, aperture);

	return float4(max(0, reflection.rgb), saturate(reflection.a));
}

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEX;
};

Texture2D normalMetallicTx : register(t0);
Texture2D<float> depthTx : register(t1);
Texture3D<float4> voxelTexture : register(t2);

float4 main(VertexOut pin) : SV_TARGET
{
	//unpack gbuffer
   float4 NormalMetallic = normalMetallicTx.Sample(linear_wrap_sampler, pin.Tex);
   float3 Normal = 2 * NormalMetallic.rgb - 1.0;
   float metallic = NormalMetallic.a;
   float depth = depthTx.Sample(linear_wrap_sampler, pin.Tex);
   float3 view_pos = GetPositionVS(pin.Tex, depth);

   float4 world_pos = mul(float4(view_pos, 1.0f), viewInv);
   world_pos /= world_pos.w;

   float3 world_normal = mul(Normal, (float3x3)transpose(view));

   return ConeTraceRadiance(voxelTexture, world_pos.xyz, world_normal);
}