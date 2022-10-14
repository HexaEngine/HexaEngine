////////////////////////////////////////////////////////////////////////////////
// Filename: light.ps
////////////////////////////////////////////////////////////////////////////////
#include "../../gbuffer.hlsl"
#include "../../brdf.hlsl"
#include "../../camera.hlsl"
#include "../../light.hlsl"

Texture2D colorTexture : register(t0);
Texture2D positionTexture : register(t1);
Texture2D normalTexture : register(t2);
Texture2D cleancoatNormalTexture : register(t3);
Texture2D emissionTexture : register(t4);
Texture2D misc0Texture : register(t5);
Texture2D misc1Texture : register(t6);
Texture2D misc2Texture : register(t7);
TextureCube irradianceTexture : register(t8);
TextureCube prefilterTexture : register(t9);
Texture2D brdfLUT : register(t10);
Texture2D ssao : register(t11);

Texture2DArray depthCSM : register(t12);
TextureCube depthOSM[8] : register(t13);
Texture2D depthPSM[8] : register(t21);

SamplerState SampleTypePoint : register(s0);
SamplerState SampleTypeAnsio : register(s1);

//////////////
// TYPEDEFS //
//////////////
struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float ShadowCalculation(SpotlightSD light, float3 fragPos, float bias, Texture2D depthTex, SamplerState state)
{
	float4 fragPosLightSpace = mul(float4(fragPos, 1.0), light.view);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	float currentDepth = projCoords.z;

	projCoords = projCoords * 0.5 + 0.5;

	float w;
	float h;
	depthTex.GetDimensions(w, h);

	// PCF
	float shadow = 0.0;
	float2 texelSize = 1.0 / float2(w, h);
	[unroll]
	for (int x = -1; x <= 1; ++x)
	{
		[unroll]
		for (int y = -1; y <= 1; ++y)
		{
			float pcfDepth = depthTex.Sample(state, projCoords.xy + float2(x, y) * texelSize).r;
			shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
		}
	}

	shadow /= 9;

	if (currentDepth > 1.0)
		shadow = 0;

	return shadow;
}

// array of offset direction for sampling
float3 gridSamplingDisk[20] =
{
	float3(1, 1, 1), float3(1, -1, 1), float3(-1, -1, 1), float3(-1, 1, 1),
   float3(1, 1, -1), float3(1, -1, -1), float3(-1, -1, -1), float3(-1, 1, -1),
   float3(1, 1, 0), float3(1, -1, 0), float3(-1, -1, 0), float3(-1, 1, 0),
   float3(1, 0, 1), float3(-1, 0, 1), float3(1, 0, -1), float3(-1, 0, -1),
   float3(0, 1, 1), float3(0, -1, 1), float3(0, -1, -1), float3(0, 1, -1)
};

float ShadowCalculation(PointLightSD light, float3 fragPos, float3 V, TextureCube depthTex, SamplerState state)
{
#if (HARD_POINT_SHADOWS)
	// get vector between fragment position and light position
	float3 fragToLight = fragPos - light.position;
	// use the light to fragment vector to sample from the depth map
	float closestDepth = depthTex.Sample(state, fragToLight).r; //texture(depthMap, fragToLight).r;
	// it is currently in linear range between [0,1]. Re-transform back to original value
	closestDepth *= 25;
	// now get current linear depth as the length between the fragment and light position
	float currentDepth = length(fragToLight);
	// now test for shadows
	float bias = 0.05;
	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	return shadow;
#else
	// get vector between fragment position and light position
	float3 fragToLight = fragPos - light.position;
	// now get current linear depth as the length between the fragment and light position
	float currentDepth = length(fragToLight);
	float shadow = 0.0;
	float bias = 0.15;
	float viewDistance = length(V);
	float diskRadius = (1.0 + (viewDistance / 25)) / 25.0;

	[unroll]
	for (int i = 0; i < 20; ++i)
	{
		float closestDepth = depthTex.Sample(state, fragToLight + gridSamplingDisk[i] * diskRadius).r;
		closestDepth *= 25; // undo mapping [0;1]
		shadow += (currentDepth - bias) > closestDepth ? 1.0 : 0.0;
	}
	shadow /= 20;
	return shadow;
#endif
}

float ShadowCalculation(DirectionalLightSD light, float3 fragPosWorldSpace, float3 normal, Texture2DArray depthTex, SamplerState state)
{
	float cascadePlaneDistances[16] = (float[16]) light.cascades;
	float farPlane = 100;

	float w;
	float h;
	uint cascadeCount;
	depthTex.GetDimensions(w, h, cascadeCount);

	// select cascade layer
	float4 fragPosViewSpace = mul(float4(fragPosWorldSpace, 1.0), view);
	float depthValue = abs(fragPosViewSpace.z);
	float cascadePlaneDistance;
	uint layer = cascadeCount;
	for (uint i = 0; i < cascadeCount; ++i)
	{
		if (depthValue < cascadePlaneDistances[i])
		{
			cascadePlaneDistance = cascadePlaneDistances[i];
			layer = i;
			break;
		}
	}

	float4 fragPosLightSpace = mul(float4(fragPosWorldSpace, 1.0), light.views[layer]);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	float currentDepth = projCoords.z;
	projCoords = projCoords * 0.5 + 0.5;

	// calculate bias (based on depth map resolution and slope)
	normal = normalize(normal);
	float bias = max(0.05 * (1.0 - dot(normal, light.dir)), 0.005);
	const float biasModifier = 0.5f;
	if (layer == cascadeCount)
	{
		bias *= 1 / (farPlane * biasModifier);
	}
	else
	{
		bias *= 1 / (cascadePlaneDistance * biasModifier);
	}

	// PCF
	float shadow = 0.0;
	float2 texelSize = 1.0 / float2(w, h);
	[unroll]
	for (int x = -1; x <= 1; ++x)
	{
		[unroll]
		for (int y = -1; y <= 1; ++y)
		{
			float pcfDepth = depthTex.Sample(state, float3(projCoords.xy + float2(x, y) * texelSize, layer)).r;
			shadow += (currentDepth - bias) > pcfDepth ? 1.0 : 0.0;
		}
	}

	shadow /= 9;

	// keep the shadow at 0.0 when outside the far_plane region of the light's frustum.
	if (currentDepth > 1.0)
	{
		shadow = 0.0;
	}

	return shadow;
}

