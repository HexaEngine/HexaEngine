#include "common.hlsl"

float transformUV(float uv)
{
    return (uv + offset.x) * scale.x;
}

float2 transformUV(float2 uv)
{
    return (uv + offset.xy) * scale.xy;
}

float3 transformUV(float3 uv)
{
    return (uv + offset.xyz) * scale.xyz;
}

float4 transformUV(float4 uv)
{
    return (uv + offset) * scale;
}

#if Blue2D
#include "blue2d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return blue(transformUV(vs.Tex));
}
#endif

#if Cellular2D
#include "cellular2d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return float4(cellular(transformUV(vs.Tex)), 0, 0);
}
#endif

#if Cellular2x2
#include "cellular2x2.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return float4(cellular2x2(transformUV(vs.Tex)), 0, 0);
}
#endif

#if Cellular2x2x2
#include "cellular2x2x2.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float3 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return float4(cellular2x2x2(transformUV(vs.Tex)), 0, 0);
}
#endif

#if Cellular3D
#include "cellular3d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float3 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return float4(cellular(transformUV(vs.Tex)), 0, 0);
}
#endif

#if Hash1D
#include "hash1d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return rand1(transformUV(vs.Tex));
}
#endif

#if Hash2D
#include "hash2d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return rand2(transformUV(vs.Tex));
}
#endif

#if Hash3D
#include "hash3d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float3 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return rand3(transformUV(vs.Tex));
}
#endif

#if Perlin2D | Perlin2DPeriodicVariant
#include "perlin2d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
#if Perlin2DPeriodicVariant
    return pnoise(transformUV(vs.Tex), repeat.xy);
#else
    return cnoise(transformUV(vs.Tex));
#endif
}
#endif

#if Perlin3D | Perlin3DPeriodicVariant
#include "perlin3d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float3 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
#if Perlin3DPeriodicVariant
    return pnoise(transformUV(vs.Tex), repeat.xyz);
#else
    return cnoise(transformUV(vs.Tex));
#endif
}
#endif

#if Perlin4D | Perlin4DPeriodicVariant
#include "perlin4d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float4 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
#if Perlin4DPeriodicVariant
    return pnoise(transformUV(vs.Tex), repeat.xyzw);
#else
    return cnoise(transformUV(vs.Tex));
#endif
}
#endif

#if Simplex2D
#include "simplex2d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return snoise(transformUV(vs.Tex));
}
#endif

#if Simplex3D
#include "simplex3d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float3 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return snoise(transformUV(vs.Tex));
}
#endif

#if Simplex3DGrad
#include "simplex3dgrad.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float3 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    float3 gradient;
    float noise = snoise(transformUV(vs.Tex), gradient);
    return float4(noise, gradient);
}
#endif

#if Simplex4D
#include "simplex4d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float4 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
    return snoise(transformUV(vs.Tex));
}
#endif

#if Simplex2DPSRD | Simplex2DPSD | Simplex2DSRD | Simplex2DSR
#include "psrdnoise2d.hlsl"

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

float4 main(VSOut vs) : SV_Target
{
#if Simplex2DPSRD
    return psrdnoise(transformUV(vs.Tex), period.xy, rotation.x);
#endif
#if Simplex2DPSD
    return psrnoise(transformUV(vs.Tex), period.xy, rotation.x);
#endif
#if Simplex2DSRD
    return srdnoise(transformUV(vs.Tex), rotation.x);
#endif
#if Simplex2DSR
    return srnoise(transformUV(vs.Tex), rotation.x);
#endif
}
#endif