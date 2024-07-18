#ifndef RT_SIZE
#define RT_SIZE 64
#endif

#ifndef NUM_FACES
#define NUM_FACES 5
#endif

#ifndef NUM_BOUNCE_SUM_THREADS
#define NUM_BOUNCE_SUM_THREADS 512
#endif

//======================================================================
//
//	DX11 Radiosity Sample
//  by MJP
//  http://mynameismjp.wordpress.com/
//
//  All code and content licensed under Microsoft Public License (Ms-PL)
//
//======================================================================

cbuffer Constants : register(b0)
{
    float4x4 ToTangentSpace[5];
    float FinalWeight;
    uint VertexIndex;
    uint NumElements;
}

Texture2DArray<float4> RadianceMap : register(t0);
RWBuffer<float4> OutputBuffer : register(u0);

//-------------------------------------------------------------------------------------------------
// Projects a direction onto SH and convolves with a cosine kernel to compute irradiance
//-------------------------------------------------------------------------------------------------
void ProjectOntoSH(in float3 n, in float3 color, out float3 sh[9])
{
	// Cosine kernel
    const float A0 = 3.141593f;
    const float A1 = 2.095395f;
    const float A2 = 0.785398f;

    // Band 0
    sh[0] = 0.282095f * A0 * color;

    // Band 1
    sh[1] = 0.488603f * n.y * A1 * color;
    sh[2] = 0.488603f * n.z * A1 * color;
    sh[3] = 0.488603f * n.x * A1 * color;

    // Band 2
    sh[4] = 1.092548f * n.x * n.y * A2 * color;
    sh[5] = 1.092548f * n.y * n.z * A2 * color;
    sh[6] = 0.315392f * (3.0f * n.z * n.z - 1.0f) * A2 * color;
    sh[7] = 1.092548f * n.x * n.z * A2 * color;
    sh[8] = 0.546274f * (n.x * n.x - n.y * n.y) * A2 * color;
}

//-------------------------------------------------------------------------------------------------
// Converts from 3-band SH to 2-band H-Basis. See "Efficient Irradiance Normal Mapping" by
// Ralf Habel and Michael Wimmer for the derivations.
//-------------------------------------------------------------------------------------------------
void ConvertToHBasis(in float3 sh[9], out float3 hBasis[4])
{
    const float rt2 = sqrt(2.0f);
    const float rt32 = sqrt(3.0f / 2.0f);
    const float rt52 = sqrt(5.0f / 2.0f);
    const float rt152 = sqrt(15.0f / 2.0f);
    const float convMatrix[4][9] =
    {
        { 1.0f / rt2, 0, 0.5f * rt32, 0, 0, 0, 0, 0, 0 },
        { 0, 1.0f / rt2, 0, 0, 0, (3.0f / 8.0f) * rt52, 0, 0, 0 },
        { 0, 0, 1.0f / (2.0f * rt2), 0, 0, 0, 0.25f * rt152, 0, 0 },
        { 0, 0, 0, 1.0f / rt2, 0, 0, 0, (3.0f / 8.0f) * rt52, 0 }
    };

	[unroll(4)]
    for (uint row = 0; row < 4; ++row)
    {
        hBasis[row] = 0.0f;

		[unroll(9)]
        for (uint col = 0; col < 9; ++col)
            hBasis[row] += convMatrix[row][col] * sh[col];
    }
}

// Shared memory for summing H-Basis coefficients for a row
groupshared float3 RowHBasis[RT_SIZE][4];

//=================================================================================================
// Performs the initial integration/weighting for each pixel and sums together all SH coefficients
// for a row. The integration is based on the "Projection from Cube Maps" section of Peter Pike
// Sloan's "Stupid Spherical Harmonics Tricks".
//=================================================================================================
[numthreads(RT_SIZE, 1, 1)]
void main(uint3 GroupID : SV_GroupID, uint3 DispatchThreadID : SV_DispatchThreadID,
					uint3 GroupThreadID : SV_GroupThreadID, uint GroupIndex : SV_GroupIndex)
{
	// Gather RGB from the texels
    const int3 location = int3(GroupThreadID.x, GroupID.y, GroupID.z);
    float3 radiance = RadianceMap.Load(int4(location.xy, location.z, 0)).xyz;

	// Calculate the location in [-1, 1] texture space
    float u = (location.x / float(RT_SIZE)) * 2.0f - 1.0f;
    float v = -((location.y / float(RT_SIZE)) * 2.0f - 1.0f);

	// Calculate weight
    float temp = 1.0f + u * u + v * v;
    float weight = 4.0f / (sqrt(temp) * temp);
    radiance *= weight;

	// Extract direction from texel u,v
    float3 dirVS = normalize(float3(u, v, 1.0f));
    float3 dirTS = mul(dirVS, (float3x3) ToTangentSpace[location.z]);

	// Project onto SH
    float3 sh[9];
    ProjectOntoSH(dirTS, radiance, sh);

	// Convert to H-Basis
    float3 hBasis[4];
    ConvertToHBasis(sh, hBasis);

	// Store in shared memory
    RowHBasis[GroupThreadID.x][0] = hBasis[0];
    RowHBasis[GroupThreadID.x][1] = hBasis[1];
    RowHBasis[GroupThreadID.x][2] = hBasis[2];
    RowHBasis[GroupThreadID.x][3] = hBasis[3];
    GroupMemoryBarrierWithGroupSync();

	// Sum the coefficients for the row
	[unroll(RT_SIZE)]
    for (uint s = RT_SIZE / 2; s > 0; s >>= 1)
    {
        if (GroupThreadID.x < s)
        {
            RowHBasis[GroupThreadID.x][0] += RowHBasis[GroupThreadID.x + s][0];
            RowHBasis[GroupThreadID.x][1] += RowHBasis[GroupThreadID.x + s][1];
            RowHBasis[GroupThreadID.x][2] += RowHBasis[GroupThreadID.x + s][2];
            RowHBasis[GroupThreadID.x][3] += RowHBasis[GroupThreadID.x + s][3];
        }

        GroupMemoryBarrierWithGroupSync();
    }

	// Have the first thread write out to the output texture
    if (GroupThreadID.x == 0)
    {
		[unroll]
        for (uint i = 0; i < 3; ++i)
        {
            float4 packed = float4(RowHBasis[0][0][i], RowHBasis[0][1][i], RowHBasis[0][2][i], RowHBasis[0][3][i]);
            OutputBuffer[GroupID.y + RT_SIZE * i + RT_SIZE * 3 * location.z] = packed;
        }
    }
}