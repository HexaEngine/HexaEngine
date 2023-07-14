struct VSOut
{
    float4 Pos : SV_Position;
    float3 WorldPos : TEXCOORD;
};

#define PI 3.141592
#define INV_PI (1 / 3.141592)

#define NumSamples 1024
#define InvNumSamples (1.0 / float(NumSamples))

cbuffer ParamsBuffer
{
    float roughness;
};

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

// Base on GGX example in:                                                            //
// http://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf
float3 importanceSampleGGX(float2 Xi, float roughness, float3 N)
{
    float a = roughness * roughness;

    float Phi = 2 * PI * Xi.x;
    float CosTheta = sqrt((1 - Xi.y) / (1 + (a * a - 1) * Xi.y));
    float SinTheta = sqrt(1 - CosTheta * CosTheta);

    float3 H;
    H.x = SinTheta * cos(Phi);
    H.y = SinTheta * sin(Phi);
    H.z = CosTheta;

    float3 UpVector = abs(N.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
    float3 TangentX = normalize(cross(UpVector, N));
    float3 TangentY = cross(N, TangentX);

    return TangentX * H.x + TangentY * H.y + N * H.z;
}

// D(h) for GGX.
// http://graphicrants.blogspot.com/2013/08/specular-brdf-reference.html
float specularD(float roughness, float NoH)
{
    float r2 = roughness * roughness;
    float NoH2 = NoH * NoH;
    float a = 1.0 / (3.14159 * r2 * pow(NoH, 4));
    float b = exp((NoH2 - 1) / r2 * NoH2);
    return a * b;
}

float3 ImportanceSample(float3 R)
{
    float3 N = R;
    float3 V = R;
    float3 Li = 0;

    uint width, height;
    inputTexture.GetDimensions(width, height);

    uint sampleCount = 0;
    for (uint sampleIndex = 0; sampleIndex < NumSamples; sampleIndex++)
    {
        float2 Xi = Hammersley(sampleIndex, NumSamples);
        float3 H = importanceSampleGGX(Xi, roughness, N);
        float3 L = 2 * dot(V, H) * H - V;
        float NdotL = max(dot(N, L), 0);
        float VdotL = max(dot(V, L), 0);
        float NdotH = max(dot(N, H), 0);
        float VdotH = max(dot(V, H), 0);

        if (NdotL > 0)
        {
            //
            // Compute pdf of BRDF
            // Taken from Epic's Siggraph 2013 Lecture:
            // http://blog.selfshadow.com/publications/s2013-shading-course/karis/s2013_pbs_epic_notes_v2.pdf
            //
            float Dh = specularD(roughness, NdotH);
            float pdf = Dh * NdotH / (4 * VdotH);
            float solidAngleTexel = 4 * PI / (6 * width * height);
            float solidAngleSample = 1.0 / (NumSamples * pdf);
            float lod = roughness == 0 ? 0 : 0.5 * log2((float) (solidAngleSample / solidAngleTexel));

            Li += inputTexture.SampleLevel(defaultSampler, L, lod).rgb;
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

    float3 importance = ImportanceSample(N);

    return float4(importance, 1);
}