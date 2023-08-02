@vertex@
#include "Common"

struct VSOut
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
};

VSOut main(float4 position : POSITION, float3 normal : NORMAL)
{
    VSOut output = (VSOut) 0;

    output.position = toWVP(position);

    output.normal = mul(normal, World);
    output.normal = normalize(output.normal);

    return output;
}

@fragment@

#include "Common"

struct PSOITOutput
{
    float4 color : SV_Target0;
    float4 alpha : SV_Target1;
};
struct PSIn
{
    float4 position : SV_POSITION;
    float4 normal : NORMAL;
};

//Ref http://jcgt.org/published/0002/02/09/
PSOITOutput calculateOIT(in float4 color, float z, float zw)
{
    float OITSlope = 1;
    float OITPower =3;
    
    PSOITOutput output = (PSOITOutput) 0;
    float weight = 1;
    //z = z - vFrustum.z;
    //if (OITWeightMode == WeightModes_LinearA)
    //    weight = max(0.01f, min(3000.0f, 100 / (0.00001f + pow(abs(z) / 5.0f, abs(OITPower)) + pow(abs(z) / 200.0f, abs(OITPower) * 2))));
    //else if (OITWeightMode == WeightModes_LinearB)
    //    weight = max(0.01f, min(3000.0f, 100 / (0.00001f + pow(abs(z) / 10.0f, abs(OITPower)) + pow(abs(z) / 200.0f, abs(OITPower) * 2))));
    //else if (OITWeightMode == WeightModes_LinearC)
    //    weight = max(0.01f, min(3000.0f, 0.3f / (0.00001f + pow(abs(z) / 200.0f, abs(OITPower)))));
    //else if (OITWeightMode == WeightModes_NonLinear)
        weight = max(0.01f, 3e3 * pow(clamp(1.0f - zw * max(OITSlope, 1), 0, 1), abs(OITPower)));

    output.color = float4(color.rgb * color.a, color.a) * (color.a * weight);
        // Blend Func: GL_ZERO, GL_ONE_MINUS_SRC_ALPHA
    output.alpha.a = color.a;
    return output;
}
//--------------------------------------------------------------------------------------
// PER PIXEL LIGHTING - BLINN-PHONG for Order Independant Transparent A-buffer rendering
// http://casual-effects.blogspot.com/2014/03/weighted-blended-order-independent.html
//--------------------------------------------------------------------------------------

PSOITOutput main(PSIn input)
{
    float4 color = CurrentMaterial.ColorDiffuse;
    // float4 color = ComputePhongColor(input.position.xyz, normal, CurrentMaterial);
    return calculateOIT(color, LookDirection.w, input.position.z);
}