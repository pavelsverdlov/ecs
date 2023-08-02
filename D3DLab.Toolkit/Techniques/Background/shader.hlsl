@vertex@
#include "Common"

static const float2 quadtexcoords[4] =
{
    float2(1, 0),
    float2(0, 0),
    float2(1, 1),
    float2(0, 1),
};
struct VSOut
{
    float4 Pos : SV_POSITION;
    noperspective
    float2 Tex : TEXCOORD0;
};
VSOut main(uint vI : SV_VERTEXID)
{
    VSOut output = (VSOut) 0;
    float2 texcoord = quadtexcoords[vI];
    output.Tex = texcoord;
    output.Pos = float4((texcoord.x - 0.5f) * 2, -(texcoord.y - 0.5f) * 2, 0, 1);
    
    return output;
}

@fragment@

SamplerState SampleType;
Texture2D texturemap : register(t0);

struct PSIn
{
    float4 Pos : SV_POSITION;
    noperspective
    float2 Tex : TEXCOORD0;
};

float4 main(PSIn input) : SV_Target
{
    return texturemap.Sample(SampleType, input.Tex);
    //return float4(1, 0, 0, 1);
}