float4 main(const PS_INPUT input) : SV_TARGET
{

    float upSampledDepth = depth.Load(int3(screenCoordinates, 0)).x;

    float3 color = 0.0f.xxx;
    float totalWeight = 0.0f;

// Select the closest downscaled pixels.

    int xOffset = screenCoordinates.x % 2 == 0 ? -1 : 1;
    int yOffset = screenCoordinates.y % 2 == 0 ? -1 : 1;

    int2 offsets[] =
    {
        int2(0, 0),
int2(0, yOffset),
int2(xOffset, 0),
int2(xOffset, yOffset)
    };

    for (int i = 0; i < 4; i++)
    {

        float3 downscaledColor = volumetricLightTexture.Load(int3(downscaledCoordinates + offsets[i], 0));

        float downscaledDepth = depth.Load(int3(downscaledCoordinates, +offsets[i]1));

        float currentWeight = 1.0f;
        currentWeight *= max(0.0f, 1.0f - (0.05f) * abs(downscaledDepth - upSampledDepth));

        color += downscaledColor * currentWeight;
        totalWeight += currentWeight;

    }

    float3 volumetricLight;
    const float epsilon = 0.0001f;
    volumetricLight.xyz = color / (totalWeight + epsilon);

    return float4(volumetricLight.xyz, 1.0f);

}