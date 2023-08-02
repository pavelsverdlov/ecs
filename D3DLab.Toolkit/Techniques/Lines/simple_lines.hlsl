@vertex@

#include "Common"

struct OutVS
{
    float4 position : SV_Position;
    float4 color : COLOR;
};

OutVS main(float4 position : POSITION, float4 color : COLOR)
{
    OutVS output = (OutVS) 0;

    output.position = toWVP(position);
    output.color = color;

    return output;
}

@fragment@

struct InFS
{
    float4 position : SV_Position;
    float4 color : COLOR;
};

float4 main(InFS input) : SV_TARGET
{
    return input.color;
}
