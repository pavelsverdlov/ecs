//https://github.com/microsoft/DirectX-Graphics-Samples

@vertex@

struct VSOut
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 color : COLOR;
};


@fragment@

struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 color : COLOR;
};

struct ListNode
{
    uint packedColor;
    uint depthAndCoverage;
    uint next;
};

globallycoherent RWTexture2D<uint> headBuffer : register(u0);
globallycoherent RWStructuredBuffer<ListNode> fragmentsList : register(u1);
RasterizerOrderedTexture2D<float4> BlendTexture : register(u2);

uint packColor(float4 color)
{
    return (uint(color.r * 255) << 24) | (uint(color.g * 255) << 16) | (uint(color.b * 255) << 8) | uint(color.a * 255);
}
void insertionSortMSAA(uint startIndex, uint sampleIndex, inout NodeData sortedFragments[MAX_FRAGMENTS], out int counter)
{
    counter = 0;
    uint index = startIndex;
    for (int i = 0; i < MAX_FRAGMENTS; i++)
    {
        if (index != 0xffffffff)
        {
            uint coverage = (fragmentsList[index].depthAndCoverage >> 16);
            if (coverage & (1 << sampleIndex))
            {
                sortedFragments[counter].packedColor = fragmentsList[index].packedColor;
                sortedFragments[counter].depth = f16tof32(fragmentsList[index].depthAndCoverage);
                counter++;
            }
            index = fragmentsList[index].next;
        }
    }

    for (int k = 1; k < MAX_FRAGMENTS; k++)
    {
        int j = k;
        NodeData t = sortedFragments[k];

        while (sortedFragments[j - 1].depth < t.depth)
        {
            sortedFragments[j] = sortedFragments[j - 1];
            j--;
            if (j <= 0)
            {
                break;
            }
        }

        if (j != k)
        {
            sortedFragments[j] = t;
        }
    }
}

[earlydepthstencil]
float4 main(PSIn input, uint coverage : SV_COVERAGE, bool frontFace : SV_IsFrontFace) : SV_TARGET
{
    float4 color = computeColorTransparent(input, frontFace);
    uint newHeadBufferValue = fragmentsList.IncrementCounter();
    if (newHeadBufferValue == 0xffffffff)
    {
        return float4(0, 0, 0, 0);
    }
	
    uint2 upos = uint2(input.position.xy);
    uint previosHeadBufferValue;
    InterlockedExchange(headBuffer[upos], newHeadBufferValue, previosHeadBufferValue);
	
    uint currentDepth = f32tof16(input.worldPos.w);
    ListNode node;
    node.packedColor = packColor(float4(color.rgb, color.a));
    node.depthAndCoverage = currentDepth | (coverage << 16);
    node.next = previosHeadBufferValue;
    fragmentsList[newHeadBufferValue] = node;
	
    return float4(0, 0, 0, 0);
}

float4 main(PSIn input, uint sampleIndex : SV_SAMPLEINDEX) : SV_TARGET
{
    uint2 upos = uint2(input.position.xy);
    uint index = headBuffer[upos];
    clip(index == 0xffffffff ? -1 : 1);
	
    float3 color = float3(0, 0, 0);
    float alpha = 1;
	
    NodeData sortedFragments[MAX_FRAGMENTS];
	[unroll]
    for (int j = 0; j < MAX_FRAGMENTS; j++)
    {
        sortedFragments[j] = (NodeData) 0;
    }

    int counter;
    insertionSortMSAA(index, sampleIndex, sortedFragments, counter);

	// resolve multisampling
    int resolveBuffer[MAX_FRAGMENTS];
    float4 colors[MAX_FRAGMENTS];
    int resolveIndex = -1;
    float prevdepth = -1.0f;
	[unroll(MAX_FRAGMENTS)]
    for (int i = 0; i < counter; i++)
    {
        if (sortedFragments[i].depth != prevdepth)
        {
            resolveIndex = -1;
            resolveBuffer[i] = 1;
            colors[i] = unpackColor(sortedFragments[i].packedColor);
        }
        else
        {
            if (resolveIndex < 0)
            {
                resolveIndex = i - 1;
            }

            colors[resolveIndex] += unpackColor(sortedFragments[i].packedColor);
            resolveBuffer[resolveIndex]++;

            resolveBuffer[i] = 0;
        }
        prevdepth = sortedFragments[i].depth;
    }

	// gather
	[unroll(MAX_FRAGMENTS)]
    for (int i = 0; i < counter; i++)
    {
		[branch]
        if (resolveBuffer[i] != 0)
        {
            float4 c = colors[i] / float(resolveBuffer[i]);
            alpha *= (1.0 - c.a);
            color = lerp(color, c.rgb, c.a);
        }
    }

    return float4(color, alpha);
}



float4 main(PSIn input, uint coverage : SV_COVERAGE, bool frontFace : SV_IsFrontFace)
{
    float4 color = input.color;
    float alfa = color.a;
    
    uint rgbe = BlendTexture[input.position.xy];
    float3 dstRGB = RGBE_to_RGB(rgbe);
    dstRGB = alfa * (1 - alfa) * dstRGB;
    BlendTexture[input.position.xy] = RGB_to_RGBE(dstRGB);
    
    return color;
}