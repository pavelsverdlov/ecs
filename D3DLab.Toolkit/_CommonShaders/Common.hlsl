/*
    CONSTS
*/

//const - Mark a variable that cannot be changed by a shader, therefore, it must be initialized in the variable declaration. 
//static -  it is initialized one time and persists between function calls

static const float3 unitZ = float3(0, 0, 1);
static const float3 unitX = float3(1, 0, 0);
static const float3 unitY = float3(0, 1, 0);

static const float3 PI = acos(-1);

/*
    STRUCTS
*/

struct Light
{
    float4 Color;
    float4 Direction;
    float Intensity;
    float Type;
};
struct Material
{
    float4 ColorAmbient;
    float4 ColorDiffuse;
    float4 ColorSpecular;
    float4 ColorReflection;
    float SpecularFactor;    
};

/*
    BUFFERS
*/

cbuffer Game : register(b0)
{
    float4 v4LookDirection;
    float4 v4CameraUp;
    float4 v4CameraPos;
    
    // viewport:
	// [w,h,1/w,1/h]
    float4 v4Viewport;

	
    float4x4 View;
    float4x4 Projection;
};
cbuffer Lights : register(b1)
{
    Light lights[3];
}
cbuffer Transformation : register(b2)
{
    float4x4 World;
    float4x4 WorldInverse;
}
cbuffer MaterialBuff : register(b3)
{
    Material CurrentMaterial;
};

/*
    FUNCTIONS
*/

float4 toScreen(float3 v)
{
    float4 p = float4(v, 1);
    //p = mul(View, p);
    //p = mul(Projection, p);
    p = mul(p, View);
    p = mul(p, Projection);
    return p;
}
float4 toWVP(float4 position)
{
	// Change the position vector to be 4 units for proper matrix calculations.
    position.w = 1.0f;

    position = mul(position, World);
    position = mul(position, View);
    position = mul(position, Projection);

    return position;
}
//convert from window coordinates to normalized device coordinates, values btw {-1;1}
float2 winToNdc(in float2 pos)
{
    return float2((pos.x * v4Viewport.z) * 2.0, (pos.y * v4Viewport.w) * 2.0);
}
float3x3 fromAxisAngle3x3(float angle, float3 axis) {
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3(
        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c
        );
}

float toNewRange(float oldVal, float oldMin, float oldMax, float newMim, float newMax) {
    return (((oldVal - oldMin) * (newMax - newMim)) / (oldMax - oldMin)) + newMim;
}

float4 ComputePhongColor(float3 P, float4 N, Material mat)
{
    float4 traceRay = v4LookDirection;
    float3 finalColor = mat.ColorAmbient;
    float intensity = 0;
    for (int i = 0; i < 3; ++i)
    {
        Light l = lights[i];
        if (l.Type == 0)
        {
            continue;
        }
        if (l.Type == 1)
        { //ambient
            finalColor *= l.Intensity;
        }
        else
        {
            float4 L;
            if (l.Type == 2)
            { //point
               // L = l.LightPosV3 - P; TODO: IMPLEMENT POINT LIGHT
                L = l.Direction;
               // finalColor += l.Intensity * MaterialColorDiffuse;
            }
            else if (l.Type == 3)
            { //directional
                L = l.Direction;
            }
            float diffIntensity = dot(N, -L); //-L because N & L points out to inverted direction
            if (diffIntensity > 0)
            { //diffuse
                finalColor += l.Intensity * saturate(diffIntensity) * mat.ColorDiffuse.rgb;
            }
            
            if (mat.SpecularFactor > 0)
            { //specular
                float4 R = reflect(L, N);
                finalColor += pow(saturate(dot(N, R)), mat.SpecularFactor) * mat.ColorSpecular.rgb;
            }
        }
    }
    
    return float4(finalColor, mat.ColorDiffuse.a);
}
