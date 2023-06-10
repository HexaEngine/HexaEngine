//https://developer.nvidia.com/gpugems/gpugems3/part-iv-image-effects/chapter-27-motion-blur-post-processing-effect

Texture2D scene_texture : register(t0);
Texture2D<float2> velocity_buffer : register(t1);

SamplerState linear_wrap_sampler : register(s0);


struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

static const int SAMPLE_COUNT = 16;


float4 main(VSOut pin) : SV_TARGET
{
    float2 velocity = velocity_buffer.SampleLevel(linear_wrap_sampler, pin.Tex, 0);
    
    float2 tex_coord = pin.Tex;

    float4 color = scene_texture.Sample(linear_wrap_sampler, tex_coord);
    
    tex_coord += velocity;
    
    for (int i = 1; i < SAMPLE_COUNT; ++i)
    {
        float4 currentColor = scene_texture.Sample(linear_wrap_sampler, tex_coord);
   
    	 color += currentColor;
        
        tex_coord += velocity;
    }
    
    float4 final_color = color / SAMPLE_COUNT;
    
    return final_color;

}
