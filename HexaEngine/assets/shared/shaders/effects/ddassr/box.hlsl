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
Texture2D normalTexture : register(t1);
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
	float roughness = normalTexture.SampleLevel(state, vs.Tex, 0).w;

	int r_size = size * roughness + 2;

	[loop]
	for (int x = -r_size; x < r_size; ++x)
	{
		[loop]
		for (int y = -r_size; y < r_size; ++y)
		{
			float2 offset = float2(float(x), float(y)) * texelSize;
			result += tex.Sample(state, vs.Tex + offset);
			count++;
		}
	}
	return result / count;
}