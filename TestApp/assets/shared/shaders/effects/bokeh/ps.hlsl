struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

Texture2D colorTexture;

SamplerState samplerState
{
	Filter = MIN_MAG_MIP_POINT;
	AddressU = Wrap;
	AddressV = Wrap;
};

cbuffer Params
{
	int size;
	float separation;
	float minThreshold;
	float maxThreshold;
};

float4 main(VSOut input) : SV_TARGET
{
	float2 texSize = float2(0,0);
	colorTexture.GetDimensions(texSize.x, texSize.y);
	float2 fragCoord = input.Pos.xy;

	float4 fragColor = colorTexture.Sample(samplerState, fragCoord / texSize);

	if (size <= 0) { return fragColor; }

	float  mx = 0.0;
	float4  cmx = fragColor;

	for (int i = -size; i <= size; ++i)
	{
		for (int j = -size; j <= size; ++j)
		{
			// For a rectangular shape.
			//if (false);

			// For a diamond shape;
			//if (!(abs(i) <= size - abs(j))) { continue; }

			// For a circular shape.
			if (!(distance(float2(i, j), float2(0, 0)) <= size)) { continue; }

			float4 c = colorTexture.Sample(samplerState, (input.Pos.xy + (float2(i, j) * separation)) / texSize);

			float mxt = dot(c.rgb, float3(0.3, 0.59, 0.11));

			if (mxt > mx) {
				mx = mxt;
				cmx = c;
			}
		}
	}

	fragColor.rgb = lerp(fragColor.rgb, cmx.rgb, smoothstep(minThreshold, maxThreshold, mx));
	return fragColor;
}