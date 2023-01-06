struct PixelInput
{
	float4 pos : SV_POSITION;
	nointerpolation uint4 color : COLOR;
};

uint4 main(PixelInput input) : SV_Target
{
	return input.color;
}