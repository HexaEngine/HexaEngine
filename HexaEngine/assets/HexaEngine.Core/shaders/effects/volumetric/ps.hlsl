#include "common.hlsl"

#include "directional.hlsl"
#include "directionalCascades.hlsl"
#include "point.hlsl"
#include "spot.hlsl"

float4 main(VertexOut input) : SV_TARGET
{
    float depth = max(input.pos.z, depthTex.SampleLevel(linearClampSampler, input.tex, 0));
    float3 position = GetPositionWS(input.tex, depth);
    float3 VN = GetCameraPos() - position;
    float3 V = normalize(VN);

    float ditherValue = dither(input.pos.xy);

    float3 color = 0;
    for (uint i = 0; i < lightCount; i++)
    {
        Light currentLight = lights[i].light;
        float volumetricStrength = lights[i].volumetricStrength;
        ShadowData shadow;

        [branch]
        if (currentLight.castsShadows)
        {
            shadow = shadowData[currentLight.shadowMapIndex];

            [branch]
            switch (currentLight.type)
            {
                case POINT_LIGHT:
                    color += PointLightVolumetric(ditherValue, position, VN, V, currentLight, shadow) * volumetricStrength;
                    break;
                case SPOT_LIGHT:
                    color += SpotlightVolumetric(ditherValue, position, VN, V, currentLight, shadow) * volumetricStrength;
                    break;
                case DIRECTIONAL_LIGHT:
                    color += DirectionalLightCascadedVolumetric(ditherValue, position, VN, V, currentLight, shadow) * volumetricStrength;
               /* [branch]
                if (currentLight.cascadedShadows)
                {
                    color += DirectionalLightCascadedVolumetric3(input.pos, input.tex, position, V, currentLight, shadow, volumetricStrength);
                }
                else
                {
                    color += DirectionalLightVolumetric3(input.pos, input.tex, position, V, currentLight, shadow, volumetricStrength);
                }*/
                    break;

            }
        }

    }

    return float4(color, 1);
}