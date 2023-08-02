@vertex@

#include "Game"


struct VSOut
{
	float4 position : SV_POSITION;
 	float4 normal : NORMAL;
	float4 color : COLOR;    
};

VSOut main(float4 position : POSITION, float3 normal : NORMAL, float4 color : COLOR) {
	VSOut output = (VSOut)0;

	output.position = toWVP(position);

	output.normal = mul(normal, World);
	output.normal = normalize(output.normal);
    output.color = color;
    
	return output;
}

@fragment@

#include "Game"
#include "Light"

static const int MAX_FRAGMENTS = 16;

struct NodeData
{
    uint packedColor;
    float depth;
};
struct ListNode
{
    uint packedColor;
    uint depthAndCoverage;
    uint next;
    //uint temp;
};

struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 color : COLOR;
};

globallycoherent RWTexture2D<uint> headBuffer : register(u2);
globallycoherent RWStructuredBuffer<ListNode> fragmentsList : register(u3);

float4 main(PSIn input, uint coverage : SV_COVERAGE, bool frontFace : SV_IsFrontFace) : SV_TARGET
{
    
    //float3 dpdx = ddx(input.position.xyz);
    //float3 dpdy = ddy(input.position.xyz);
    ////float4 normal = float4(normalize(cross(dpdy, dpdx)), 0);
    
    input.color = input.color * computeLight(input.position.xyz, input.normal, -LookDirection.xyz, 1000);
    
    return input.color;
}