// By Morgan McGuire and Michael Mara at Williams College 2014
// Released as open source under the BSD 2-Clause License
// http://opensource.org/licenses/BSD-2-Clause
//
// Copyright (c) 2014, Morgan McGuire and Michael Mara
// All rights reserved.
//
// From McGuire and Mara, Efficient GPU Screen-Space Ray Tracing,
// Journal of Computer Graphics Techniques, 2014
//
// This software is open source under the "BSD 2-clause license":
//
// Redistribution and use in source and binary forms, with or
// without modification, are permitted provided that the following
// conditions are met:
//
// 1. Redistributions of source code must retain the above
// copyright notice, this list of conditions and the following
// disclaimer.
//
// 2. Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following
// disclaimer in the documentation and/or other materials provided
// with the distribution.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
// CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
// USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
// AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
// LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
// IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
/**
 * The ray tracing step of the SSLR implementation.
 * Modified version of the work stated above.
 */

#include "HexaEngine.Core:shaders/camera.hlsl"
#include "HexaEngine.Core:shaders/gbuffer.hlsl"

cbuffer DDASSRParams
{
	float intensity;
	float cb_zThickness = 0.05f; // thickness to ascribe to each pixel in the depth buffer
	float cb_stride = 10; // Step in horizontal or vertical pixels between samples. This is a float
	// because integer math is slow on GPUs, but should be set to an integer >= 1.
	int cb_maxSteps = 100; // Maximum number of iterations. Higher gives better images but may be slow.
	float cb_maxDistance = 10; // Maximum camera-space distance to trace before returning a miss.
	float cb_strideZCutoff = 100; // More distant pixels are smaller in screen space. This value tells at what point to
};

static const float4x4 viewToTextureSpaceMatrix =
{
	0.5f, 0.0f, 0.0f, 0.5f,
	0.0f, -0.5f, 0.0f, 0.5f,
	0.0f, 0.0f, 1.0f, 0.0f,
	0.0f, 0.0f, 0.0f, 1.0f
};

SamplerState pointClampSampler : register(s0);
SamplerState linearClampSampler : register(s1);
SamplerState linearBorderSampler : register(s2);

Texture2D inputTex : register(t0);
Texture2D<float> depthTex : register(t1);
Texture2D normalTex : register(t2);

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float distanceSquared(float2 a, float2 b)
{
	a -= b;
	return dot(a, a);
}

bool intersectsDepthBuffer(float z, float minZ, float maxZ)
{
	/*
	 * Based on how far away from the camera the depth is,
	 * adding a bit of extra thickness can help improve some
	 * artifacts. Driving this value up too high can cause
	 * artifacts of its own.
	 */
	 //float depthScale = min(1.0f, z * cb_strideZCutoff);
	 //z += cb_zThickness + lerp(0.0f, 2.0f, depthScale);
	return (maxZ >= z) && (minZ - cb_zThickness <= z);
}

void swap(inout float a, inout float b)
{
	float t = a;
	a = b;
	b = t;
}

float linearDepthTexelFetch(int2 hitPixel)
{
	return GetLinearDepth(depthTex.Load(int3(hitPixel, 0)));
}

