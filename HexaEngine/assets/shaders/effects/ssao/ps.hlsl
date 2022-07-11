struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

Texture2D positionTexture : register(t1);
Texture2D normalTexture : register(t2);
Texture2D cleancoatNormalTexture : register(t3);
Texture2D emissionTexture : register(t4);
Texture2D misc0Texture : register(t5);
Texture2D misc1Texture : register(t6);
Texture2D misc2Texture : register(t7);
Texture2D noise : register(t0);

SamplerState samplerState;

cbuffer mvp : register(b0)
{
    matrix view;
    matrix projection;
};

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

#define TAP_SIZE 0.02
#define NUM_TAPS 16
#define THRESHOLD 0.1
#define SCALE 1.0



float4 main(VSOut vs) : SV_Target
{
// reconstruct the view space position from the depth map
    float start_Z = positionTexture.Sample(samplerState, vs.Tex).w;
    float start_Y = 1.0 - vs.Tex.y; // texture coordinates for D3D have origin in top left, but in camera space origin is in bottom left
    float3 start_Pos = float3(vs.Tex.x, start_Y, start_Z);
    float3 ndc_Pos = (2.0 * start_Pos) - 1.0;
    float4 unproject = mul(float4(ndc_Pos.x, ndc_Pos.y, ndc_Pos.z, 1.0), view);
    float3 viewPos = unproject.xyz / unproject.w;
    float3 viewNorm = normalTexture.Sample(samplerState, vs.Tex).xyz;

	// WORKNOTE: start_Z was a huge negative value at one point because we had a D16_UNORM depth target
	// but we set the shader resource view format to R16_FLOAT instead of R16_UNORM

    float total = 0.0;
    for (uint i = 0; i < NUM_TAPS; i++)
    {
        float3 offset = TAP_SIZE * taps[i];
        float2 offTex = vs.Tex + float2(offset.x, -offset.y);
        
        float off_start_Z = positionTexture.Sample(samplerState, offTex).w;
        float3 off_start_Pos = float3(offTex.x, start_Y + offset.y, off_start_Z);
        float3 off_ndc_Pos = (2.0 * off_start_Pos) - 1.0;
        float4 off_unproject = mul(float4(off_ndc_Pos.x, off_ndc_Pos.y, off_ndc_Pos.z, 1.0), view);
        float3 off_viewPos = off_unproject.xyz / off_unproject.w;

        float3 diff = off_viewPos.xyz - viewPos.xyz;
        float distance = length(diff);
        float3 diffnorm = normalize(diff);
        float dotx = diffnorm.x * viewNorm.x;

        float occlusion = max(0.0, dot(viewNorm, diffnorm)); // * SCALE / (1.0 + distance);
        total += (1.0 - occlusion);
    }

    total /= NUM_TAPS;
    return float4(total, total, total, 1.0);
}