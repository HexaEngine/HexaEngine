#define PI 3.14159265358979323846

float sqr(float x)
{
    return x * x;
}

float SchlickFresnel(float u)
{
    float m = clamp(1 - u, 0, 1);
    float m2 = m * m;
    return m2 * m2 * m; // pow(m,5)
}

float F_Schlick(float f0, float f90, float VoH)
{
    return f0 + (f90 - f0) * pow(1 - VoH, 5);
}

float GTR1(float NdotH, float a)
{
    if (a >= 1)
        return 1 / PI;
    float a2 = a * a;
    float t = 1 + (a2 - 1) * NdotH * NdotH;
    return (a2 - 1) / (PI * log(a2) * t);
}

float GTR2(float NdotH, float a)
{
    float a2 = a * a;
    float t = 1 + (a2 - 1) * NdotH * NdotH;
    return a2 / (PI * t * t);
}

float GTR2_aniso(float NdotH, float HdotX, float HdotY, float ax, float ay)
{
    return 1 / (PI * ax * ay * sqr(sqr(HdotX / ax) + sqr(HdotY / ay) + NdotH * NdotH));
}

float smithG_GGX(float NdotV, float alphaG)
{
    float a = alphaG * alphaG;
    float b = NdotV * NdotV;
    return 1 / (NdotV + sqrt(a + b - a * b));
}

float smithG_GGX_aniso(float NdotV, float VdotX, float VdotY, float ax, float ay)
{
    return 1 / (NdotV + sqrt(sqr(VdotX * ax) + sqr(VdotY * ay) + sqr(NdotV)));
}

float3 mon2lin(float3 x)
{
    return float3(pow(x[0], 2.2), pow(x[1], 2.2), pow(x[2], 2.2));
}

void directionOfAnisotropicity(float3 normal, out float3 tangent, out float3 binormal)
{
    tangent = cross(normal, float3(0., 1., 0.));
    binormal = normalize(cross(normal, tangent));
    tangent = normalize(cross(normal, binormal));
}

void DirectionOfAnisotropicityTan(float3 normal, float3 tangent, out float3 binormal)
{
    binormal = normalize(cross(normal, tangent));
}

float3 BRDF(
	float3 L,
	float3 V,
	float3 N,
	float3 X,
	float3 Y,
	float3 baseColor,
	float specular, float specularTint,
	float metallic, float roughness,
	float sheen, float sheenTint,
	float clearcoat, float clearcoatGloss,
	float anisotropic,
	float subsurface)
{
    float NdotL = max(dot(N, L), 0);
    float NdotV = max(dot(N, V), 0);

    if (NdotL == 0)
        return 0;
    
    float3 H = normalize(L + V);
    float NdotH = max(dot(N, H), 0);
    float LdotH = max(dot(L, H), 0);

    float3 Cdlin = mon2lin(baseColor);
    float Cdlum = .3 * Cdlin[0] + .6 * Cdlin[1] + .1 * Cdlin[2]; // luminance approx.

    float3 Ctint = Cdlum > 0 ? Cdlin / Cdlum : float3(1, 1, 1); // normalize lum. to isolate hue+sat
    float3 Cspec0 = lerp(specular * .08 * lerp(float3(1, 1, 1), Ctint, specularTint), Cdlin, metallic);
    float3 Csheen = lerp(float3(1, 1, 1), Ctint, sheenTint);

	// Diffuse fresnel - go from 1 at normal incidence to .5 at grazing
	// and lerp in diffuse retro-reflection based on roughness
    float Fd90 = 0.5 + 2 * LdotH * LdotH * roughness;
    float FL = F_Schlick(1, Fd90, NdotL);
    float FV = F_Schlick(1, Fd90, NdotV);
    float Fd = FL * FV;

	// Based on Hanrahan-Krueger brdf approximation of isotropic bssrdf
	// 1.25 scale is used to (roughly) preserve albedo
	// Fss90 used to "flatten" retroreflection based on roughness
    float Fss90 = LdotH * LdotH * roughness;
    float Fss = lerp(1.0, Fss90, FL) * lerp(1.0, Fss90, FV);
    float ss = 1.25 * (Fss * (1 / (NdotL + NdotV) - .5) + .5);

	// specular
    float aspect = sqrt(1 - anisotropic * 0.9);
    float ax = max(.001, sqr(roughness) / aspect);
    float ay = max(.001, sqr(roughness) * aspect);
    float Ds = GTR2_aniso(NdotH, dot(H, X), dot(H, Y), ax, ay);
    float FH = SchlickFresnel(LdotH);
    float3 Fs = lerp(Cspec0, float3(1, 1, 1), FH);
    float Gs;
    Gs = smithG_GGX_aniso(NdotL, dot(L, X), dot(L, Y), ax, ay);
    Gs *= smithG_GGX_aniso(NdotV, dot(V, X), dot(V, Y), ax, ay);

	// sheen
    float3 Fsheen = FH * sheen * Csheen;

	// clearcoat (ior = 1.5 -> F0 = 0.04)
    float Dr = GTR1(NdotH, lerp(.1, .001, clearcoatGloss));
    float Fr = lerp(.04, 1.0, FH);
    float Gr = smithG_GGX(NdotL, .25) * smithG_GGX(NdotV, .25);

    float3 result = ((1 / PI) * (lerp(Fd, ss, subsurface) * Cdlin) + Fsheen) * (1 - metallic) + Gs * Fs * Ds + .25 * clearcoat * Gr * Fr * Dr;

    return result * NdotL;
}

