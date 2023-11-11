#include "common.hlsl"
#include "../../camera.hlsl"
#include "../../shadow.hlsl"
#include "../../math.hlsl"

#include "directional.hlsl"
#include "directionalCascades.hlsl"
#include "point.hlsl"
#include "spot.hlsl"

float4 main(VertexOut input) : SV_TARGET
{
    float depth = max(input.pos.z, depthTx.SampleLevel(linear_clamp_sampler, input.tex, 0));
    float3 position = GetPositionWS(input.tex, depth);
    float3 V = normalize(GetCameraPos() - position);

    float3 color = 0;
    for (uint i = 0; i < lightCount; i++)
    {
        Light currentLight = lights[i].light;
        float volumetricStrength = lights[i].volumetricStrength;
        ShadowData shadow;

        if (currentLight.castsShadows)
        {
            shadow = shadowData[currentLight.shadowMapIndex];
        }

        [branch]
        switch (currentLight.type)
        {
            case POINT_LIGHT:
                color += PointLightVolumetric3(input.pos, input.tex, position, V, currentLight, shadow, volumetricStrength);
                break;
            case SPOT_LIGHT:
                color += SpotlightVolumetric3(input.pos, input.tex, position, V, currentLight, shadow, volumetricStrength);
                break;
            case DIRECTIONAL_LIGHT:
                [branch]
                if (currentLight.cascadedShadows)
                {
                    color += DirectionalLightCascadedVolumetric3(input.pos, input.tex, position, V, currentLight, shadow, volumetricStrength);
                }
                else
                {
                    color += DirectionalLightVolumetric3(input.pos, input.tex, position, V, currentLight, shadow, volumetricStrength);
                }
                break;

        }

    }

    return float4(color, 1);
}