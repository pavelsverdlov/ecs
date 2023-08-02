@vertex@

#include "Common"


struct VSOut
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 positionW : TEXCOORD0;
    float2 tex : TEXCOORD1;
};


VSOut main(float4 position : POSITION, float3 normal : NORMAL, float2 tex : TEXCOORD0)
{
    VSOut output = (VSOut) 0;

    output.position = toWVP(position);
    output.positionW = mul(position, World);

    output.normal = mul(normal, World);
    output.normal = normalize(output.normal);
    output.tex = tex;

    return output;
}

@fragment@

#include "Common"

SamplerState SampleType;
Texture2D texturemap : register(t0);

struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 positionW : TEXCOORD0;
    float2 tex : TEXCOORD1;
};
float4 main(PSIn input, bool isFront : SV_IsFrontFace) : SV_TARGET
{
    float4 texcolor = texturemap.Sample(SampleType, input.tex);
    
    Material mat = CurrentMaterial;
    
    texcolor = float4(texcolor.rgb, mat.ColorDiffuse.a);

    mat.ColorAmbient = texcolor;
    mat.ColorDiffuse = texcolor;
    mat.ColorSpecular = texcolor;
    
    bool hasAlpha = texcolor.a < 1;
    
    float4 normal = hasAlpha ? input.normal : input.normal * (1 - isFront * 2);
    
    //return float4(ComputePhongColor(input.position.xyz, normal, mat).rgb, 0.5);
    return ComputePhongColor(input.position.xyz, normal, mat);
}
