#include "../../weather.hlsl"
#include "../../math.hlsl"

struct VertexOut
{
    float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
};

TextureCube cubeMap : register(t0);
SamplerState linearWrapSampler : register(s0);

float saturatedDot(in float3 a, in float3 b)
{
    return max(dot(a, b), 0.0);
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

float3 calculateSkyLuminanceRGB(in float3 s, in float3 e, in float t)
{
    float thetaS = acos(clamp(s.y, 0, 1));
    float thetaE = acos(saturatedDot(e, float3(0, 1, 0)));
    float gammaE = acos(saturatedDot(s, e));

    float3 fThetaGamma = calculatePerezLuminanceYxy(thetaE, gammaE, A, B, C, D, E);
    float3 fZeroThetaS = calculatePerezLuminanceYxy(0.0, thetaS, A, B, C, D, E);

    float3 Yp = F * (fThetaGamma / fZeroThetaS);

    return YxyToRGB(Yp);
}

float4 main(VertexOut pin) : SV_TARGET
{
    float3 dir = normalize(pin.pos);

    float3 skyLuminance = calculateSkyLuminanceRGB(light_dir.xyz, dir, 2);

    skyLuminance *= 0.005;

    float lerpFactor = 1;

    float cosTheta = light_dir.y;
    if (cosTheta < 0.0f)    // Handle sun going below the horizon
    {
        float a = clamp(1.0f + cosTheta * 2.0f, 0, 1);
        lerpFactor *= max(a, 0.1);
    }

    float3 skyColor = cubeMap.Sample(linearWrapSampler, pin.tex).xyz * 0.4;

    float3 finalColor = lerp(skyLuminance, skyColor * (1 - lerpFactor), saturate(pin.tex.y)) + skyLuminance * lerpFactor;

    return float4(finalColor, 1.0);
}