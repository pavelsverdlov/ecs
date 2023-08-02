@vertex@

#include "Game"
#include "Light"

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

	output.color = color * computeLight(output.position.xyz, output.normal, -LookDirection.xyz, 1000);

   
    
	return output;
}

@fragment@

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

//globallycoherent RWTexture2D<uint> headBuffer : register(u2);
//globallycoherent RWStructuredBuffer<ListNode> fragmentsList : register(u3);



//uint packColor(float4 color)
//{
//    return (uint(color.r * 255) << 24) | (uint(color.g * 255) << 16) | (uint(color.b * 255) << 8) | uint(color.a * 255);
//}
//float4 unpackColor(uint color)
//{
//    float4 output;
//    output.r = float((color >> 24) & 0x000000ff) / 255.0f;
//    output.g = float((color >> 16) & 0x000000ff) / 255.0f;
//    output.b = float((color >> 8) & 0x000000ff) / 255.0f;
//    output.a = float(color & 0x000000ff) / 255.0f;
//    return saturate(output);
//}

//void insertionSortMSAA(uint startIndex, uint sampleIndex, inout NodeData sortedFragments[MAX_FRAGMENTS], out int counter)
//{
//    counter = 0;
//    uint index = startIndex;
//    for (int i = 0; i < MAX_FRAGMENTS; i++)
//    {
//        if (index != 0xffffffff)
//        {
//            uint coverage = (fragmentsList[index].depthAndCoverage >> 16);
//            if (coverage & (1 << sampleIndex))
//            {
//                sortedFragments[counter].packedColor = fragmentsList[index].packedColor;
//                sortedFragments[counter].depth = f16tof32(fragmentsList[index].depthAndCoverage);
//                counter++;
//            }
//            index = fragmentsList[index].next;
//        }
//    }

//    for (int k = 1; k < MAX_FRAGMENTS; k++)
//    {
//        int j = k;
//        NodeData t = sortedFragments[k];

//        while (sortedFragments[j - 1].depth < t.depth)
//        {
//            sortedFragments[j] = sortedFragments[j - 1];
//            j--;
//            if (j <= 0)
//            {
//                break;
//            }
//        }

//        if (j != k)
//        {
//            sortedFragments[j] = t;
//        }
//    }
//}

RWTexture2D<float4> UBuffer0 : register(u1); // R8G8B8A8
RWTexture2D<float> UBuffer1 : register(u2); // R32F

float4 main(PSIn input, uint coverage : SV_COVERAGE, bool frontFace : SV_IsFrontFace) : SV_TARGET
{
    
    uint2 addr = uint2(input.position.xy);
    float depth = UBuffer1[addr];
    if (input.position.z > depth)//input.position.z == 0
    {
        UBuffer0[addr] = input.color; // color
        UBuffer1[addr] = input.position.z; // depth
    }
    else
    {
        UBuffer0[addr] = float4(1, 1, 1, 1);
        UBuffer1[addr] = input.position.z; // depth
    }
	
    input.color = UBuffer0[addr];
    return input.color;
}


//[earlydepthstencil]
//float4 main2(PSIn input, uint coverage : SV_COVERAGE, bool frontFace : SV_IsFrontFace) : SV_TARGET
//{
	
//    //depths[uint2(input.position.xy)] = 0xFF000000;
//    //depths[uint2(input.position.xy)] = float(input.position.w);
    
//    float4 color = input.color; //computeColorTransparent(input, frontFace);
//    uint newHeadBufferValue = fragmentsList.IncrementCounter();
//    if (newHeadBufferValue == 0xffffffff)
//    {
//        return float4(0, 0, 0, 0);
//    }
	
//    uint2 upos = uint2(input.position.xy);
//    uint previosHeadBufferValue;
//    InterlockedExchange(headBuffer[upos], newHeadBufferValue, previosHeadBufferValue);
	
//    uint currentDepth = f32tof16(input.position.z); //input.worldPos.w
//    ListNode node;
//    node.packedColor = packColor(float4(color.rgb, color.a));
//    node.depthAndCoverage = currentDepth | (coverage << 16);
//    node.next = previosHeadBufferValue;
//    fragmentsList[newHeadBufferValue] = node;
	
//	return input.color;
//}

//float4 main1(PSIn input, uint sampleIndex : SV_SAMPLEINDEX) : SV_TARGET
//{
//    uint2 upos = uint2(input.position.xy);
//    uint index = headBuffer[upos];
//    clip(index == 0xffffffff ? -1 : 1);
	
//    float3 color = float3(0, 0, 0);
//    float alpha = 1;
	
//    NodeData sortedFragments[MAX_FRAGMENTS];
//	[unroll]
//    for (int j = 0; j < MAX_FRAGMENTS; j++)
//    {
//        sortedFragments[j] = (NodeData) 0;
//    }

//    int counter;
//    insertionSortMSAA(index, sampleIndex, sortedFragments, counter);

//	// resolve multisampling
//    int resolveBuffer[MAX_FRAGMENTS];
//    float4 colors[MAX_FRAGMENTS];
//    int resolveIndex = -1;
//    float prevdepth = -1.0f;
//	[unroll(MAX_FRAGMENTS)]
//    for (int i = 0; i < counter; i++)
//    {
//        if (sortedFragments[i].depth != prevdepth)
//        {
//            resolveIndex = -1;
//            resolveBuffer[i] = 1;
//            colors[i] = unpackColor(sortedFragments[i].packedColor);
//        }
//        else
//        {
//            if (resolveIndex < 0)
//            {
//                resolveIndex = i - 1;
//            }

//            colors[resolveIndex] += unpackColor(sortedFragments[i].packedColor);
//            resolveBuffer[resolveIndex]++;

//            resolveBuffer[i] = 0;
//        }
//        prevdepth = sortedFragments[i].depth;
//    }

//	// gather
//	[unroll(MAX_FRAGMENTS)]
//    for (int i = 0; i < counter; i++)
//    {
//		[branch]
//        if (resolveBuffer[i] != 0)
//        {
//            float4 c = colors[i] / float(resolveBuffer[i]);
//            alpha *= (1.0 - c.a);
//            color = lerp(color, c.rgb, c.a);
//        }
//    }

//    return float4(color, alpha);
//}