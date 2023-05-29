#include "defs.hlsl"

VS_CONTROL_POINT_OUTPUT main(VS_CONTROL_POINT_INPUT Input)
{
    VS_CONTROL_POINT_OUTPUT Output;

    Output.vPosition = Input.pos;

    return Output;
}
