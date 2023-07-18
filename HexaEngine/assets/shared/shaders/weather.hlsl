cbuffer WeatherCBuf : register(b2)
{
    float4 light_dir;
    float4 light_color;
    float4 sky_color;
    float4 ambient_color;
    float4 wind_dir;
    
    float wind_speed;
    float time;
    float crispiness;
    float curliness;
    
    float coverage;
    float absorption;
    float clouds_bottom_height;
    float clouds_top_height;
    
    float density_factor;
    float cloud_type;
    
    //padd float2
    
    float3 A;
    float3 B;
    float3 C;
    float3 D;
    float3 E;
    float3 F;
    float3 G;
    float3 H;
    float3 I;
    float3 Z;
}