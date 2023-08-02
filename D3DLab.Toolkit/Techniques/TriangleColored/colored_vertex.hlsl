@vertex@

#include "Common"

struct VSOut
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 positionW : TEXCOORD0;
};


VSOut main(float4 position : POSITION, float3 normal : NORMAL)
{
    VSOut output = (VSOut) 0;

    output.position = toWVP(position);
    output.positionW = mul(position, World);

    output.normal = mul(normal, World);
    output.normal = normalize(output.normal);

    return output;
}

@fragment@

#include "Common"

struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
};
float4 main(PSIn input, bool isFront : SV_IsFrontFace) : SV_TARGET
{
    float4 normal = input.normal * (1 - isFront * 2);
    float4 color = ComputePhongColor(input.position.xyz, normal, CurrentMaterial);
    
    return color;
}