// Returns true if the ray hit something
bool traceScreenSpaceRay(
	// Camera-space ray origin, which must be within the view volume
	float3 csOrig,
	// Unit length camera-space ray direction
	float3 csDir,
	// Number between 0 and 1 for how far to bump the ray in stride units
	// to conceal banding artifacts. Not needed if stride == 1.
	float jitter,
	// Pixel coordinates of the first intersection with the scene
	out float2 hitPixel,
	// Camera space location of the ray hit
	out float3 hitPoint)
{
	// Clip to the near plane
	float rayLength = ((csOrig.z + csDir.z * cb_maxDistance) < camNear) ?
		(camNear - csOrig.z) / csDir.z : cb_maxDistance;
	float3 csEndPoint = csOrig + csDir * rayLength;

	// Project into homogeneous clip space
	float4 H0 = mul(float4(csOrig, 1.0f), proj);
	float4 H1 = mul(float4(csEndPoint, 1.0f), proj);

	float k0 = 1.0f / H0.w;
	float k1 = 1.0f / H1.w;

	// The interpolated homogeneous version of the camera-space points
	float3 Q0 = csOrig * k0;
	float3 Q1 = csEndPoint * k1;

	// Screen-space endpoints
	float2 P0 = H0.xy * k0;
	float2 P1 = H1.xy * k1;

	P0 = P0 * float2(0.5, -0.5) + float2(0.5, 0.5);
	P1 = P1 * float2(0.5, -0.5) + float2(0.5, 0.5);

	P0.xy *= screenDim.xy;
	P1.xy *= screenDim.xy;

	// If the line is degenerate, make it cover at least one pixel
	// to avoid handling zero-pixel extent as a special case later
	P1 += (distanceSquared(P0, P1) < 0.0001f) ? float2(0.01f, 0.01f) : 0.0f;
	float2 delta = P1 - P0;

	// Permute so that the primary iteration is in x to collapse
	// all quadrant-specific DDA cases later
	bool permute = false;
	if (abs(delta.x) < abs(delta.y))
	{
		// This is a more-vertical line
		permute = true;
		delta = delta.yx;
		P0 = P0.yx;
		P1 = P1.yx;
	}

	float stepDir = sign(delta.x);
	float invdx = stepDir / delta.x;

	// Track the derivatives of Q and k
	float3 dQ = (Q1 - Q0) * invdx;
	float dk = (k1 - k0) * invdx;
	float2 dP = float2(stepDir, delta.y * invdx);

	// Scale derivatives by the desired pixel stride and then
	// offset the starting values by the jitter fraction
	float strideScale = 1.0f - min(1.0f, csOrig.z * cb_strideZCutoff);
	float stride = 1.0f + strideScale * cb_stride;
	dP *= stride;
	dQ *= stride;
	dk *= stride;

	P0 += dP * jitter;
	Q0 += dQ * jitter;
	k0 += dk * jitter;

	// Slide P from P0 to P1, (now-homogeneous) Q from Q0 to Q1, k from k0 to k1
	float4 PQk = float4(P0, Q0.z, k0);
	float4 dPQk = float4(dP, dQ.z, dk);
	float3 Q = Q0;

	// Adjust end condition for iteration direction
	float end = P1.x * stepDir;

	float stepCount = 0.0f;
	float prevZMaxEstimate = csOrig.z;
	float rayZMin = prevZMaxEstimate;
	float rayZMax = prevZMaxEstimate;
	float sceneZMax = rayZMax + 200.0f;

	for (;
		((PQk.x * stepDir) <= end) && (stepCount < cb_maxSteps) &&
		!intersectsDepthBuffer(sceneZMax, rayZMin, rayZMax) &&
		(sceneZMax != 0.0f);
		++stepCount)
	{
		rayZMin = prevZMaxEstimate;
		rayZMax = (dPQk.z * 0.5f + PQk.z) / (dPQk.w * 0.5f + PQk.w);
		prevZMaxEstimate = rayZMax;

		if (rayZMin > rayZMax)
		{
			swap(rayZMin, rayZMax);
		}

		hitPixel = permute ? PQk.yx : PQk.xy;

		sceneZMax = linearDepthTexelFetch(hitPixel);

		PQk += dPQk;
	}

	// Advance Q based on the number of steps
	Q.xy += dQ.xy * stepCount;
	hitPoint = Q * (1.0f / PQk.w);

	return intersectsDepthBuffer(sceneZMax, rayZMin, rayZMax);
}

float4 main(VertexOut pin) : SV_TARGET
{
	uint2 screenPos = (uint2)(pin.Tex * screenDim);
	float4 scene_color = inputTex.Load(int3(screenPos, 0));
	float depth = depthTex.Load(int3(screenPos, 0)).r;

	float4 NormalRoughness = normalTex.Load(int3(screenPos, 0));
	float roughness = NormalRoughness.a;

	if (depth == 1)
	{
		return scene_color;
	}

	float3 normal = UnpackNormal(NormalRoughness.rgb);

	//get world position from depth
	float2 uv = (screenPos.xy + 0.5) * screenDimInv;
	float4 clipPos = float4(2 * uv - 1, depth, 1);
	clipPos.y = -clipPos.y;

	float3 toPosition = 0;
	float3 rayDirection = 0;
	float4 result = 0;
	bool intersection = false;

	//convert normal to view space to find the correct reflection vector
	float3 normalVS = mul(normal.xyz, (float3x3)view);

	float4 viewPos = mul(clipPos, projInv);
	viewPos.xyz /= viewPos.w;

	float3 rayOrigin = viewPos.xyz + normalVS * 0.01;
	toPosition = normalize(rayOrigin.xyz);

	/*
			* Since position is reconstructed in view space, just normalize it to get the
			* vector from the eye to the position and then reflect that around the normal to
			* get the ray direction to trace.
			*/
		rayDirection = reflect(toPosition, normalVS);

		float2 hitPixel = float2(0.0f, 0.0f);
		float3 hitPoint = float3(0.0f, 0.0f, 0.0f);

		float jitter = cb_stride > 1.0f ? float(int(screenPos.x + screenPos.y) & 1) * 0.5f : 0.0f;

		// perform ray tracing - true if hit found, false otherwise
		intersection = traceScreenSpaceRay(rayOrigin, rayDirection, jitter, hitPixel, hitPoint);

		// move hit pixel from pixel position to UVs;
		/*if (hitPixel.x > screenDim.x || hitPixel.x < 0.0f || hitPixel.y > screenDim.y || hitPixel.y < 0.0f)
		{
			intersection = false;
		}*/

		//return float4(rayDirectionVS, 1);

	#if SSR_CONE_TRACE
		// output rDotV to the alpha channel for use in determining how much to fade the ray
		float rDotV = dot(rayDirectionVS, toPositionVS);
	#endif

		depth = depthTex.Load(int3(hitPixel, 0)).r;

	#if SSR_CONE_TRACE
		return float4(hitPixel, depth, rDotV) * (intersection ? 1.0f : 0.0f);
	#endif

		//return float4(hitPixel, 0, 1) * (intersection ? 1.0f : 0.0f);

		float3 reflectionColor = (intersection ? 1.0f : 0.0f) * inputTex.Load(int3(hitPixel, 0)).rgb;

		//return float4(reflectionColor, 1);
		return scene_color + (1 - roughness) * max(0, float4(reflectionColor, 1.0f));
}