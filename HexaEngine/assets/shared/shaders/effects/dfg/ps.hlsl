struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

#ifndef WIDTH
#define WIDTH 128
#endif

#ifndef HEIGHT
#define HEIGHT 128
#endif

#ifndef MULTISCATTER
#define MULTISCATTER 0
#endif

#ifndef CLOTH
#define CLOTH 0
#endif

#define F_PI 3.14159265359

float pow5(float x)
{
    const float x2 = x * x;
    return x2 * x2 * x;
}

float pow6(float x)
{
    const float x2 = x * x;
    return x2 * x2 * x2;
}

float3 hemisphereImportanceSampleDggx(float2 u, float a)
{
    // pdf = D(a) * cosTheta
    const float phi = 2.0f * F_PI * u.x;
    // NOTE: (aa-1) == (a-1)(a+1) produces better fp accuracy
    const float cosTheta2 = (1 - u.y) / (1 + (a + 1) * ((a - 1) * u.y));
    const float cosTheta = sqrt(cosTheta2);
    const float sinTheta = sqrt(1 - cosTheta2);
    return float3(sinTheta * cos(phi), sinTheta * sin(phi), cosTheta);
}

float3 hemisphereCosSample(float2 u)
{
    // pdf = cosTheta / F_PI;
    const float phi = 2.0f * (float) F_PI * u.x;
    const float cosTheta2 = 1 - u.y;
    const float cosTheta = sqrt(cosTheta2);
    const float sinTheta = sqrt(1 - cosTheta2);
    return float3(sinTheta * cos(phi), sinTheta * sin(phi), cosTheta);
}

float3 hemisphereUniformSample(float2 u)
{
    // pdf = 1.0 / (2.0 * F_PI);
    const float phi = 2.0f * (float) F_PI * u.x;
    const float cosTheta = 1 - u.y;
    const float sinTheta = sqrt(1 - cosTheta * cosTheta);
    return float3(sinTheta * cos(phi), sinTheta * sin(phi), cosTheta);
}

float2 hammersley(uint i, float iN)
{
    const float tof = 0.5f / 0x80000000U;
    uint bits = i;
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float2(i * iN, bits * tof);
}

float Visibility(float NoV, float NoL, float a)
{
    // Heitz 2014, "Understanding the Masking-Shadowing Function in Microfacet-Based BRDFs"
    // Height-correlated GGX
    const float a2 = a * a;
    const float GGXL = NoV * sqrt((NoL - NoL * a2) * NoL + a2);
    const float GGXV = NoL * sqrt((NoV - NoV * a2) * NoV + a2);
    return 0.5f / (GGXV + GGXL);
}

float VisibilityAshikhmin(float NoV, float NoL, float a)
{
    // Neubelt and Pettineo 2013, "Crafting a Next-gen Material Pipeline for The Order: 1886"
    return 1 / (4 * (NoL + NoV - NoL * NoV));
}

float DistributionCharlie(float NoH, float linearRoughness)
{
    // Estevez and Kulla 2017, "Production Friendly Microfacet Sheen BRDF"
    float a = linearRoughness;
    float invAlpha = 1 / a;
    float cos2h = NoH * NoH;
    float sin2h = 1 - cos2h;
    return (2.0f + invAlpha) * pow(sin2h, invAlpha * 0.5f) / (2.0f * (float) F_PI);
}

float DFV_Charlie_Uniform(float NoV, float linearRoughness, uint numSamples)
{
    float r = 0.0;
    const float3 V = float3(sqrt(1 - NoV * NoV), 0, NoV);
    for (uint i = 0; i < numSamples; i++)
    {
        const float2 u = hammersley(i, 1.0f / numSamples);
        const float3 H = hemisphereUniformSample(u);
        const float3 L = 2 * dot(V, H) * H - V;
        const float VoH = saturate(dot(V, H));
        const float NoL = saturate(L.z);
        const float NoH = saturate(H.z);
        if (NoL > 0)
        {
            const float v = VisibilityAshikhmin(NoV, NoL, linearRoughness);
            const float d = DistributionCharlie(NoH, linearRoughness);
            r += v * d * NoL * VoH; // VoH comes from the Jacobian, 1/(4*VoH)
        }
    }
    // uniform sampling, the PDF is 1/2pi, 4 comes from the Jacobian
    return r * (4.0f * 2.0f * (float) F_PI / numSamples);
}