float3 FresnelSchlickRoughness(float cosTheta, float3 F0, float roughness)
{
    return F0 + (max(float3(1.0 - roughness, 1.0 - roughness, 1.0 - roughness), F0) - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

float3 specularIBL(SamplerState samplerState, TextureCube prefilterMap, Texture2D brdfLUT, float3 r, float roughness)
{
    const float MAX_REFLECTION_LOD = 12.0;
    return prefilterMap.SampleLevel(samplerState, r, roughness * MAX_REFLECTION_LOD).rgb;
}

float3 BRDFIndirect(
	SamplerState samplerState,
	TextureCube irradianceTex,
	TextureCube prefilterMap,
	Texture2D brdfLUT,
    float3 N,
    float3 V,
    float3 baseColor,
    float metallic,
    float roughness,
    float clearcoat,
    float clearcoatGloss,
    float sheen,
    float sheenTint,
    float ao,
    float anisotropy)
{
    float3 F0 = lerp(float3(0.04f, 0.04f, 0.04f), baseColor, metalness);
    
    float NdotV = max(dot(N, V), 0.0);

    float3 R = reflect(-V, N);

    float3 anisotropyDirection = float3(0., 1, 0.);
    float3 anisotropicTangent = cross(anisotropyDirection, V);
    float3 anisotropicNormal = cross(anisotropicTangent, anisotropyDirection);
    float3 bentNormal = normalize(lerp(N, anisotropicNormal, abs(anisotropy)));
    float3 bentR = reflect(-V, bentNormal);

    // specular
    float3 Cs = FresnelSchlickRoughness(NdotV, F0, roughness);
    float3 Cd = 1.0 - Cs;
    float3 Fs = specularIBL(samplerState, prefilterMap, brdfLUT, bentR, roughness);
    float2 brdf = brdfLUT.Sample(samplerState, float2(NdotV, roughness)).rg;
    Cs = Cs * brdf.x + brdf.y;
    float3 specular = Cs * Fs;

    // diffuse
    float3 irradiance = irradianceTex.Sample(samplerState, bentNormal).rgb;
    float3 Fd = irradiance * baseColor / PI;
    float3 diffuse = Cd * Fd;

    // clearcoat
    float Fc = F_Schlick(0.04, 1.0, NdotV) * clearcoat;
    float2 Fcbrdf = brdfLUT.Sample(samplerState, float2(NdotV, 1 - clearcoatGloss)).rg;
    Fc = Fc * brdf.x + brdf.y;
    diffuse *= 1.0 - Fc;
    specular *= sqr(1.0 - Fc);
    specular += specularIBL(samplerState, prefilterMap, brdfLUT, bentR, 1 - clearcoatGloss) * Fc;

    float3 result = diffuse + specular;

    return result * ao;
}