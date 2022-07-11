struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};


cbuffer mvp : register(b0)
{
    matrix view;
    matrix viewInv;
    matrix projection;
    matrix projectionInv;
};

static const float g_FarPlaneDist = 100;

static const int g_maxBinarySearchStep = 40;
static const int g_maxRayStep = 70;
static const float g_depthbias = 0.00001f;
static const float g_rayStepScale = 1.05f;
static const float g_maxThickness = 1.8f;
static const float g_maxRayLength = 200.f;

Texture2D colorTexture : register(t0);
Texture2D positionTexture : register(t1);
Texture2D normalTexture : register(t2);

SamplerState samplerState;

float Noise(float2 seed)
{
    return frac(sin(dot(seed.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float3 GetTexCoordXYLinearDepthZ(float3 viewPos)
{
    float4 projPos = mul(float4(viewPos, 1.f), projection);
    projPos.xy /= projPos.w;
    projPos.xy = projPos.xy * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
    projPos.z = viewPos.z / 100;
    return projPos.xyz;
}

float4 BinarySearch(float3 dir, float3 viewPos)
{
    float3 texCoord = float3(0.f, 0.f, 0.f);
    float srcdepth = 0.f;
    float depthDiff = 0.f;

	[loop]
    for (int i = 0; i < g_maxBinarySearchStep; ++i)
    {
        texCoord = GetTexCoordXYLinearDepthZ(viewPos);
        srcdepth = positionTexture.SampleLevel(samplerState, texCoord.xy, 0).w;
        depthDiff = srcdepth.x - texCoord.z;

        if (depthDiff > 0.f)
        {
            viewPos += dir;
            dir *= 0.5f;
        }

        viewPos -= dir;
    }

    texCoord = GetTexCoordXYLinearDepthZ(viewPos);
    srcdepth = positionTexture.SampleLevel(samplerState, texCoord.xy, 0).w;
    depthDiff = abs(srcdepth - texCoord.z);
    float4 result = float4(0.f, 0.f, 0.f, 0.f);
    if (texCoord.z < 0.9999f && depthDiff < g_depthbias)
    {
        result = colorTexture.SampleLevel(samplerState, texCoord.xy, 0);
    }

    return result;
}


float4 main(VSOut input) : SV_TARGET
{
    float4 gpos = positionTexture.SampleLevel(samplerState, input.Tex, 0);
    float4 gnormal = normalTexture.SampleLevel(samplerState, input.Tex, 0);


    if (gpos.w == 0)
        return float4(0, 0, 0, 0);

    if (gnormal.w == 1)
        return float4(0, 0, 0, 0);

    float4 pos = float4(gpos.xyz, 1);
    float3 viewPos = mul(pos, view).xyz;

    float3 normal = mul(gnormal.xyz, (float3x3) view);

    float3 incidentVec = normalize(viewPos);
    float3 viewNormal = normalize(normal);

    float3 reflectVec = reflect(incidentVec, viewNormal);
    reflectVec = normalize(reflectVec);
    reflectVec *= g_rayStepScale;

    float3 reflectPos = viewPos;

    float thickness = g_maxThickness;

	[loop]
    for (int i = 0; i < g_maxRayStep; ++i)
    {
        float3 texCoord = GetTexCoordXYLinearDepthZ(reflectPos);
        float srcdepth = positionTexture.SampleLevel(samplerState, texCoord.xy, 0).w;

        float depthDiff = texCoord.z - srcdepth;
        if (depthDiff > g_depthbias && depthDiff < thickness)
        {
            float4 reflectColor = BinarySearch(reflectVec, reflectPos);

            float edgeFade = 1.f - pow(length(texCoord.xy - 0.5f) * 2.f, 2.f);
            reflectColor.a *= pow(0.75f, (length(reflectPos - viewPos) / g_maxRayLength)) * edgeFade;
            return reflectColor;
        }
        else
        {
            reflectPos += (i + Noise(texCoord.xy)) * reflectVec;
        }
    }

    return float4(0.f, 0, 0, 0.f);
}
