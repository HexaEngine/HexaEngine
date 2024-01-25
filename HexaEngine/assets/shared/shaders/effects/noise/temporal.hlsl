#include "../../camera.hlsl"

cbuffer TemporalNoiseCB : register(b0)
{
    float scale;
    float3 padd;
};

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

//a random texture generator, but you can also use a pre-computed perturbation texture
float4 rnm(in float2 tc)
{
    float noise = sin(dot(tc + float2(cumulativeTime, cumulativeTime), float2(12.9898, 78.233))) * 43758.5453;

    float noiseR = frac(noise) * 2.0 - 1.0;
    float noiseG = frac(noise * 1.2154) * 2.0 - 1.0;
    float noiseB = frac(noise * 1.3453) * 2.0 - 1.0;
    float noiseA = frac(noise * 1.3647) * 2.0 - 1.0;

    return float4(noiseR, noiseG, noiseB, noiseA);
}

float fade(in float t)
{
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

#define permTexUnit (1.0/256.0)		// Perm texture texel-size
#define permTexUnitHalf (0.5/256.0)	// Half perm texture texel-size

float pnoise3D(in float3 p)
{
    float3 pi = permTexUnit * floor(p) + permTexUnitHalf; // Integer part, scaled so +1 moves permTexUnit texel
	// and offset 1/2 texel to sample texel centers
    float3 pf = frac(p); // Fractional part for interpolation

	// Noise contributions from (x=0, y=0), z=0 and z=1
    float perm00 = rnm(pi.xy).a;
    float3 grad000 = rnm(float2(perm00, pi.z)).rgb * 4.0 - 1.0;
    float n000 = dot(grad000, pf);
    float3 grad001 = rnm(float2(perm00, pi.z + permTexUnit)).rgb * 4.0 - 1.0;
    float n001 = dot(grad001, pf - float3(0.0, 0.0, 1.0));

	// Noise contributions from (x=0, y=1), z=0 and z=1
    float perm01 = rnm(pi.xy + float2(0.0, permTexUnit)).a;
    float3 grad010 = rnm(float2(perm01, pi.z)).rgb * 4.0 - 1.0;
    float n010 = dot(grad010, pf - float3(0.0, 1.0, 0.0));
    float3 grad011 = rnm(float2(perm01, pi.z + permTexUnit)).rgb * 4.0 - 1.0;
    float n011 = dot(grad011, pf - float3(0.0, 1.0, 1.0));

	// Noise contributions from (x=1, y=0), z=0 and z=1
    float perm10 = rnm(pi.xy + float2(permTexUnit, 0.0)).a;
    float3 grad100 = rnm(float2(perm10, pi.z)).rgb * 4.0 - 1.0;
    float n100 = dot(grad100, pf - float3(1.0, 0.0, 0.0));
    float3 grad101 = rnm(float2(perm10, pi.z + permTexUnit)).rgb * 4.0 - 1.0;
    float n101 = dot(grad101, pf - float3(1.0, 0.0, 1.0));

	// Noise contributions from (x=1, y=1), z=0 and z=1
    float perm11 = rnm(pi.xy + float2(permTexUnit, permTexUnit)).a;
    float3 grad110 = rnm(float2(perm11, pi.z)).rgb * 4.0 - 1.0;
    float n110 = dot(grad110, pf - float3(1.0, 1.0, 0.0));
    float3 grad111 = rnm(float2(perm11, pi.z + permTexUnit)).rgb * 4.0 - 1.0;
    float n111 = dot(grad111, pf - float3(1.0, 1.0, 1.0));

	// Blend contributions along x
    float4 n_x = lerp(float4(n000, n001, n010, n011), float4(n100, n101, n110, n111), fade(pf.x));

	// Blend contributions along y
    float2 n_xy = lerp(n_x.xy, n_x.zw, fade(pf.y));

	// Blend contributions along z
    float n_xyz = lerp(n_xy.x, n_xy.y, fade(pf.z));

	// We're done, return the final noise value.
    return n_xyz;
}

//2d coordinate orientation thing
float2 coordRot(in float2 tc, in float angle)
{
    float aspect = screenDim.x / screenDim.y;
    float rotX = ((tc.x * 2.0 - 1.0) * aspect * cos(angle)) - ((tc.y * 2.0 - 1.0) * sin(angle));
    float rotY = ((tc.y * 2.0 - 1.0) * cos(angle)) + ((tc.x * 2.0 - 1.0) * aspect * sin(angle));
    rotX = ((rotX / aspect) * 0.5 + 0.5);
    rotY = rotY * 0.5 + 0.5;
    return float2(rotX, rotY);
}

float main(VSOut input) : SV_Target
{
    float2 texCoord = input.Tex;

    float x = (texCoord.x + 4.0) * (texCoord.y + 4.0) * (cumulativeTime * 10.0);
    float grain = (fmod((fmod(x, 13.0) + 1.0) * (fmod(x, 123.0) + 1.0), 0.01) - 0.005) * scale;

    return grain;
}