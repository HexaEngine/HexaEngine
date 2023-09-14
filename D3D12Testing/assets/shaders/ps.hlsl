struct PSIn
{
    float4 Position : SV_Position;
    float3 Color : Color;
};

float4 main(PSIn input) : SV_Target
{
    return float4(input.Color, 1);
}