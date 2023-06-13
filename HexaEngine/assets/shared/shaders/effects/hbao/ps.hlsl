#include "../../camera.hlsl"
#include "../../gbuffer.hlsl"
struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D<float> depthTexture : register(t0);
Texture2D normalTexture : register(t1);

SamplerState samplerState;

static const float3 taps[16] =
{
    float3(-0.364452, -0.014985, -0.513535),
	float3(0.004669, -0.445692, -0.165899),
	float3(0.607166, -0.571184, 0.377880),
	float3(-0.607685, -0.352123, -0.663045),
	float3(-0.235328, -0.142338, 0.925718),
	float3(-0.023743, -0.297281, -0.392438),
	float3(0.918790, 0.056215, 0.092624),
	float3(0.608966, -0.385235, -0.108280),
	float3(-0.802881, 0.225105, 0.361339),
	float3(-0.070376, 0.303049, -0.905118),
	float3(-0.503922, -0.475265, 0.177892),
	float3(0.035096, -0.367809, -0.475295),
	float3(-0.316874, -0.374981, -0.345988),
	float3(-0.567278, -0.297800, -0.271889),
	float3(-0.123325, 0.197851, 0.626759),
	float3(0.852626, -0.061007, -0.144475)
};

cbuffer ConfigBuffer : register(b0)
{
    bool enabled;
    float SAMPLING_RADIUS;
    uint NUM_SAMPLING_DIRECTIONS;
    float SAMPLING_STEP;
    uint NUM_SAMPLING_STEPS;
    float3 padd;
    float2 Res;
    float2 ResInv;
};

#define TANGENT_BIAS 0.2
#define M_PI 3.1415926535897932384626433832795

float3 hash3(float2 p)
{
    float3 q = float3(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)), dot(p, float2(419.2, 371.9)));
    return frac(sin(q) * 43758.5453);
}

float4 main(VSOut input) : SV_Target
{
    if (!enabled)
    {
        return float4(1, 1, 1, 1);
    }

    // reconstruct the view space position from the depth map
    float start_Z = depthTexture.Sample(samplerState, input.Tex);
    float start_Y = 1.0 - input.Tex.y; // texture coordinates for D3D have origin in top left, but in camera space origin is in bottom left
    float2 start_Pos = float2(input.Tex.x, start_Y);
    float2 ndc_Pos = (2.0 * start_Pos) - 1.0;
    float4 unproject = mul(float4(ndc_Pos.x, ndc_Pos.y, start_Z, 1.0), projInv);
    float3 viewPos = unproject.xyz / unproject.w;
    float3 viewNorm = mul(UnpackNormal(normalTexture.Sample(samplerState, input.Tex).xyz), (float3x3) view);

    float total = 0.0;
    float sample_direction_increment = 2 * M_PI / NUM_SAMPLING_DIRECTIONS;
    
    for (uint i = 0; i < NUM_SAMPLING_DIRECTIONS; i++)
    {
	    // no jittering or randomization of sampling direction just yet
        float sampling_angle = i * sample_direction_increment; // azimuth angle theta in the paper
        float2 sampleDir = float2(cos(sampling_angle), sin(sampling_angle));
        
	    // we will now march along sampleDir and calculate the horizon
	    // horizon starts with the tangent plane to the surface, whose angle we can get from the normal
        float tangentAngle = acos(dot(float3(sampleDir.x, sampleDir.y, 0), viewNorm)) - (0.5 * M_PI) + TANGENT_BIAS;
        float horizonAngle = tangentAngle;
        float3 lastDiff = float3(0, 0, 0);
        for (uint j = 0; j < NUM_SAMPLING_STEPS; j++)
        {
		// march along the sampling direction and see what the horizon is
            float2 sampleOffset = float(j + 1) * SAMPLING_STEP * sampleDir;
            float2 offTex = input.Tex + float2(sampleOffset.x, -sampleOffset.y);

            float off_start_Z = depthTexture.Sample(samplerState, offTex);
            float2 off_start_Pos = float2(offTex.x, start_Y + sampleOffset.y);
            float2 off_ndc_Pos = (2.0 * off_start_Pos) - 1.0;
            float4 off_unproject = mul(float4(off_ndc_Pos.x, off_ndc_Pos.y, off_start_Z, 1.0), projInv);
            float3 off_viewPos = off_unproject.xyz / off_unproject.w;
		    // we now have the view space position of the offset point
            float3 diff = off_viewPos.xyz - viewPos.xyz;
            if (length(diff) < SAMPLING_RADIUS)
            {
			// skip samples which are outside of our local sampling radius
                lastDiff = diff;
			// WORKNOTE: in LH coordinate system, closer object will have smaller Z, so negative diff means higher elevation angle
			// that is why we negate diff.z
                float elevationAngle = atan(-diff.z / length(diff.xy));
                horizonAngle = max(horizonAngle, elevationAngle);
			//return float4(horizonAngle, 0, 0, 1.0);
            }
        }
	    // the paper uses this attenuation but I like the other way better
        float normDiff = length(lastDiff) / SAMPLING_RADIUS;
        float attenuation = 1 - normDiff * normDiff;
	    //float attenuation = 1.0 / (1 + length(lastDiff));
	    // now compare horizon angle to tangent angle to get ambient occlusion
        float occlusion = clamp(attenuation * (sin(horizonAngle) - sin(tangentAngle)), 0.0, 1.0);
        total += 1.0 - occlusion;
    }
    total /= NUM_SAMPLING_DIRECTIONS;
    return float4(total, total, total, 1.0);
}