@vertex@
#include "Common"

static const float2 quadtexcoords[4] =
{
    float2(1, 0),
    float2(0, 0),
    float2(1, 1),
    float2(0, 1),
};
struct MeshOutlinePS_INPUT
{
    float4 Pos : SV_POSITION;
    noperspective
    float2 Tex : TEXCOORD0;
};
MeshOutlinePS_INPUT main(uint vI : SV_VERTEXID)
{
    MeshOutlinePS_INPUT output = (MeshOutlinePS_INPUT) 0;
    float2 texcoord = quadtexcoords[vI];
    output.Tex = texcoord;
    output.Pos = float4((texcoord.x - 0.5f) * 2, -(texcoord.y - 0.5f) * 2, 0, 1);
    return output;
}

@fragment@

SamplerState samplerSurface : register(s0);
Texture2D texOITColor : register(t10);
Texture2D texOITAlpha : register(t11);

struct MeshOutlinePS_INPUT
{
    float4 Pos : SV_POSITION;
    noperspective
    float2 Tex : TEXCOORD0;
};

float4 main(MeshOutlinePS_INPUT input): SV_Target
{
    return float4(1, 0, 0, 1);
    
    //float4 accum = texOITColor.Sample(samplerSurface, input.Tex.xy);
    //float reveal = texOITAlpha.Sample(samplerSurface, input.Tex.xy).a;
    //return float4(accum.rgb / max(accum.a, 1e-5), reveal);
    
    
}