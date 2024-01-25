Texture2D hdrTexture : register(t0);
SamplerState linearClampSampler : register(s0);

static const float permTexUnit = 1.0 / 256.0; // Perm texture texel-size
static const float permTexUnitHalf = 0.5 / 256.0; // Half perm texture texel-size

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer Params
{
    float Width;
    float Height;
    float Time;
    float GrainIntensity;

    float GrainSize;
    bool GrainColored;
    float GrainColorAmount;
    float GrainLumaAmount;
};

float fract(float inp)                 //GeDoSato did not (to my knowledge) have a fract function, so I implemented one.
{
    return (inp - floor(inp));
}

float4 mix(float x, float y, float a)          //GeDoSaTo did not to my knowledge have a mix function either, so I implemented one.
{
    return x * (1 - a) + y * a;
}

// a random texture generator, but you can also use a pre-computed perturbation texture
float4 rnm(in float2 tc)
{
    float noise = sin(dot(float3(tc.x, tc.y, Time), float3(12.9898, 78.233, 0.0025216))) * 43758.5453;

    float noiseR = frac(noise) * 2.0 - 1.0;
    float noiseG = frac(noise * 1.2154) * 2.0 - 1.0;
    float noiseB = frac(noise * 1.3453) * 2.0 - 1.0;
    float noiseA = frac(noise * 1.3647) * 2.0 - 1.0;

    return float4(noiseR, noiseG, noiseB, noiseA);
}

float fade(in float t)
{
    return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
}

float pnoise3D(in float3 p)
{
    float3 pi = permTexUnit * floor(p) + permTexUnitHalf; // Integer part, scaled so +1 moves permTexUnit texel
    // and offset 1/2 texel to sample texel centers
    float3 pf = frac(p); // Fractional part for interpolation

    // Noise contributions from (x=0, y=0), z=0 and z=1
    float perm00 = rnm(pi.xy).a;
    float3 grad000 = rnm(float2(perm00, pi.z)).rgb * 4.0 - 1.0;
    float n000 = dot(grad000, pf);
    float3 grad001 = rnm(float2(perm00, pi.z + permTexUnit)).rgb * 4.0 - 1.0;
    float n001 = dot(grad001, pf - float3(0.0, 0.0, 1.0));

    // Noise contributions from (x=0, y=1), z=0 and z=1
    float perm01 = rnm(pi.xy + float2(0.0, permTexUnit)).a;
    float3 grad010 = rnm(float2(perm01, pi.z)).rgb * 4.0 - 1.0;
    float n010 = dot(grad010, pf - float3(0.0, 1.0, 0.0));
    float3 grad011 = rnm(float2(perm01, pi.z + permTexUnit)).rgb * 4.0 - 1.0;
    float n011 = dot(grad011, pf - float3(0.0, 1.0, 1.0));

    // Noise contributions from (x=1, y=0), z=0 and z=1
    float perm10 = rnm(pi.xy + float2(permTexUnit, 0.0)).a;
    float3 grad100 = rnm(float2(perm10, pi.z)).rgb * 4.0 - 1.0;
    float n100 = dot(grad100, pf - float3(1.0, 0.0, 0.0));
    float3 grad101 = rnm(float2(perm10, pi.z + permTexUnit)).rgb * 4.0 - 1.0;
    float n101 = dot(grad101, pf - float3(1.0, 0.0, 1.0));

    // Noise contributions from (x=1, y=1), z=0 and z=1
    float perm11 = rnm(pi.xy + float2(permTexUnit, permTexUnit)).a;
    float3 grad110 = rnm(float2(perm11, pi.z)).rgb * 4.0 - 1.0;
    float n110 = dot(grad110, pf - float3(1.0, 1.0, 0.0));
    float3 grad111 = rnm(float2(perm11, pi.z + permTexUnit)).rgb * 4.0 - 1.0;
    float n111 = dot(grad111, pf - float3(1.0, 1.0, 1.0));

    // Blend contributions along x
    float4 n_x = lerp(float4(n000, n001, n010, n011), float4(n100, n101, n110, n111), fade(pf.x));

    // Blend contributions along y
    float2 n_xy = lerp(n_x.xy, n_x.zw, fade(pf.y));

    // Blend contributions along z
    float n_xyz = lerp(n_xy.x, n_xy.y, fade(pf.z));

    // We're done, return the final noise value.
    return n_xyz;
}

//2d coordinate orientation thing
float2 coordRot(in float2 tc, in float angle)
{
    float aspectr = Width / Height;
    float rotX = ((tc.x * 2.0 - 1.0) * aspectr * cos(angle)) - ((tc.y * 2.0 - 1.0) * sin(angle));
    float rotY = ((tc.y * 2.0 - 1.0) * cos(angle)) + ((tc.x * 2.0 - 1.0) * aspectr * sin(angle));
    rotX = ((rotX / aspectr) * 0.5 + 0.5);
    rotY = rotY * 0.5 + 0.5;
    return float2(rotX, rotY);
}

float4 main(VSOut vs) : SV_Target
{
    float2 tex = vs.Tex;

    float3 rotOffset = float3(1.425, 3.892, 5.835); //rotation offset values
    float2 rotCoordsR = coordRot(tex, Time + rotOffset.x);
    float2 rot = rotCoordsR * float2(Width / GrainSize, Height / GrainSize);
    float pNoise = pnoise3D(float3(rot.x, rot.y, 0.0));
    float3 noise = float3(pNoise, pNoise, pNoise);

    if (GrainColored == 1)
    {
        float2 rotCoordsG = coordRot(tex, Time + rotOffset.y);
        float2 rotCoordsB = coordRot(tex, Time + rotOffset.z);
        noise.g = lerp(noise.r, pnoise3D(float3(rotCoordsG * float2(Width / GrainSize, Height / GrainSize), 1.0)), GrainColorAmount);
        noise.b = lerp(noise.r, pnoise3D(float3(rotCoordsB * float2(Width / GrainSize, Height / GrainSize), 2.0)), GrainColorAmount);
    }

    float3 color = hdrTexture.SampleLevel(linearClampSampler, vs.Tex, 0).rgb;

    // noisiness response curve based on scene luminance
    const float3 lumcoeff = float3(0.299, 0.587, 0.114);
    float luminance = lerp(0.0, dot(color, lumcoeff), GrainLumaAmount);
    float lum = smoothstep(0.2, 0.0, luminance);
    lum += luminance;

    float2 thepow = pow(lum, 4.0);

    noise = lerp(noise, float3(0.0, 0.0, 0.0), pow(lum, 4.0));
    color += noise * GrainIntensity;

    return float4(color, 1);
}