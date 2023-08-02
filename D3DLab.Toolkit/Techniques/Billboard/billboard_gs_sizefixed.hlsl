@geometry@

#include "Common"

struct InGS
{
    float4 p : SV_POSITION;
    float3 imageSize : COLOR;
};

struct OutGS
{
    float4 p : SV_POSITION;   
    noperspective //Do not perform perspective-correction during interpolation. 
        float2 t : TEXCOORD;
};

[maxvertexcount(4)]
void main(point InGS input[1], inout TriangleStream<OutGS> SpriteStream) {
    float4 ndcPosition0 = input[0].p;
    float4 ndcPosition1 = input[0].p;
    float4 ndcPosition2 = input[0].p;
    float4 ndcPosition3 = input[0].p;
    float3 size = input[0].imageSize;

    float sx = size.x * 0.5;
    float sy = size.y * 0.5 - size.z;//size.z fixed image offset

    float2 offTL = float2(-sx, sy);
    float2 offBR = float2(sx, -sy);
    float2 offTR = float2(sx, sy);
    float2 offBL = float2(-sx, -sy);

    ndcPosition0.xy = offBL;
    ndcPosition1.xy = offTL;
    ndcPosition2.xy = offBR;
    ndcPosition3.xy = offTR;

    ndcPosition0 = mul(ndcPosition0, Projection);
    ndcPosition1 = mul(ndcPosition1, Projection);
    ndcPosition2 = mul(ndcPosition2, Projection);
    ndcPosition3 = mul(ndcPosition3, Projection);

    float4 ndcTranslated0 = ndcPosition0 / ndcPosition0.w;
    float4 ndcTranslated1 = ndcPosition1 / ndcPosition1.w;
    float4 ndcTranslated2 = ndcPosition2 / ndcPosition2.w;
    float4 ndcTranslated3 = ndcPosition3 / ndcPosition3.w;

    float halfX = abs(ndcPosition0.x);
    float halfY = abs(ndcPosition0.y);

    //align left botton corner to position 
    float3 vBL = (input[0].p.xyz + ndcTranslated0.xyz) + unitY * halfY + unitX * halfX;
    float3 vTL = (input[0].p.xyz + ndcTranslated1.xyz) + unitY * halfY + unitX * halfX;
    float3 vBR = (input[0].p.xyz + ndcTranslated2.xyz) + unitY * halfY + unitX * halfX;
    float3 vTR = (input[0].p.xyz + ndcTranslated3.xyz) + unitY * halfY + unitX * halfX;

    OutGS output = (OutGS)0;
    output.p = float4(vTL, 1.0);
    output.t = float2(0, 0);
    SpriteStream.Append(output);

    output.p = float4(vTR, 1.0);
    output.t = float2(1, 0);
    SpriteStream.Append(output);

    output.p = float4(vBL, 1.0);
    output.t = float2(0, 1);
    SpriteStream.Append(output);

    output.p = float4(vBR, 1.0);
    output.t = float2(1, 1);
    SpriteStream.Append(output);

    SpriteStream.RestartStrip();
}