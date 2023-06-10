struct VSOut
{
	float4 Pos : SV_Position;
	float2 Tex : TEXCOORD;
};

float WhiteNose(float2 p)
{
    return frac(sin(fmod(dot(p, float2(12.9898, 78.233)), 6.283)) * 43758.5453);
}

float4 main(VSOut vs) : SV_Target
{
    return WhiteNose(vs.Tex);
}