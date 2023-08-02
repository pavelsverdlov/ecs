@geometry@

struct GSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 color : COLOR;
};
struct GSOut
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 color : COLOR;
    float2 barycentricCoordinates : TEXCOORD9;
};

[maxvertexcount(3)]
void main(
	triangle GSIn i[3],
	inout TriangleStream<GSOut> stream
)
{
    GSOut g0 = (GSOut) 0;
    g0.position = i[0].position;
    g0.normal = i[0].normal;
    g0.color = i[0].color;
    GSOut g1 = (GSOut) 0;
    g1.position = i[1].position;
    g1.normal = i[1].normal;
    g1.color = i[1].color;
    GSOut g2 = (GSOut) 0;
    g2.position = i[2].position;
    g2.normal = i[2].normal;
    g2.color = i[2].color;
    
    g0.barycentricCoordinates = float2(1, 0);
    g1.barycentricCoordinates = float2(0, 1);
    g2.barycentricCoordinates = float2(0, 0);
   
    stream.Append(g0);
    stream.Append(g1);
    stream.Append(g2);
}

@fragment@

#include "Common"

struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 color : COLOR;
    float2 barycentricCoordinates : TEXCOORD9;
};
float4 main(
    PSIn input, 
    bool isFront : SV_IsFrontFace
) : SV_TARGET
{
    float4 normal = input.normal * (1 - isFront * 2);
    float4 color = ComputePhongColor(input.position.xyz, normal, CurrentMaterial);
    
    float3 barys;
    barys.xy = input.barycentricCoordinates;
    barys.z = 1 - barys.x - barys.y;
    float minBary = min(barys.x, min(barys.y, barys.z));
    
    //float delta = abs(ddx(minBary)) + abs(ddy(minBary));
    float delta = fwidth(minBary);
    minBary = smoothstep(0, delta * 2, minBary);
    //minBary = smoothstep(delta, 2 * delta, minBary);
    
    return float4(color.xyz * minBary, color.w);
}
