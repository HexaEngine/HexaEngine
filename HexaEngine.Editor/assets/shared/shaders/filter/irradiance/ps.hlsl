struct VSOut
{
    float4 Pos : SV_POSITION;
    float3 WorldPos : TEXCOORD0;
};

#define PI 3.141592
#define INV_PI (1 / 3.141592)

#define NumSamples 1024
#define InvNumSamples (1.0f / float(1024f))

TextureCube inputTexture : register(t0);
SamplerState defaultSampler : register(s0);

// ----------------------------------------------------------------------------
// http://holger.dammertz.org/stuff/notes_HammersleyOnHemisphere.html
// efficient VanDerCorpus calculation.
float RadicalInverse_VdC(uint bits)
{
    bits = (bits << 16u) | (bits >> 16u);
    bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
    bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
    bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
    bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
    return float(bits) * 2.3283064365386963e-10; // / 0x100000000
}
// ----------------------------------------------------------------------------
float2 Hammersley(uint i, uint N)
{
    return float2(float(i) / float(N), RadicalInverse_VdC(i));
}

//
// Derived from GGX example in:
// http://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf
// Image Based Lighting.
//
float3 importanceSampleDiffuse(float2 Xi, float3 N)
{
    float CosTheta = 1.0 - Xi.y;
    float SinTheta = sqrt(1.0 - CosTheta * CosTheta);
    float Phi = 2 * PI * Xi.x;

    float3 H;
    H.x = SinTheta * cos(Phi);
    H.y = SinTheta * sin(Phi);
    H.z = CosTheta;

    float3 UpVector = abs(N.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
    float3 TangentX = normalize(cross(UpVector, N));
    float3 TangentY = cross(N, TangentX);

    return TangentX * H.x + TangentY * H.y + N * H.z;
}

float3 ImportanceSample(float3 N)
{
    float3 V = N;
    float3 Li = 0;
    uint width, height;
    inputTexture.GetDimensions(width, height);

    uint sampleCount;
    for (uint sampleIndex = 0; sampleIndex < NumSamples; sampleIndex++)
    {
        float2 Xi = Hammersley(sampleIndex, NumSamples);
        float3 H = importanceSampleDiffuse(Xi, N);
        float3 L = normalize(2 * dot(V, H) * H - V);
        float NdotL = saturate(dot(N, L));
        if (NdotL > 0)
        {
            float pdf = max(0.0, NdotL * INV_PI);
            float solidAngleTexel = 4 * PI / (6 * width * height);
            float solidAngleSample = 1.0 / (NumSamples * pdf);
            float lod = 0.5 * log2((float) (solidAngleSample / solidAngleTexel));

            Li += inputTexture.SampleLevel(defaultSampler, H, lod).rgb;
            sampleCount++;
        }
    }

    if (sampleCount == 0)
    {
        return Li;
    }
    else
    {
        return Li / float(sampleCount);
    }
}

float4 main(VSOut vs) : SV_Target
{
    float3 N = normalize(vs.WorldPos);

    float3 irradiance = ImportanceSample(N);

    return float4(irradiance, 1.0);
}