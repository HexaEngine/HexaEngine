struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float rand(float2 co)
{
	return frac(sin(dot(co, float2(12.9898, 78.233))) * 43758.5453);
}

float4 main(VSOut vs) : SV_Target
{
	return float4(rand(vs.Tex), rand(vs.Tex + float2(1, 1)), 0, 0);
}