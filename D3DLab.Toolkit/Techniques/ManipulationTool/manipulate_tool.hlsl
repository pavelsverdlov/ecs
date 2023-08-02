@vertex@

struct VSOut
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 color : COLOR;
};

VSOut main(float4 position : POSITION, float3 normal : NORMAL, float4 color : COLOR)
{
    
}

@fragment@

struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
    float4 color : COLOR;
};

float4 main(PSIn input) : SV_TARGET
{
    return input.color;
}