#define PI 3.141592
#define INV_PI (1 / 3.141592)

#define NumSamples 1024
#define InvNumSamples (1.0 / float(NumSamples))

TextureCube inputTexture : register(t0);
RWTexture2DArray<float4> outputTexture : register(u0);

SamplerState defaultSampler : register(s0);

// Calculate normalized sampling direction vector based on current fragment coordinates.
// This is essentially "inverse-sampling": we reconstruct what the sampling vector would be if we wanted it to "hit"
// this particular fragment in a cubemap.
float3 GetSamplingVector(uint3 ThreadID)
{
    float outputWidth, outputHeight, outputDepth;
    outputTexture.GetDimensions(outputWidth, outputHeight, outputDepth);

    float2 st = ThreadID.xy / float2(outputWidth, outputHeight);
    float2 uv = 2.0 * float2(st.x, 1.0 - st.y) - 1.0;

	// Select vector based on cubemap face index.
    float3 ret;
    switch (ThreadID.z)
    {
        case 0:
            ret = float3(1.0, uv.y, -uv.x);
            break;
        case 1:
            ret = float3(-1.0, uv.y, uv.x);
            break;
        case 2:
            ret = float3(uv.x, 1.0, -uv.y);
            break;
        case 3:
            ret = float3(uv.x, -1.0, uv.y);
            break;
        case 4:
            ret = float3(uv.x, uv.y, 1.0);
            break;
        case 5:
            ret = float3(-uv.x, uv.y, -1.0);
            break;
    }
    return normalize(ret);
}

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

[numthreads(32, 32, 1)]
void main(uint3 ThreadID : SV_DispatchThreadID)
{
    float3 N = GetSamplingVector(ThreadID);

    float3 importance = ImportanceSample(N);

    outputTexture[ThreadID] = float4(importance, 1);
}