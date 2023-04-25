struct PixelInput
{
    float4 pos : SV_POSITION;
    float4 color : COLOR;
};

float4 main(PixelInput input, uint primitiveId : SV_PrimitiveID) : SV_Target
{
    input.color.z = primitiveId;
    return input.color;
}