SamplerState linear_wrap_sampler : register(s0);
SamplerState linear_clamp_sampler : register(s1);


#define PI 3.1415926535897932384626433832795

//https://github.com/turanszkij/WickedEngine

struct VoxelRadiance
{
    float3 GridCenter;
    float DataSize; // voxel half-extent in world space units
    float DataSizeRCP; // 1.0 / voxel-half extent
    uint DataRes; // voxel grid resolution
    float DataResRCP; // 1.0 / voxel grid resolution
    uint NumCones;
    float NumConesRCP;
    float MaxDistance;
    float RayStepSize;
    uint Mips;
};

struct VoxelType
{
    uint ColorMask;
    uint NormalMask;
};

static const float hdr_range = 10.0f;

static const float max_voxel_lights = 8;

// Encode HDR color to a 32 bit uint
// Alpha is 1 bit + 7 bit HDR remapping
inline uint EncodeColor(in float4 color)
{
	// normalize color to LDR
    float hdr = length(color.rgb);
    color.rgb /= hdr;

	// encode LDR color and HDR range
    uint3 iColor = uint3(color.rgb * 255.0f);
    uint iHDR = (uint) (saturate(hdr / hdr_range) * 127);
    uint colorMask = (iHDR << 24u) | (iColor.r << 16u) | (iColor.g << 8u) | iColor.b;

	// encode alpha into highest bit
    uint iAlpha = (color.a > 0 ? 1u : 0u);
    colorMask |= iAlpha << 31u;

    return colorMask;
}

// Decode 32 bit uint into HDR color with 1 bit alpha
inline float4 DecodeColor(in uint colorMask)
{
    float hdr;
    float4 color;

    hdr = (colorMask >> 24u) & 0x0000007f;
    color.r = (colorMask >> 16u) & 0x000000ff;
    color.g = (colorMask >> 8u) & 0x000000ff;
    color.b = colorMask & 0x000000ff;

    hdr /= 127.0f;
    color.rgb /= 255.0f;

    color.rgb *= hdr * hdr_range;

    color.a = (colorMask >> 31u) & 0x00000001;

    return color;
}

// Encode specified normal (normalized) into an unsigned integer. Each axis of
// the normal is encoded into 9 bits (1 for the sign/ 8 for the value).
inline uint EncodeNormal(in float3 normal)
{
    int3 iNormal = int3(normal * 255.0f);
    uint3 iNormalSigns;
    iNormalSigns.x = (iNormal.x >> 5) & 0x04000000;
    iNormalSigns.y = (iNormal.y >> 14) & 0x00020000;
    iNormalSigns.z = (iNormal.z >> 23) & 0x00000100;
    iNormal = abs(iNormal);
    uint normalMask = iNormalSigns.x | (iNormal.x << 18) | iNormalSigns.y | (iNormal.y << 9) | iNormalSigns.z | iNormal.z;
    return normalMask;
}

// Decode specified mask into a float3 normal (normalized).
inline float3 DecodeNormal(in uint normalMask)
{
    int3 iNormal;
    iNormal.x = (normalMask >> 18) & 0x000000ff;
    iNormal.y = (normalMask >> 9) & 0x000000ff;
    iNormal.z = normalMask & 0x000000ff;
    int3 iNormalSigns;
    iNormalSigns.x = (normalMask >> 25) & 0x00000002;
    iNormalSigns.y = (normalMask >> 16) & 0x00000002;
    iNormalSigns.z = (normalMask >> 7) & 0x00000002;
    iNormalSigns = 1 - iNormalSigns;
    float3 normal = float3(iNormal) / 255.0f;
    normal *= iNormalSigns;
    return normal;
}

// 3D array index to flattened 1D array index
inline uint Flatten3D(uint3 coord, uint3 dim)
{
    return (coord.z * dim.x * dim.y) + (coord.y * dim.x) + coord.x;
}
// flattened array index to 3D array index
inline uint3 Unflatten3D(uint idx, uint3 dim)
{
    const uint z = idx / (dim.x * dim.y);
    idx -= (z * dim.x * dim.y);
    const uint y = idx / dim.x;
    const uint x = idx % dim.x;
    return uint3(x, y, z);
}