static float2 DFV(float NoV, float linearRoughness, uint numSamples)
{
    float2 r = 0;
    const float3 V = float3(sqrt(1 - NoV * NoV), 0, NoV);
    for (uint i = 0; i < numSamples; i++)
    {
        const float2 u = hammersley(i, 1.0f / numSamples);
        const float3 H = hemisphereImportanceSampleDggx(u, linearRoughness);
        const float3 L = 2 * dot(V, H) * H - V;
        const float VoH = saturate(dot(V, H));
        const float NoL = saturate(L.z);
        const float NoH = saturate(H.z);
        if (NoL > 0)
        {
            /*
             * Fc = (1 - V*H)^5
             * F(h) = f0*(1 - Fc) + f90*Fc
             *
             * f0 and f90 are known at runtime, but thankfully can be factored out, allowing us
             * to split the integral in two terms and store both terms separately in a LUT.
             *
             * At runtime, we can reconstruct Er() exactly as below:
             *
             *            4                      <v*h>
             *   DFV.x = --- Sum (1 - Fc) V(v, l) ------- <n*l>
             *            N  h                   <n*h>
             *
             *
             *            4                      <v*h>
             *   DFV.y = --- Sum (    Fc) V(v, l) ------- <n*l>
             *            N  h                   <n*h>
             *
             *
             *   Er() = f0 * DFV.x + f90 * DFV.y
             *
             */
            const float v = Visibility(NoV, NoL, linearRoughness) * NoL * (VoH / NoH);
            const float Fc = pow5(1 - VoH);
            r.x += v * (1.0f - Fc);
            r.y += v * Fc;
        }
    }
    return r * (4.0f / numSamples);
}

static float2 DFV_Multiscatter(float NoV, float linearRoughness, uint numSamples)
{
    float2 r = 0;
    const float3 V = float3(sqrt(1 - NoV * NoV), 0, NoV);
    for (uint i = 0; i < numSamples; i++)
    {
        const float2 u = hammersley(i, 1.0f / numSamples);
        const float3 H = hemisphereImportanceSampleDggx(u, linearRoughness);
        const float3 L = 2 * dot(V, H) * H - V;
        const float VoH = saturate(dot(V, H));
        const float NoL = saturate(L.z);
        const float NoH = saturate(H.z);
        if (NoL > 0)
        {
            const float v = Visibility(NoV, NoL, linearRoughness) * NoL * (VoH / NoH);
            const float Fc = pow5(1 - VoH);
            /*
             * Assuming f90 = 1
             *   Fc = (1 - V*H)^5
             *   F(h) = f0*(1 - Fc) + Fc
             *
             * f0 and f90 are known at runtime, but thankfully can be factored out, allowing us
             * to split the integral in two terms and store both terms separately in a LUT.
             *
             * At runtime, we can reconstruct Er() exactly as below:
             *
             *            4                <v*h>
             *   DFV.x = --- Sum Fc V(v, l) ------- <n*l>
             *            N  h             <n*h>
             *
             *
             *            4                <v*h>
             *   DFV.y = --- Sum    V(v, l) ------- <n*l>
             *            N  h             <n*h>
             *
             *
             *   Er() = (1 - f0) * DFV.x + f0 * DFV.y
             *
             *        = mix(DFV.xxx, DFV.yyy, f0)
             *
             */
            r.x += v * Fc;
            r.y += v;
        }
    }
    return r * (4.0f / numSamples);
}

float3 DFG(float NdotV, float linear_roughness)
{
    float3 result = 0;
#if (MULTISCATTER)
    result.rg = DFV_Multiscatter(NdotV, linear_roughness, 1024);
#else
    result.rg = DFV(NdotV, linear_roughness, 1024);
#endif
#if (CLOTH)
    result.b = DFV_Charlie_Uniform(NdotV, linear_roughness, 4096);
#endif

    return result;
}

float4 main(VSOut output) : SV_Target
{
    const float h = (float) HEIGHT;
    float x = output.Tex.x * WIDTH;
    float y = output.Tex.y * HEIGHT;
    const float coord = saturate((h - y + 0.5f) / h);
    const float linear_roughness = coord * coord;
    const float NoV = saturate((x + 0.5f) / WIDTH);
    float3 dfg = DFG(NoV, linear_roughness);
    return float4(dfg, 1);
}