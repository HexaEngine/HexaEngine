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

Texture2D tex0;
Texture2D tex1;
SamplerState state;

float4 main(VSOut vs) : SV_Target
{
	if (size <= 0)
	{
        return tex1.Sample(state, vs.Tex);
    }

	float width;
	float heigth;
    tex0.GetDimensions(width, heigth);

	float2 texelSize = 1.0 / float2(width, heigth);

	float4 result = 0.0;
	float count = 0.0;
	for (int x = -size; x < size; ++x)
	{
		for (int y = -size; y < size; ++y)
		{
			float2 offset = float2(float(x), float(y)) * texelSize;
            result += tex0.Sample(state, vs.Tex + offset);
			count++;
		}
	}
	
	
    float4 blur = result / count;
	
    float4 color = tex1.Sample(state, vs.Tex);
	
    return saturate(blur + color);
}