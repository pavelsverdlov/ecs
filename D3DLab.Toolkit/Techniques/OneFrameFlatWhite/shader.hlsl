@vertex@

#include "Common"

struct VSOut
{
    float4 position : SV_POSITION;
};


VSOut main(float4 position : POSITION)
{
    VSOut output = (VSOut) 0;

    output.position = toWVP(position);

    return output;
}

@fragment@

struct PSIn
{
    float4 position : SV_POSITION;
};
float4 main(PSIn input) : SV_TARGET
{
    return float4(1, 1, 1, 1);
}

