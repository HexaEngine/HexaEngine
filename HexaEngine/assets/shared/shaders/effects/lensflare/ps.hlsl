#include "../../camera.hlsl"
#include "../../weather.hlsl"

#define OCCLUSION_SAMPLES 4
#define OCCLUSION_RADIUS 1
#define OCCLUSION_DEPTH_BIAS 0.00001

struct VSOut
{
    float4 Pos : SV_Position;
    float2 Tex : TEXCOORD;
};

cbuffer LensParams
{
    float4 sunPosition;
    float4 tint;
};

Texture2D<float> depthTex : register(t0);
SamplerState linearClampSampler : register(s0);

float3 lensflare(float2 uv, float2 pos)
{

    float aspectRatio = screenDim.x / screenDim.y;

    float3 color = 0;

    float flaremultR = 0.04 * tint.r;
    float flaremultG = 0.05 * tint.g;
    float flaremultB = 0.075 * tint.b;
    float flarescale = 1.0;

    float sunmask = 1;
    float sunVisibility = 1;

    float cosTheta = light_dir.y;
    if (cosTheta < 0.0f)    // Handle sun going below the horizon
    {
        float a = clamp(1.0f + cosTheta * 50.0f, 0, 1);
        sunmask *= a;
    }

    // Small sun glare/glow

    float2 flare1scale = float2(1.7 * flarescale, 1.7 * flarescale);
    float flare1pow = 12.0;
    float2 flare1pos = float2(pos.x * aspectRatio * flare1scale.x, pos.y * flare1scale.y);

    float flare1 = distance(flare1pos, float2(uv.x * aspectRatio * flare1scale.x, uv.y * flare1scale.y));
    flare1 = 0.5 - flare1;
    flare1 = clamp(flare1, 0.0, 10.0);
    flare1 *= sunmask;
    flare1 = pow(flare1, 1.8);

    flare1 *= flare1pow;
    if (sunmask > 0.2)
    {
        color.r += flare1 * 1.0 * flaremultR;
        color.g += flare1 * 0.1 * flaremultG;
        color.b += flare1 * 0.0 * flaremultB;
    }
    else
    {
        color.r += flare1 * 0.0 * flaremultR;
        color.g += flare1 * 0.1 * flaremultG;
        color.b += flare1 * 0.5 * flaremultB;
    }

    /*--------------------------------------------------------------------*/

    // Huge sun glare/glow
    float2 flare1Bscale = float2(0.5 * flarescale, 0.5 * flarescale);
    float flare1Bpow = 6.0;
    float2 flare1Bpos = float2(pos.x * aspectRatio * flare1Bscale.x, pos.y * flare1Bscale.y);

    float flare1B = distance(flare1Bpos, float2(uv.x * aspectRatio * flare1Bscale.x, uv.y * flare1Bscale.y));
    flare1B = 0.5 - flare1B;
    flare1B = clamp(flare1B, 0.0, 10.0);
    flare1B *= sunmask;
    flare1B = pow(flare1B, 1.8);

    flare1B *= flare1Bpow;

    color.r += flare1B * 1.0 * flaremultR;
    color.g += flare1B * 0.1 * flaremultG;
    color.b += flare1B * 0.0 * flaremultB;
    /*--------------------------------------------------------------------*/

    //Far blue flare MAIN
    float2 flare3scale = float2(2.0 * flarescale, 2.0 * flarescale);
    float flare3pow = 0.7;
    float flare3fill = 10.0;
    float flare3offset = -0.5;
    float2 flare3pos = float2(((1.0 - pos.x) * (flare3offset + 1.0) - (flare3offset * 0.5)) * aspectRatio * flare3scale.x, ((1.0 - pos.y) * (flare3offset + 1.0) - (flare3offset * 0.5)) * flare3scale.y);

    float flare3 = distance(flare3pos, float2(uv.x * aspectRatio * flare3scale.x, uv.y * flare3scale.y));
    flare3 = 0.5 - flare3;
    flare3 = clamp(flare3 * flare3fill, 0.0, 1.0);
    flare3 = sin(flare3 * 1.57075);
    flare3 *= sunmask;
    flare3 = pow(flare3, 1.1);

    flare3 *= flare3pow;

    //subtract from blue flare
    float2 flare3Bscale = float2(1.4 * flarescale, 1.4 * flarescale);
    float flare3Bpow = 1.0;
    float flare3Bfill = 2.0;
    float flare3Boffset = -0.65f;
    float2 flare3Bpos = float2(((1.0 - pos.x) * (flare3Boffset + 1.0) - (flare3Boffset * 0.5)) * aspectRatio * flare3Bscale.x, ((1.0 - pos.y) * (flare3Boffset + 1.0) - (flare3Boffset * 0.5)) * flare3Bscale.y);

    float flare3B = distance(flare3Bpos, float2(uv.x * aspectRatio * flare3Bscale.x, uv.y * flare3Bscale.y));
    flare3B = 0.5 - flare3B;
    flare3B = clamp(flare3B * flare3Bfill, 0.0, 1.0);
    flare3B = sin(flare3B * 1.57075);
    flare3B *= sunmask;
    flare3B = pow(flare3B, 0.9);

    flare3B *= flare3Bpow;

    flare3 = clamp(flare3 - flare3B, 0.0, 10.0);

    color.r += flare3 * 0.5 * flaremultR;
    color.g += flare3 * 0.3 * flaremultG;
    color.b += flare3 * 1.0 * flaremultB;
    /*--------------------------------------------------------------------*/

    //Far blue flare MAIN 2
    float2 flare3Cscale = float2(3.2 * flarescale, 3.2 * flarescale);
    float flare3Cpow = 1.4;
    float flare3Cfill = 10.0;
    float flare3Coffset = -0.0;
    float2 flare3Cpos = float2(((1.0 - pos.x) * (flare3Coffset + 1.0) - (flare3Coffset * 0.5)) * aspectRatio * flare3Cscale.x, ((1.0 - pos.y) * (flare3Coffset + 1.0) - (flare3Coffset * 0.5)) * flare3Cscale.y);

    float flare3C = distance(flare3Cpos, float2(uv.x * aspectRatio * flare3Cscale.x, uv.y * flare3Cscale.y));
    flare3C = 0.5 - flare3C;
    flare3C = clamp(flare3C * flare3Cfill, 0.0, 1.0);
    flare3C = sin(flare3C * 1.57075);

    flare3C = pow(flare3C, 1.1);

    flare3C *= flare3Cpow;

	//subtract from blue flare
    float2 flare3Dscale = float2(2.1 * flarescale, 2.1 * flarescale);
    float flare3Dpow = 2.7;
    float flare3Dfill = 1.4;
    float flare3Doffset = -0.05f;
    float2 flare3Dpos = float2(((1.0 - pos.x) * (flare3Doffset + 1.0) - (flare3Doffset * 0.5)) * aspectRatio * flare3Dscale.x, ((1.0 - pos.y) * (flare3Doffset + 1.0) - (flare3Doffset * 0.5)) * flare3Dscale.y);

    float flare3D = distance(flare3Dpos, float2(uv.x * aspectRatio * flare3Dscale.x, uv.y * flare3Dscale.y));
    flare3D = 0.5 - flare3D;
    flare3D = clamp(flare3D * flare3Dfill, 0.0, 1.0);
    flare3D = sin(flare3D * 1.57075);
    flare3D = pow(flare3D, 0.9);

    flare3D *= flare3Dpow;

    flare3C = clamp(flare3C - flare3D, 0.0, 10.0);
    flare3C *= sunmask;

    color.r += flare3C * 0.5 * flaremultR;
    color.g += flare3C * 0.3 * flaremultG;
    color.b += flare3C * 1.0 * flaremultB;

	/*--------------------------------------------------------------------*/

	//far small pink flare
    float2 flare4scale = float2(4.5 * flarescale, 4.5 * flarescale);
    float flare4pow = 0.3;
    float flare4fill = 3.0;
    float flare4offset = -0.1;
    float2 flare4pos = float2(((1.0 - pos.x) * (flare4offset + 1.0) - (flare4offset * 0.5)) * aspectRatio * flare4scale.x, ((1.0 - pos.y) * (flare4offset + 1.0) - (flare4offset * 0.5)) * flare4scale.y);

    float flare4 = distance(flare4pos, float2(uv.x * aspectRatio * flare4scale.x, uv.y * flare4scale.y));
    flare4 = 0.5 - flare4;
    flare4 = clamp(flare4 * flare4fill, 0.0, 1.0);
    flare4 = sin(flare4 * 1.57075);
    flare4 *= sunmask;
    flare4 = pow(flare4, 1.1);

    flare4 *= flare4pow;

    color.r += flare4 * 1.6 * flaremultR;
    color.g += flare4 * 0.0 * flaremultG;
    color.b += flare4 * 1.8 * flaremultB;

	/*---------------------------------------------------------------*/
    /*
	//far small pink flare2
    float2 flare4Bscale = float2(7.5 * flarescale, 7.5 * flarescale);
    float flare4Bpow = 0.4;
    float flare4Bfill = 2.0;
    float flare4Boffset = 0.0;
    float2 flare4Bpos = float2(((1.0 - pos.x) * (flare4Boffset + 1.0) - (flare4Boffset * 0.5)) * aspectRatio * flare4Bscale.x, ((1.0 - pos.y) * (flare4Boffset + 1.0) - (flare4Boffset * 0.5)) * flare4Bscale.y);

    float flare4B = distance(flare4Bpos, float2(uv.x * aspectRatio * flare4Bscale.x, uv.y * flare4Bscale.y));
    flare4B = 0.5 - flare4B;
    flare4B = clamp(flare4B * flare4Bfill, 0.0, 1.0);
    flare4B = sin(flare4B * 1.57075);
    flare4B *= sunmask;
    flare4B = pow(flare4B, 1.1);

    flare4B *= flare4Bpow;

    color.r += flare4B * 1.4 * flaremultR;
    color.g += flare4B * 0.0 * flaremultG;
    color.b += flare4B * 1.8 * flaremultB;
    */
	/*------------------------------------------------------------*/
    /*
	//far small pink flare3
    float2 flare4Cscale = float2(37.5 * flarescale, 37.5 * flarescale);
    float flare4Cpow = 2.0;
    float flare4Cfill = 2.0;
    float flare4Coffset = -0.3;
    float2 flare4Cpos = float2(((1.0 - pos.x) * (flare4Coffset + 1.0) - (flare4Coffset * 0.5)) * aspectRatio * flare4Cscale.x, ((1.0 - pos.y) * (flare4Coffset + 1.0) - (flare4Coffset * 0.5)) * flare4Cscale.y);

    float flare4C = distance(flare4Cpos, float2(uv.x * aspectRatio * flare4Cscale.x, uv.y * flare4Cscale.y));
    flare4C = 0.5 - flare4C;
    flare4C = clamp(flare4C * flare4Cfill, 0.0, 1.0);
    flare4C = sin(flare4C * 1.57075);
    flare4C *= sunmask;
    flare4C = pow(flare4C, 1.1);

    flare4C *= flare4Cpow;

    color.r += flare4C * 1.6 * flaremultR;
    color.g += flare4C * 0.3 * flaremultG;
    color.b += flare4C * 1.1 * flaremultB;
    */
	/*----------------------------------------------------------------------------*/
    /*
	//far small pink flare4
    float2 flare4Dscale = float2(67.5 * flarescale, 67.5 * flarescale);
    float flare4Dpow = 1.0;
    float flare4Dfill = 2.0;
    float flare4Doffset = -0.35f;
    float2 flare4Dpos = float2(((1.0 - pos.x) * (flare4Doffset + 1.0) - (flare4Doffset * 0.5)) * aspectRatio * flare4Dscale.x, ((1.0 - pos.y) * (flare4Doffset + 1.0) - (flare4Doffset * 0.5)) * flare4Dscale.y);

    float flare4D = distance(flare4Dpos, float2(uv.x * aspectRatio * flare4Dscale.x, uv.y * flare4Dscale.y));
    flare4D = 0.5 - flare4D;
    flare4D = clamp(flare4D * flare4Dfill, 0.0, 1.0);
    flare4D = sin(flare4D * 1.57075);
    flare4D *= sunmask;
    flare4D = pow(flare4D, 1.1);

    flare4D *= flare4Dpow;

    color.r += flare4D * 1.2 * flaremultR;
    color.g += flare4D * 0.2 * flaremultG;
    color.b += flare4D * 1.2 * flaremultB;
    */
	/*------------------------------------------------------------------*/
    /*
	//far small pink flare5
    float2 flare4Escale = float2(60.5 * flarescale, 60.5 * flarescale);
    float flare4Epow = 1.0;
    float flare4Efill = 3.0;
    float flare4Eoffset = -0.3393f;
    float2 flare4Epos = float2(((1.0 - pos.x) * (flare4Eoffset + 1.0) - (flare4Eoffset * 0.5)) * aspectRatio * flare4Escale.x, ((1.0 - pos.y) * (flare4Eoffset + 1.0) - (flare4Eoffset * 0.5)) * flare4Escale.y);

    float flare4E = distance(flare4Epos, float2(uv.x * aspectRatio * flare4Escale.x, uv.y * flare4Escale.y));
    flare4E = 0.5 - flare4E;
    flare4E = clamp(flare4E * flare4Efill, 0.0, 1.0);
    flare4E = sin(flare4E * 1.57075);
    flare4E *= sunmask;
    flare4E = pow(flare4E, 1.1);

    flare4E *= flare4Epow;

    color.r += flare4E * 1.2 * flaremultR;
    color.g += flare4E * 0.2 * flaremultG;
    color.b += flare4E * 1.0 * flaremultB;
    */
	/*----------------------------------------------------------*/

	//Sun glow
    float2 flare5scale = float2(3.2 * flarescale, 3.2 * flarescale);
    float flare5pow = 13.4;
    float flare5fill = 1.0;
    float flare5offset = -2.0;
    float2 flare5pos = float2(((1.0 - pos.x) * (flare5offset + 1.0) - (flare5offset * 0.5)) * aspectRatio * flare5scale.x, ((1.0 - pos.y) * (flare5offset + 1.0) - (flare5offset * 0.5)) * flare5scale.y);

    float flare5 = distance(flare5pos, float2(uv.x * aspectRatio * flare5scale.x, uv.y * flare5scale.y));
    flare5 = 0.5 - flare5;
    flare5 = clamp(flare5 * flare5fill, 0.0, 1.0);
    flare5 *= sunmask;
    flare5 = pow(flare5, 1.9);

    flare5 *= flare5pow;

    color.r += flare5 * 2.0 * flaremultR;
    color.g += flare5 * 0.4 * flaremultG;
    color.b += flare5 * 0.1 * flaremultB;
	/*-----------------------------------------------------*/

	//Anamorphic lens
    float2 flareEscale = float2(0.2 * flarescale, 5.0 * flarescale);
    float flareEpow = 5.0;
    float flareEfill = 0.75;
    float2 flareEpos = float2(pos.x * aspectRatio * flareEscale.x, pos.y * flareEscale.y);

    float flareE = distance(flareEpos, float2(uv.x * aspectRatio * flareEscale.x, uv.y * flareEscale.y));
    flareE = 0.5 - flareE;
    flareE = clamp(flareE * flareEfill, 0.0, 1.0);
    flareE *= sunmask;
    flareE = pow(flareE, 1.4);
    flareE *= flareEpow;

    color.r += flareE * 0.0 * flaremultR;
    color.g += flareE * 0.05 * flaremultG;
    color.b += flareE * 1.0 * flaremultB;
	/*----------------------------------------------*/

    /*
	//first red sweep
    float2 flare_extra3scale = float2(32.0 * flarescale, 32.0 * flarescale);
    float flare_extra3pow = 2.5;
    float flare_extra3fill = 1.1;
    float flare_extra3offset = -1.3;
    float2 flare_extra3pos = float2(((1.0 - pos.x) * (flare_extra3offset + 1.0) - (flare_extra3offset * 0.5)) * aspectRatio * flare_extra3scale.x, ((1.0 - pos.y) * (flare_extra3offset + 1.0) - (flare_extra3offset * 0.5)) * flare_extra3scale.y);

    float flare_extra3 = distance(flare_extra3pos, float2(uv.x * aspectRatio * flare_extra3scale.x, uv.y * flare_extra3scale.y));
    flare_extra3 = 0.5 - flare_extra3;
    flare_extra3 = clamp(flare_extra3 * flare_extra3fill, 0.0, 1.0);
    flare_extra3 = sin(flare_extra3 * 1.57075);
    flare_extra3 *= sunmask;
    flare_extra3 = pow(flare_extra3, 1.1);

    flare_extra3 *= flare_extra3pow;

	//subtract
    float2 flare_extra3Bscale = float2(5.1 * flarescale, 5.1 * flarescale);
    float flare_extra3Bpow = 1.5;
    float flare_extra3Bfill = 1.0;
    float flare_extra3Boffset = -0.77f;
    float2 flare_extra3Bpos = float2(((1.0 - pos.x) * (flare_extra3Boffset + 1.0) - (flare_extra3Boffset * 0.5)) * aspectRatio * flare_extra3Bscale.x, ((1.0 - pos.y) * (flare_extra3Boffset + 1.0) - (flare_extra3Boffset * 0.5)) * flare_extra3Bscale.y);

    float flare_extra3B = distance(flare_extra3Bpos, float2(uv.x * aspectRatio * flare_extra3Bscale.x, uv.y * flare_extra3Bscale.y));
    flare_extra3B = 0.5 - flare_extra3B;
    flare_extra3B = clamp(flare_extra3B * flare_extra3Bfill, 0.0, 1.0);
    flare_extra3B = sin(flare_extra3B * 1.57075);
    flare_extra3B *= sunmask;
    flare_extra3B = pow(flare_extra3B, 0.9);

    flare_extra3B *= flare_extra3Bpow;

    flare_extra3 = clamp(flare_extra3 - flare_extra3B, 0.0, 10.0);

    color.r += flare_extra3 * 1.0 * flaremultR;
    color.g += flare_extra3 * 0.0 * flaremultG;
    color.b += flare_extra3 * 0.2 * flaremultB;
    */

	/*--------------------------------------------------------------------------*/

    /*
	//mid purple sweep
    float2 flare_extra4scale = float2(35.0 * flarescale, 35.0 * flarescale);
    float flare_extra4pow = 1.0;
    float flare_extra4fill = 1.1;
    float flare_extra4offset = -1.2;
    float2 flare_extra4pos = float2(((1.0 - pos.x) * (flare_extra4offset + 1.0) - (flare_extra4offset * 0.5)) * aspectRatio * flare_extra4scale.x, ((1.0 - pos.y) * (flare_extra4offset + 1.0) - (flare_extra4offset * 0.5)) * flare_extra4scale.y);

    float flare_extra4 = distance(flare_extra4pos, float2(uv.x * aspectRatio * flare_extra4scale.x, uv.y * flare_extra4scale.y));
    flare_extra4 = 0.5 - flare_extra4;
    flare_extra4 = clamp(flare_extra4 * flare_extra4fill, 0.0, 1.0);
    flare_extra4 = sin(flare_extra4 * 1.57075);
    flare_extra4 *= sunmask;
    flare_extra4 = pow(flare_extra4, 1.1);

    flare_extra4 *= flare_extra4pow;

	//subtract
    float2 flare_extra4Bscale = float2(5.1 * flarescale, 5.1 * flarescale);
    float flare_extra4Bpow = 1.5;
    float flare_extra4Bfill = 1.0;
    float flare_extra4Boffset = -0.77f;
    float2 flare_extra4Bpos = float2(((1.0 - pos.x) * (flare_extra4Boffset + 1.0) - (flare_extra4Boffset * 0.5)) * aspectRatio * flare_extra4Bscale.x, ((1.0 - pos.y) * (flare_extra4Boffset + 1.0) - (flare_extra4Boffset * 0.5)) * flare_extra4Bscale.y);

    float flare_extra4B = distance(flare_extra4Bpos, float2(uv.x * aspectRatio * flare_extra4Bscale.x, uv.y * flare_extra4Bscale.y));
    flare_extra4B = 0.5 - flare_extra4B;
    flare_extra4B = clamp(flare_extra4B * flare_extra4Bfill, 0.0, 1.0);
    flare_extra4B = sin(flare_extra4B * 1.57075);
    flare_extra4B *= sunmask;
    flare_extra4B = pow(flare_extra4B, 0.9);

    flare_extra4B *= flare_extra4Bpow;

    flare_extra4 = clamp(flare_extra4 - flare_extra4B, 0.0, 10.0);

    color.r += flare_extra4 * 0.7 * flaremultR;
    color.g += flare_extra4 * 0.1 * flaremultG;
    color.b += flare_extra4 * 1.0 * flaremultB;
    */

	/*----------------------------------------------------------------------------*/

    /*
	//last blue/purple sweep
    float2 flare_extra5scale = float2(25.0 * flarescale, 25.0 * flarescale);
    float flare_extra5pow = 4.0;
    float flare_extra5fill = 1.1;
    float flare_extra5offset = -0.9;
    float2 flare_extra5pos = float2(((1.0 - pos.x) * (flare_extra5offset + 1.0) - (flare_extra5offset * 0.5)) * aspectRatio * flare_extra5scale.x, ((1.0 - pos.y) * (flare_extra5offset + 1.0) - (flare_extra5offset * 0.5)) * flare_extra5scale.y);

    float flare_extra5 = distance(flare_extra5pos, float2(uv.x * aspectRatio * flare_extra5scale.x, uv.y * flare_extra5scale.y));
    flare_extra5 = 0.5 - flare_extra5;
    flare_extra5 = clamp(flare_extra5 * flare_extra5fill, 0.0, 1.0);
    flare_extra5 = sin(flare_extra5 * 1.57075);
    flare_extra5 *= sunmask;
    flare_extra5 = pow(flare_extra5, 1.1);

    flare_extra5 *= flare_extra5pow;

	//subtract
    float2 flare_extra5Bscale = float2(5.1 * flarescale, 5.1 * flarescale);
    float flare_extra5Bpow = 1.0;
    float flare_extra5Bfill = 1.0;
    float flare_extra5Boffset = -0.77f;
    float2 flare_extra5Bpos = float2(((1.0 - pos.x) * (flare_extra5Boffset + 1.0) - (flare_extra5Boffset * 0.5)) * aspectRatio * flare_extra5Bscale.x, ((1.0 - pos.y) * (flare_extra5Boffset + 1.0) - (flare_extra5Boffset * 0.5)) * flare_extra5Bscale.y);

    float flare_extra5B = distance(flare_extra5Bpos, float2(uv.x * aspectRatio * flare_extra5Bscale.x, uv.y * flare_extra5Bscale.y));
    flare_extra5B = 0.5 - flare_extra5B;
    flare_extra5B = clamp(flare_extra5B * flare_extra5Bfill, 0.0, 1.0);
    flare_extra5B = sin(flare_extra5B * 1.57075);
    flare_extra5B *= sunmask;
    flare_extra5B = pow(flare_extra5B, 0.9);

    flare_extra5B *= flare_extra5Bpow;

    flare_extra5 = clamp(flare_extra5 - flare_extra5B, 0.0, 10.0);

    color.r += flare_extra5 * 0.2 * flaremultR;
    color.g += flare_extra5 * 0.1 * flaremultG;
    color.b += flare_extra5 * 0.6 * flaremultB;
    */

	/*----------------------------------------------------------------------*/
    /*
	//mid orange sweep
    float2 flare10scale = float2(6.0 * flarescale, 6.0 * flarescale);
    float flare10pow = 1.9;
    float flare10fill = 1.1;
    float flare10offset = -0.7;
    float2 flare10pos = float2(((1.0 - pos.x) * (flare10offset + 1.0) - (flare10offset * 0.5)) * aspectRatio * flare10scale.x, ((1.0 - pos.y) * (flare10offset + 1.0) - (flare10offset * 0.5)) * flare10scale.y);

    float flare10 = distance(flare10pos, float2(uv.x * aspectRatio * flare10scale.x, uv.y * flare10scale.y));
    flare10 = 0.5 - flare10;
    flare10 = clamp(flare10 * flare10fill, 0.0, 1.0);
    flare10 = sin(flare10 * 1.57075);
    flare10 *= sunmask;
    flare10 = pow(flare10, 1.1);

    flare10 *= flare10pow;

	//subtract
    float2 flare10Bscale = float2(5.1 * flarescale, 5.1 * flarescale);
    float flare10Bpow = 1.5;
    float flare10Bfill = 1.0;
    float flare10Boffset = -0.77f;
    float2 flare10Bpos = float2(((1.0 - pos.x) * (flare10Boffset + 1.0) - (flare10Boffset * 0.5)) * aspectRatio * flare10Bscale.x, ((1.0 - pos.y) * (flare10Boffset + 1.0) - (flare10Boffset * 0.5)) * flare10Bscale.y);

    float flare10B = distance(flare10Bpos, float2(uv.x * aspectRatio * flare10Bscale.x, uv.y * flare10Bscale.y));
    flare10B = 0.5 - flare10B;
    flare10B = clamp(flare10B * flare10Bfill, 0.0, 1.0);
    flare10B = sin(flare10B * 1.57075);
    flare10B *= sunmask;
    flare10B = pow(flare10B, 0.9);

    flare10B *= flare10Bpow;

    flare10 = clamp(flare10 - flare10B, 0.0, 10.0);

    color.r += flare10 * 0.5 * flaremultR;
    color.g += flare10 * 0.3 * flaremultG;
    color.b += flare10 * 0.0 * flaremultB;
    */
	/*-----------------------------------------------------------------------------*/
    /*
	//mid blue sweep
    float2 flare10Cscale = float2(6.0 * flarescale, 6.0 * flarescale);
    float flare10Cpow = 1.9;
    float flare10Cfill = 1.1;
    float flare10Coffset = -0.6;
    float2 flare10Cpos = float2(((1.0 - pos.x) * (flare10Coffset + 1.0) - (flare10Coffset * 0.5)) * aspectRatio * flare10Cscale.x, ((1.0 - pos.y) * (flare10Coffset + 1.0) - (flare10Coffset * 0.5)) * flare10Cscale.y);

    float flare10C = distance(flare10Cpos, float2(uv.x * aspectRatio * flare10Cscale.x, uv.y * flare10Cscale.y));
    flare10C = 0.5 - flare10C;
    flare10C = clamp(flare10C * flare10Cfill, 0.0, 1.0);
    flare10C = sin(flare10C * 1.57075);
    flare10C *= sunmask;
    flare10C = pow(flare10C, 1.1);

    flare10C *= flare10Cpow;

	//subtract
    float2 flare10Dscale = float2(5.1 * flarescale, 5.1 * flarescale);
    float flare10Dpow = 1.5;
    float flare10Dfill = 1.0;
    float flare10Doffset = -0.67f;
    float2 flare10Dpos = float2(((1.0 - pos.x) * (flare10Doffset + 1.0) - (flare10Doffset * 0.5)) * aspectRatio * flare10Dscale.x, ((1.0 - pos.y) * (flare10Doffset + 1.0) - (flare10Doffset * 0.5)) * flare10Dscale.y);

    float flare10D = distance(flare10Dpos, float2(uv.x * aspectRatio * flare10Dscale.x, uv.y * flare10Dscale.y));
    flare10D = 0.5 - flare10D;
    flare10D = clamp(flare10D * flare10Dfill, 0.0, 1.0);
    flare10D = sin(flare10D * 1.57075);
    flare10D *= sunmask;
    flare10D = pow(flare10D, 0.9);

    flare10D *= flare10Dpow;

    flare10C = clamp(flare10C - flare10D, 0.0, 10.0);

    color.r += flare10C * 0.5 * flaremultR;
    color.g += flare10C * 0.3 * flaremultG;
    color.b += flare10C * 1.0 * flaremultB;
    */
	/*--------------------------------------------------------------------------------------*/

	//RedGlow1
    float2 flare11scale = float2(1.5 * flarescale, 1.5 * flarescale);
    float flare11pow = 1.1;
    float flare11fill = 2.0;
    float flare11offset = -0.523f;
    float2 flare11pos = float2(((1.0 - pos.x) * (flare11offset + 1.0) - (flare11offset * 0.5)) * aspectRatio * flare11scale.x, ((1.0 - pos.y) * (flare11offset + 1.0) - (flare11offset * 0.5)) * flare11scale.y);

    float flare11 = distance(flare11pos, float2(uv.x * aspectRatio * flare11scale.x, uv.y * flare11scale.y));
    flare11 = 0.5 - flare11;
    flare11 = clamp(flare11 * flare11fill, 0.0, 1.0);
    flare11 = pow(flare11, 2.9);
    flare11 *= sunmask;

    flare11 *= flare11pow;

    color.r += flare11 * 1.0 * flaremultR;
    color.g += flare11 * 0.2 * flaremultG;
    color.b += flare11 * 0.0 * flaremultB;

	/*------------------------------------------------------------------*/

	//PurpleGlow2
    float2 flare12scale = float2(2.5 * flarescale, 2.5 * flarescale);
    float flare12pow = 0.5;
    float flare12fill = 2.0;
    float flare12offset = -0.323f;
    float2 flare12pos = float2(((1.0 - pos.x) * (flare12offset + 1.0) - (flare12offset * 0.5)) * aspectRatio * flare12scale.x, ((1.0 - pos.y) * (flare12offset + 1.0) - (flare12offset * 0.5)) * flare12scale.y);

    float flare12 = distance(flare12pos, float2(uv.x * aspectRatio * flare12scale.x, uv.y * flare12scale.y));
    flare12 = 0.5 - flare12;
    flare12 = clamp(flare12 * flare12fill, 0.0, 1.0);
    flare12 = pow(flare12, 2.9);
    flare12 *= sunmask;

    flare12 *= flare12pow;

    color.r += flare12 * 0.7 * flaremultR;
    color.g += flare12 * 0.0 * flaremultG;
    color.b += flare12 * 1.0 * flaremultB;

	/*------------------------------------------------------------------*/

	//BlueGlow3
    float2 flare13scale = float2(1.0 * flarescale, 1.0 * flarescale);
    float flare13pow = 1.5;
    float flare13fill = 2.0;
    float flare13offset = +0.138f;
    float2 flare13pos = float2(((1.0 - pos.x) * (flare13offset + 1.0) - (flare13offset * 0.5)) * aspectRatio * flare13scale.x, ((1.0 - pos.y) * (flare13offset + 1.0) - (flare13offset * 0.5)) * flare13scale.y);

    float flare13 = distance(flare13pos, float2(uv.x * aspectRatio * flare13scale.x, uv.y * flare13scale.y));
    flare13 = 0.5 - flare13;
    flare13 = clamp(flare13 * flare13fill, 0.0, 1.0);
    flare13 = pow(flare13, 2.9);
    flare13 *= sunmask;

    flare13 *= flare13pow;

    color.r += flare13 * 0.0 * flaremultR;
    color.g += flare13 * 0.2 * flaremultG;
    color.b += flare13 * 1.0 * flaremultB;

	/*-------------------------------------------------------------*/

    return color;
}

float4 main(VSOut pin) : SV_Target
{
    float2 uv = pin.Tex;
    float2 sunPos = sunPosition.xy;

    float referenceDepth = saturate(sunPosition.z);

    const float2 texel = 1.0f / screenDim;
    const float2 radius = texel * OCCLUSION_RADIUS;
    const float2 step = radius / OCCLUSION_SAMPLES;

    float accdepth = 0;

    uint i = 0;
    [unroll(OCCLUSION_SAMPLES)]
    for (float y = -radius.y; i < OCCLUSION_SAMPLES; y += step.y, i++)
    {
        uint j = 0;
        [unroll(OCCLUSION_SAMPLES)]
        for (float x = -radius.x; j < OCCLUSION_SAMPLES; x += step.x, j++)
        {
            accdepth += depthTex.SampleLevel(linearClampSampler, sunPosition.xy + float2(x, y), 0).r >= referenceDepth - OCCLUSION_DEPTH_BIAS ? 1 : 0;
        }
    }
    accdepth /= (OCCLUSION_SAMPLES * OCCLUSION_SAMPLES);

    if (accdepth <= 0)
        discard;

    float3 color = 0;
    color = lensflare(uv, sunPos.xy) * accdepth;
    return float4(color, 1.0);
}