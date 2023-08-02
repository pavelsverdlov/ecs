@vertex@

#include "Common"

struct InVS
{
    float4 p : POSITION;
    float3 imageSize : COLOR;
};
struct OutVS
{
    float4 p : SV_POSITION;
    float3 imageSize : COLOR;
};

OutVS main(InVS input)
{
    OutVS output = (OutVS) 0;
    output.imageSize = input.imageSize;
    
    output.p = toWVP(input.p);
    return output;
}

@fragment@

struct InPS
{
    float4 p : SV_POSITION;
    noperspective //Do not perform perspective-correction during interpolation. 
        float2 t : TEXCOORD;
};

Texture2D billboardTexture : register(t0);
SamplerState samplerBillboard : register(s0);
static const float discardAlphaLess = 0.5;

float4 main(InPS input) : SV_Target
{
    float4 pixelColor = billboardTexture.Sample(samplerBillboard, input.t);

    //Discards the current pixel if the specified value is less than zero.
    //https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-clip
    clip(pixelColor.a - discardAlphaLess);

    return pixelColor;
}