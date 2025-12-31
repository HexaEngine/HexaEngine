#include "HexaEngine.Core:shaders/common.hlsl"

Texture2D inputTex;
Texture2D<float> depthTex;
Texture2D normalTex;

Texture2D velocityBufferTex;
Texture2D prevTex;


SamplerState linearClampSampler;
SamplerState pointClampSampler;

cbuffer SSGIParams : register(b0)
{
	float intensity;
	float distance;
	int SSGI_RAY_COUNT;
	int SSGI_RAY_STEPS;
	float SSGI_RAY_STEP;
	float SSGI_DEPTH_BIAS;
};

float3 GetViewPos(float2 coord)
{
	float depth = depthTex.Sample(linearClampSampler, coord).r;
	return GetPositionVS(coord, depth);
}

float3 GetViewNormal(float2 coord)
{
	float3 normal = normalTex.Sample(linearClampSampler, coord).rgb;
	return normalize(mul(UnpackNormal(normal), (float3x3)view));
}

float lenSq(float3 v)
{
	return pow(v.x, 2.0) + pow(v.y, 2.0) + pow(v.z, 2.0);
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

float3 SampleCosineHemisphere(float3 n, float2 u)
{
    float phi = 2.0 * PI * u.x;
    float cosTheta = sqrt(1.0 - u.y);
    float sinTheta = sqrt(u.y);

    float3 local = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);

    float3 up = abs(n.z) < 0.999 ? float3(0, 0, 1) : float3(1, 0, 0);
    float3 t = normalize(cross(up, n));
    float3 b = cross(n, t);
    return t * local.x + b * local.y + n * local.z;
}

uint Hash(uint x)
{
    x ^= x >> 16u;
    x *= 0x7feb352du;
    x ^= x >> 15u;
    x *= 0x846ca68bu;
    x ^= x >> 16u;
    return x;
}

float3 ComputeScreenSpaceGI(float2 uv, float3 viewPos, float3 normal)
{
    float thickness = 0.05;
    float falloff = 1.0 / distance;
    float3 accumulatedGI = 0;
    int validSamples = 0;
    uint pixelIndex = asuint(uv.x * 65536) + asuint(uv.y * 65536) * 65536;
	
	[loop]
    for (int ray = 0; ray < SSGI_RAY_COUNT; ray++)
    {
        float t = 0.0f;

		[loop]
        for (int step = 0; step < SSGI_RAY_STEPS; step++)
        {
            t += SSGI_RAY_STEP;

            uint seed = pixelIndex * 9781u + frame * 6271u + ray * 26699u + step * 31847u;
            float2 xi = float2(RadicalInverse_VdC(Hash(seed)), RadicalInverse_VdC(Hash(seed ^ 0xA511E9B3u)));
            float3 rayDir = SampleCosineHemisphere(normal, xi);
            
            float3 p = viewPos + rayDir * t;
            float2 suv = ProjectUV(p);
            if (any(suv < 0) || any(suv > 1))
                break;

            float sceneDepth = depthTex.Sample(pointClampSampler, suv);
            if (sceneDepth >= 1.0)
                break;

            float3 scenePos = GetPositionVS(suv, sceneDepth);

            if (abs(p.z - scenePos.z) < thickness)
            {
                float3 lightColor = inputTex.Sample(linearClampSampler, suv).rgb;
                float3 lightNormal = GetViewNormal(suv);

                float NoL = saturate(dot(normal, rayDir));
                float LoN = saturate(dot(-rayDir, lightNormal));

                float weight = NoL * LoN * exp(-t * falloff);

                accumulatedGI += lightColor * weight;
                validSamples++;
                break;
            }
        }
    }

    return (validSamples > 0) ? accumulatedGI / validSamples : 0;
}

struct VertexOut
{
	float4 PosH : SV_POSITION;
	float2 Tex : TEXCOORD;
};

float4 main(VertexOut pin) : SV_Target
{
	float2 uv = pin.Tex;
	float depth = depthTex.Sample(linearClampSampler, uv);
	float3 position = GetPositionVS(uv, depth);
	float3 normal = GetViewNormal(uv);
	float3 gi = ComputeScreenSpaceGI(uv, position, normal);
    
    float2 velocity = velocityBufferTex.SampleLevel(linearClampSampler, uv, 0).xy;
    float2 prevUV = uv - velocity;
    bool validUV = all(prevUV > 0.0f) && all(prevUV < 1.0f);
    
    
    const float depthThreshold = 0.02f;
    float depthPrev = depthTex.SampleLevel(linearClampSampler, prevUV, 0);
    bool depthValid = abs(depth - depthPrev) < depthThreshold;
    float movementFactor = max(saturate(length(velocity)), 0.05f);
    
  
    float blendFactor = (validUV && depthValid) ? movementFactor : 1.0f;

    float3 historyGi = prevTex.Sample(linearClampSampler, prevUV).rgb;
    float3 finalGi = saturate(lerp(historyGi.rgb, gi, blendFactor));
    
    return float4(finalGi, 1.0f);
}