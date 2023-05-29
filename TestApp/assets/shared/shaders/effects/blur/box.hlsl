struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

cbuffer params
{
	int size;
	float3 padd;
};

Texture2D tex;
SamplerState state;

float4 main(VSOut vs) : SV_Target
{
	if (size <= 0)
	{
		return tex.Sample(state, vs.Tex);
	}

	float width;
	float heigth;
	tex.GetDimensions(width, heigth);

	float2 texelSize = 1.0 / float2(width, heigth);

	float4 result = 0.0;
	float count = 0.0;
	for (int x = -size; x < size; ++x)
	{
		for (int y = -size; y < size; ++y)
		{
			float2 offset = float2(float(x), float(y)) * texelSize;
			result += tex.Sample(state, vs.Tex + offset);
			count++;
		}
	}
	return result / count;
}