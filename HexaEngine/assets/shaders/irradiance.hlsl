float3 IrradianceConvolution(TextureCube environmentMap, SamplerState samplerState, float3 localPos, float3 N)
{
    const float PI = 3.14159265359;
    float3 normal = normalize(localPos);
    
    float3 irradiance = float3(0.0);

    float3 up = float3(0.0, 1.0, 0.0);
    float3 right = normalize(cross(up, normal));
    up = normalize(cross(normal, right));

    float sampleDelta = 0.025;
    float nrSamples = 0.0;
    for (float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
    {
        for (float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
        {
            // spherical to cartesian (in tangent space)
            float3 tangentSample = float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
            
            // tangent space to world
            float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;
          
            irradiance += environmentMap.Sample(samplerState, sampleVec).rgb * cos(theta) * sin(theta);
            nrSamples++;
        }
    }
    return PI * irradiance * (1.0 / float(nrSamples));
}