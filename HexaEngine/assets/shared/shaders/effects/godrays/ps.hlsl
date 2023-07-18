
#define DITHER          //Dithering toggle
#define QUALITY     3   //0- low, 1- medium, 2- high

#define DECAY       .974
#define EXPOSURE    .24
#if (QUALITY==3)
#define SAMPLES    128
#define DENSITY    .97
#define WEIGHT     .25
#elif (QUALITY==2)
#define SAMPLES    64
#define DENSITY    .97
#define WEIGHT     .25
#elif (QUALITY==1)
#define SAMPLES    32
#define DENSITY    .95
#define WEIGHT     .25
#else
#define SAMPLES    16
#define DENSITY    .93
#define WEIGHT     .36
#endif

cbuffer GodrayParams
{
    float4 screen_space_position;
    float godrays_density;
    float godrays_weight;
    float godrays_decay;
    float godrays_exposure;
};

SamplerState linear_clamp_sampler : register(s0);

Texture2D sun_texture : register(t0);

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float WhiteNose(float2 p)
{
    return frac(sin(fmod(dot(p, float2(12.9898, 78.233)), 6.283)) * 43758.5453);
}

float BlueNoise(float2 p)
{
    return ((WhiteNose(p + float2(-1, -1)) + WhiteNose(p + float2(0, -1)) + WhiteNose(p + float2(1, -1)) + WhiteNose(p + float2(-1, 0)) - 8. * WhiteNose(p) + WhiteNose(p + float2(1, 0)) + WhiteNose(p + float2(-1, 1)) + WhiteNose(p + float2(0, 1)) + WhiteNose(p + float2(1, 1))) * .5 / 9. * 2.1 + .5);
}

static const float gr = (1.0 + sqrt(5.0)) * 0.5;

float4 main(VSOut pin) : SV_TARGET
{
    float2 tex_coord = pin.Tex;
    float3 color = sun_texture.SampleLevel(linear_clamp_sampler, tex_coord, 0).rgb;
    
    float2 light_pos = screen_space_position.xy;
    
    float2 delta_tex_coord = (tex_coord - light_pos);
    delta_tex_coord *= DENSITY / SAMPLES;
    
    float illumination_decay = 1.0f;
  
    float3 accumulated_god_rays = float3(0.0f, 0.0f, 0.0f);

    float3 accumulated = 0.0f;

    float jitter = BlueNoise(tex_coord.xy);
 
    for (int i = 0; i < SAMPLES; i++)
    {
        jitter = frac(jitter + gr * i);
        tex_coord.xy -= delta_tex_coord;
        float3 sam = sun_texture.SampleLevel(linear_clamp_sampler, tex_coord.xy + delta_tex_coord * jitter, 0).rgb;
        sam *= illumination_decay * WEIGHT;
        accumulated += sam;
        illumination_decay *= DECAY;
    }
    
    accumulated *= EXPOSURE;
    
    return float4(color + accumulated, 1.0f);
}