float4 ComputeLightingPBR(VSOut input, GeometryAttributes attrs)
{
	float3 position = attrs.pos;
	float3 baseColor = attrs.albedo;

	float roughness = attrs.roughness;
	float metalness = attrs.metalness;

	float3 N = normalize(attrs.normal);
	float3 V = normalize(GetCameraPos() - position);
	float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metalness);

	float3 Lo = float3(0.0f, 0.0f, 0.0f);

	for (uint x = 0; x < directionalLightCount; x++)
	{
		DirectionalLight light = directionalLights[x];
		Lo += BRDFDirect(light.color.rgb, normalize(-light.dir), F0, V, N, baseColor, roughness, metalness);
	}

	for (uint y = 0; y < directionalLightSDCount; y++)
	{
		DirectionalLightSD light = directionalLightSDs[y];
		float bias = max(0.05f * (1.0 - dot(N, V)), 0.005f);
		float shadow = ShadowCalculation(light, attrs.pos, N, depthCSM, SampleTypeAnsio);
		Lo += (1.0f - shadow) * BRDFDirect(light.color.rgb, normalize(-light.dir), F0, V, N, baseColor, roughness, metalness);
	}

	for (uint z = 0; z < pointLightCount; z++)
	{
		PointLight light = pointLights[z];
		float3 LN = light.position - position;
		float distance = length(LN);
		float3 L = normalize(LN);

		float attenuation = 1.0 / (distance * distance);
		float3 radiance = light.color.rgb * attenuation;

		Lo += BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metalness);
	}

	for (uint zd = 0; zd < pointLightSDCount; zd++)
	{
		PointLightSD light = pointLightSDs[zd];
		float3 LN = light.position - position;
		float distance = length(LN);
		float3 L = normalize(LN);

		float attenuation = 1.0 / (distance * distance);
		float3 radiance = light.color.rgb * attenuation;
		float shadow = ShadowCalculation(light, attrs.pos, V, depthOSM[zd], SampleTypeAnsio);
		Lo += (1.0f - shadow) * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metalness);
	}

	for (uint w = 0; w < spotlightCount; w++)
	{
		Spotlight light = spotlights[w];
		float3 LN = light.pos - position;
		float3 L = normalize(LN);

		float theta = dot(L, normalize(-light.dir));
		if (theta > light.cutOff)
		{
			float distance = length(LN);
			float attenuation = 1.0 / (distance * distance);
			float epsilon = light.cutOff - light.outerCutOff;
			float falloff = 1;
			if (epsilon != 0)
				falloff = 1 - smoothstep(0.0, 1.0, (theta - light.outerCutOff) / epsilon);
			float3 radiance = light.color.rgb * attenuation * falloff;
			Lo += BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metalness);
		}
	}

	for (uint wd = 0; wd < spotlightSDCount; wd++)
	{
		SpotlightSD light = spotlightSDs[wd];
		float3 LN = light.pos - position;
		float3 L = normalize(LN);

		float theta = dot(L, normalize(-light.dir));
		if (theta > light.cutOff)
		{
			float distance = length(LN);
			float attenuation = 1.0 / (distance * distance);
			float epsilon = light.cutOff - light.outerCutOff;
			float falloff = 1;
			if (epsilon != 0)
				falloff = 1 - smoothstep(0.0, 1.0, (theta - light.outerCutOff) / epsilon);
			float3 radiance = light.color.rgb * attenuation * falloff;
			float cosTheta = dot(N, -L);
			float bias = clamp(0.005 * tan(acos(cosTheta)), 0, 0.01);
			float shadow = ShadowCalculation(light, attrs.pos, bias, depthPSM[wd], SampleTypeAnsio);

			Lo += (1.0f - shadow) * BRDFDirect(radiance, L, F0, V, N, baseColor, roughness, metalness);
		}
	}

	float ao = ssao.Sample(SampleTypePoint, input.Tex).r * attrs.ao;
	float3 ambient = BRDFIndirect2(SampleTypeAnsio, irradianceTexture, prefilterTexture, brdfLUT, F0, N, V, baseColor, roughness, ao);

	float3 color = ambient + Lo;

	return float4(color, attrs.opacity);
}

float4 main(VSOut pixel) : SV_TARGET
{
	GeometryAttributes attrs;
	ExtractGeometryData(
	pixel.Tex,
	colorTexture,
	positionTexture,
	normalTexture,
	cleancoatNormalTexture,
	emissionTexture,
	misc0Texture,
	misc1Texture,
	misc2Texture,
	SampleTypePoint,
	attrs);
	return ComputeLightingPBR(pixel, attrs);
}