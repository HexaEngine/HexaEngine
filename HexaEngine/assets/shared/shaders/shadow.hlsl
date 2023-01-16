#include "light.hlsl"
#include "camera.hlsl"

float ShadowCalculation(
	PointLightSD light,
	float3 fragPos,
	TextureCube depthTex,
	SamplerState state)
{
	// get vector between fragment position and light position
	float3 fragToLight = fragPos - light.position;
	float3 texCoord = float3(fragToLight.x, fragToLight.y, fragToLight.z);
	// use the light to fragment vector to sample from the depth map
	float closestDepth = depthTex.Sample(state, texCoord).r;
	// it is currently in linear range between [0,1]. Re-transform back to original value
	closestDepth *= 25;
	// now get current linear depth as the length between the fragment and light position
	float currentDepth = length(fragToLight);
	// now test for shadows
	float bias = 0.05;
	float shadow = currentDepth - bias > closestDepth ? 1.0 : 0.0;

	return shadow;
}

float ShadowCalculation(
	DirectionalLightSD light,
	float3 fragPos,
	float3 N,
	float4x4 lightSpaceMatrices[16],
	float4 cascadePlaneDistancesPack[4],
	uint cascadeCount,
	Texture2DArray depthTex,
	SamplerState state)
{
	float cascadePlaneDistances[16] = (float[16]) cascadePlaneDistancesPack;
	float farPlane = 100;

	// select cascade layer
	float4 fragPosViewSpace = mul(float4(fragPos, 1.0), view);
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

	float4 fragPosLightSpace = mul(float4(fragPos, 1.0), lightSpaceMatrices[layer]);
	fragPosLightSpace.y = -fragPosLightSpace.y;
	float3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
	float currentDepth = projCoords.z;
	projCoords = projCoords * 0.5 + 0.5;

	// calculate bias (based on depth map resolution and slope)
	float bias = max(0.05 * (1.0 - dot(N, -light.dir)), 0.005);
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
	float w;
	float h;
	uint s;
	depthTex.GetDimensions(w, h, s);
	float2 texelSize = 1.0 / float2(w, h);
	[unroll]
	for (int x = -1; x <= 1; ++x)
	{
		[unroll]
		for (int y = -1; y <= 1; ++y)
		{
			float pcfDepth = depthTex.Sample(state, float3(projCoords.xy + float2(x, y) * texelSize, layer)).r;
			shadow += (currentDepth - 0.005) > pcfDepth ? 1.0 : 0.0;
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