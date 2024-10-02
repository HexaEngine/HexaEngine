#include "../../weather.hlsl"
#include "../../camera.hlsl"

struct VertexOut
{
	float4 position : SV_POSITION;
	float3 pos : POSITION;
	float3 tex : TEXCOORD;
};

float saturatedDot(in float3 a, in float3 b)
{
	return abs(max(dot(a, b), 0.0));
}

float3 YxyToXYZ(in float3 Yxy)
{
	float Y = Yxy.r;
	float x = Yxy.g;
	float y = Yxy.b;

	float X = x * (Y / y);
	float Z = (1.0 - x - y) * (Y / y);

	return float3(X, Y, Z);
}

float3 XYZToRGB(in float3 XYZ)
{
	// CIE/E
	float3x3 M = float3x3
	(
		2.3706743, -0.9000405, -0.4706338,
		-0.5138850, 1.4253036, 0.0885814,
		0.0052982, -0.0146949, 1.0093968
	);

	return mul(M, XYZ);
}

float3 YxyToRGB(in float3 Yxy)
{
	float3 XYZ = YxyToXYZ(Yxy);
	float3 RGB = XYZToRGB(XYZ);
	return RGB;
}

float3 calculatePerezLuminanceYxy(in float theta, in float gamma, in float3 A, in float3 B, in float3 C, in float3 D, in float3 E)
{
	return (1.0 + A * exp(B / cos(theta))) * (1.0 + C * exp(D * gamma) + E * cos(gamma) * cos(gamma));
}

float3 calculateSkyLuminanceRGB(in float3 s, in float3 e)
{
	float thetaS = acos(clamp(s.y, 0, 1));
	float thetaE = acos(saturatedDot(e, float3(0, 1, 0)));
	float gammaE = acos(max(dot(s, e) - 1.19209290e-07, 0));

	float3 fThetaGamma = calculatePerezLuminanceYxy(thetaE, gammaE, A, B, C, D, E);
	float3 fZeroThetaS = calculatePerezLuminanceYxy(0.0, thetaS, A, B, C, D, E);

	float3 Yp = F * (fThetaGamma / fZeroThetaS);

	return YxyToRGB(Yp);
}

float4 main(VertexOut pin) : SV_TARGET
{
	float3 dir = normalize(pin.tex);

	float3 skyLuminance = calculateSkyLuminanceRGB(light_dir.xyz, dir);

	skyLuminance *= 0.05; // hdr compensation.

	float distanceToSun = distance(light_dir.xyz, dir);
	float sunRadius = 0.05f;
	float sunIntensity = 10.0f;
	float brightness = saturate(1.0f - distanceToSun / sunRadius);
	float3 sunColor = float3(1.0f, 1.0f, 0.5f);
	float3 L0 = sunColor * brightness * sunIntensity;
	float3 finalColor = skyLuminance + L0;

	// Fog blending logic

	bool useHeightBased = (fog_mode & 0x04) != 0;
	float heightFactor = 1;
	if (useHeightBased)
	{
		heightFactor = GetFogHeightFactor(pin.pos.y);
	}

	float fogBlend = heightFactor * fog_intensity;
	finalColor = lerp(finalColor, fog_color, fogBlend);  // Blend sky with fog based on height

	return float4(finalColor, 0.0);
}