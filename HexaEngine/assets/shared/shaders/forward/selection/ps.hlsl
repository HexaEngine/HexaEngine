struct PixelInput
{
	float4 pos : SV_POSITION;
	nointerpolation uint4 color : COLOR;
};

uint4 main(PixelInput input, uint primitiveId : SV_PrimitiveID) : SV_Target
{
    input.color.z = primitiveId;
	return input.color;
}