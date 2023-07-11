#include "commonMath.hlsl"
#include "inputMaterial.hlsl"

struct Light
{
    float4 colorIntensity; // rgb, pre-exposed intensity
    float3 l;
    float attenuation;
    float3 worldPosition;
    float NoL;
    float3 direction;
    float zLight;
    bool castsShadows;
    bool contactShadows;
    uint type;
    int shadowIndex;
    int channels;
};

struct PixelParams
{
    float3 diffuseColor;
    float perceptualRoughness;
    float perceptualRoughnessUnclamped;
    float3 f0;
    float roughness;
    float3 dfg;
    float3 energyCompensation;

#if defined(MATERIAL_HAS_CLEAR_COAT)
    float clearCoat;
    float clearCoatPerceptualRoughness;
    float clearCoatRoughness;
#endif

#if defined(MATERIAL_HAS_SHEEN_COLOR)
    float3  sheenColor;
#if !defined(SHADING_MODEL_CLOTH)
    float sheenRoughness;
    float sheenPerceptualRoughness;
    float sheenScaling;
    float sheenDFG;
#endif
#endif

#if defined(MATERIAL_HAS_ANISOTROPY)
    float3  anisotropicT;
    float3  anisotropicB;
    float anisotropy;
#endif

#if defined(SHADING_MODEL_SUBSURFACE) || defined(MATERIAL_HAS_REFRACTION)
    float thickness;
#endif
#if defined(SHADING_MODEL_SUBSURFACE)
    float3  subsurfaceColor;
    float subsurfacePower;
#endif

#if defined(SHADING_MODEL_CLOTH) && defined(MATERIAL_HAS_SUBSURFACE_COLOR)
    float3  subsurfaceColor;
#endif

#if defined(MATERIAL_HAS_REFRACTION)
    float etaRI;
    float etaIR;
    float transmission;
    float uThickness;
    float3  absorption;
#endif
};

float computeMicroShadowing(float NoL, float visibility)
{
    // Chan 2018, "Material Advances in Call of Duty: WWII"
    float aperture = inversesqrt(1.0 - min(visibility, 0.9999));
    float microShadow = saturate(NoL * aperture);
    return microShadow * microShadow;
}

/**
 * Returns the reflected vector at the current shading point. The reflected vector
 * return by this function might be different from shading_reflected:
 * - For anisotropic material, we bend the reflection vector to simulate
 *   anisotropic indirect lighting
 * - The reflected vector may be modified to point towards the dominant specular
 *   direction to match reference renderings when the roughness increases
 */

float3 getReflectedVector(const PixelParams pixel, const float3 v, const float3 n)
{
#if defined(MATERIAL_HAS_ANISOTROPY)
    float3  anisotropyDirection = pixel.anisotropy >= 0.0 ? pixel.anisotropicB : pixel.anisotropicT;
    float3  anisotropicTangent  = cross(anisotropyDirection, v);
    float3  anisotropicNormal   = cross(anisotropicTangent, anisotropyDirection);
    float bendFactor          = abs(pixel.anisotropy) * saturate(5.0 * pixel.perceptualRoughness);
    float3  bentNormal          = normalize(mix(n, anisotropicNormal, bendFactor));

    float3 r = reflect(-v, bentNormal);
#else
    float3 r = reflect(-v, n);
#endif
    return r;
}

void getAnisotropyPixelParams(const MaterialInputs material, inout PixelParams pixel)
{
#if defined(MATERIAL_HAS_ANISOTROPY)
    float3 direction = material.anisotropyDirection;
    pixel.anisotropy = material.anisotropy;
    pixel.anisotropicT = normalize(shading_tangentToWorld * direction);
    pixel.anisotropicB = normalize(cross(getWorldGeometricNormalVector(), pixel.anisotropicT));
#endif
}