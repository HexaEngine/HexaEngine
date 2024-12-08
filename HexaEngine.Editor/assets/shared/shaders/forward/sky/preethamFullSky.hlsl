// Based on "A Practical Analytic Model for Daylight" aka The Preetham Model, the de facto standard analytic skydome model
// http://www.cs.utah.edu/~shirley/papers/sunsky/sunsky.pdf
// Original implementation by Simon Wallner: http://www.simonwallner.at/projects/atmospheric-scattering
// Improved by Martin Upitis: http://blenderartists.org/forum/showthread.php?245954-preethams-sky-impementation-HDR
// Three.js integration by zz85: http://twitter.com/blurspline / https://github.com/zz85 / http://threejs.org/examples/webgl_shaders_sky.html
// Additional uniforms, refactoring and integrated with editable sky example: https://twitter.com/Sam_Twidale / https://github.com/Tw1ddle/Sky-Particles-Shader

#include "../../weather.hlsl"
#include "../../camera.hlsl"

cbuffer PreethamSkyBuffer : register(b0)
{
    float3 primaries;
    float depolarizationFactor;
    float3 mieKCoefficient;
    float mieCoefficient;
    float3 sunPosition;
    float mieDirectionalG;
    float mieV;
    float mieZenithLength;
    float numMolecules;
    float rayleigh;
    float rayleighZenithLength;
    float refractiveIndex;
    float sunAngularDiameterDegrees;
    float sunIntensityFactor;
    float sunIntensityFalloffSteepness;
    float turbidity;
    float luminance;
    float tonemapWeighting;
};

struct VertexOut
{
    float4 position : SV_POSITION;
    float3 pos : POSITION;
    float3 tex : TEXCOORD;
};

#define PI 3.141592653589793238462643383279502884197169
#define UP float3(0.0, 1.0, 0.0)

float3 totalRayleigh(float3 lambda)
{
    return (8.0 * pow(PI, 3.0) * pow(pow(refractiveIndex, 2.0) - 1.0, 2.0) * (6.0 + 3.0 * depolarizationFactor)) / (3.0 * numMolecules * pow(lambda, 4.0) * (6.0 - 7.0 * depolarizationFactor));
}

float3 totalMie(float3 lambda, float3 K, float T)
{
    float c = 0.2 * T * 10e-18;
    return 0.434 * c * PI * pow((2.0 * PI) / lambda, mieV - 2.0) * K;
}

float rayleighPhase(float cosTheta)
{
    return (3.0 / (16.0 * PI)) * (1.0 + pow(cosTheta, 2.0));
}

float henyeyGreensteinPhase(float cosTheta, float g)
{
    return (1.0 / (4.0 * PI)) * ((1.0 - pow(g, 2.0)) / pow(1.0 - 2.0 * g * cosTheta + pow(g, 2.0), 1.5));
}

float sunIntensity(float zenithAngleCos)
{
    float cutoffAngle = PI / 1.95; // Earth shadow hack
    return sunIntensityFactor * max(0.0, 1.0 - exp(-((cutoffAngle - acos(zenithAngleCos)) / sunIntensityFalloffSteepness)));
}

// Whitescale tonemapping calculation, see http://filmicgames.com/archives/75
// Also see http://blenderartists.org/forum/showthread.php?321110-Shaders-and-Skybox-madness
float3 Uncharted2Tonemap(float3 W)
{
    const float A = 0.15; // Shoulder strength
    const float B = 0.50; // Linear strength
    const float C = 0.10; // Linear angle
    const float D = 0.20; // Toe strength
    const float E = 0.02; // Toe numerator
    const float F = 0.30; // Toe denominator
    return ((W * (A * W + C * B) + D * E) / (W * (A * W + B) + D * F)) - E / F;
}

float4 main(VertexOut pin) : SV_TARGET
{
    float3 dir = normalize(pin.pos);

    // Rayleigh coefficient
    float sunfade = 1.0 - clamp(1.0 - exp((sunPosition.y / 450000.0)), 0.0, 1.0);
    float rayleighCoefficient = rayleigh - (1.0 * (1.0 - sunfade));
    float3 betaR = totalRayleigh(primaries) * rayleighCoefficient;

	// Mie coefficient
    float3 betaM = totalMie(primaries, mieKCoefficient, turbidity) * mieCoefficient;

	// Optical length, cutoff angle at 90 to avoid singularity
    float zenithAngle = acos(max(0.0, dot(UP,
    normalize(pin.pos - camPos))));
    float denom = cos(zenithAngle) + 0.15 * pow(93.885 - ((zenithAngle * 180.0) / PI), -1.253);
    float sR = rayleighZenithLength / denom;
    float sM = mieZenithLength / denom;

	// Combined extinction factor
    float3 Fex = exp(-(betaR * sR + betaM * sM));

	// In-scattering
    float3 sunDirection = normalize(light_dir.xyz);
    float cosTheta = dot(normalize(pin.pos - camPos), sunDirection);
    float3 betaRTheta = betaR * rayleighPhase(cosTheta * 0.5 + 0.5);
    float3 betaMTheta = betaM * henyeyGreensteinPhase(cosTheta, mieDirectionalG);
    float sunE = sunIntensity(dot(sunDirection, UP));
    float3 Lin = pow(sunE * ((betaRTheta + betaMTheta) / (betaR + betaM)) * (1.0 - Fex), 1.5);
    Lin *= lerp(1.0, pow(sunE * ((betaRTheta + betaMTheta) / (betaR + betaM)) * Fex, 0.5), clamp(pow(1.0 - dot(UP,
    sunDirection),
    5.0),
    0.0, 1.0));

	// Composition + solar disc
    float sunAngularDiameterCos = cos(sunAngularDiameterDegrees);
    float sundisk = smoothstep(sunAngularDiameterCos, sunAngularDiameterCos + 0.00002, cosTheta);
    float3 L0 = 0.1 * Fex;
    L0 += sunE * 19000.0 * Fex * sundisk;
    float3 texColor = Lin + L0;
    texColor *= 0.04;
    texColor += float3(0.0, 0.001, 0.0025) * 0.3;

    // Tonemapping
    float3 whiteScale = 1.0 / Uncharted2Tonemap(tonemapWeighting);
    float3 curr = Uncharted2Tonemap((log2(2.0 / pow(luminance, 4.0))) * texColor);
    float3 color = curr * whiteScale;
    float3 retColor = pow(color, (1.0 / (1.2 + (1.2 * sunfade))));

    return float4(retColor, 1.0);
}