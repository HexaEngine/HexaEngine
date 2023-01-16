TextureCube environmentMap;
SamplerState samplerState;

struct VSOut
{
	float4 Pos : SV_POSITION;
	float3 WorldPos : TEXCOORD0;
};

float4 main(VSOut vs) : SV_Target
{
	float PI = 3.14159265359;
	float3 N = normalize(vs.WorldPos);

	float3 irradiance = float3(0.0, 0.0, 0.0);

	// tangent space calculation from origin point
	float3 up = float3(0.0, 1.0, 0.0);
	float3 right = normalize(cross(up, N));
	up = normalize(cross(N, right));

	float sampleDelta = 0.025;
	float nrSamples = 0.0f;
	for (float phi = 0.0; phi < 2.0 * PI; phi += sampleDelta)
	{
		for (float theta = 0.0; theta < 0.5 * PI; theta += sampleDelta)
		{
			// spherical to cartesian (in tangent space)
			float3 tangentSample = float3(sin(theta) * cos(phi), sin(theta) * sin(phi), cos(theta));
			// tangent space to world
			float3 sampleVec = tangentSample.x * right + tangentSample.y * up + tangentSample.z * N;

			irradiance += environmentMap.SampleLevel(samplerState, sampleVec, -1).rgb * cos(theta) * sin(theta);
			nrSamples++;
		}
	}
	irradiance = PI * irradiance * (1.0 / float(nrSamples));

	return float4(irradiance, 1.0);
}