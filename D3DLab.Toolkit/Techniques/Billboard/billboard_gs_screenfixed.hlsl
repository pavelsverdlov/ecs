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
    float2 size = winToNdc(input[0].imageSize.xy);
    float halfX = size.x * 0.5;
    float halfY = size.y * 0.5;
    //here we work with NDC orientation space it means values btw -1;1
    //Z+ is always look at and
    float3 up = unitX;
    float3 right = -unitY;
    //calculate center based on attach position and size
    float3 center = input[0].p.xyz;
    //use position as left-bottom point of image
    center = center + unitY * halfY;
    center = center + unitX * halfX;

    float3 v0 = center - right * halfY - up * halfX; //left bottom
    float3 v1 = center - right * halfY + up * halfX; // left top
    float3 v2 = center + right * halfY + up * halfX; // right top
    float3 v3 = center + right * halfY - up * halfX; //right bottom

    OutGS output = (OutGS)0;
    output.p = float4(v0, 1.0);
    output.t = float2(0, 0);
    SpriteStream.Append(output);

    output.p = float4(v1, 1.0);
    output.t = float2(1, 0);
    SpriteStream.Append(output);

    output.p = float4(v3, 1.0);
    output.t = float2(0, 1);
    SpriteStream.Append(output);

    output.p = float4(v2, 1.0);
    output.t = float2(1, 1);
    SpriteStream.Append(output);

    SpriteStream.RestartStrip();
}