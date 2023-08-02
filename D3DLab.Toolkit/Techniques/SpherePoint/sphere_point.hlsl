@vertex@

#include "Common"

struct VSOut {
    float4 position : SV_Position;
    float4 positionW : TEXCOORD0;
	float4 color: COLOR;
};
VSOut main(float4 position : POSITION, float4 color : COLOR) {
    VSOut output;

    output.position = toWVP(position);
    output.positionW = mul(position, World);

	output.color = color;
	return output;
}

@fragment@

#include "Common"

float4 main(float4 position : SV_POSITION, float4 color : COLOR, float4 normal : NORMAL) : SV_TARGET{
    Material material = (Material)0;
	material.ColorAmbient = color;
	material.ColorDiffuse = color;
	material.ColorSpecular = color;
	material.ColorReflection = color;
	material.SpecularFactor = 400;

	return ComputePhongColor(position.xyz, normal, material);
}

@geometry@

#include "Common"

struct GSIn {
    float4 position : SV_Position;
    float4 positionW : TEXCOORD0;
    float4 color: COLOR;
};
struct GSOut {
    float4 position : SV_Position;
    float4 color: COLOR;
    float4 normal : NORMAL;
};

GSOut createVertex(in float3 sphCenter, in float3 p, in float4 color) {
    GSOut fs = (GSOut)0;
    fs.position = toScreen(p);
    fs.normal = float4(p - sphCenter, 0);
    fs.color = color;
    return fs;
}
GSOut createCenterVertex(in float3 p, in float4 color) {
    GSOut fs = (GSOut)0;
    fs.position = toScreen(p);
    fs.normal = -v4LookDirection;
    fs.color = color;
    return fs;
}

[maxvertexcount(75)]
void main(point GSIn points[1], inout TriangleStream<GSOut> output) {
    float radius = 0.5;

    float3 look = -normalize(v4LookDirection.xyz);
    float3 up = v4CameraUp.xyz;
    float3 right = normalize(cross(look, up));

    float3 sphCenter = points[0].positionW.xyz;
    float4 color = points[0].color;

    float3x3 TBN = fromAxisAngle3x3(10 * (PI / 180), look);
    float3 tangent = right;
    //move forward to make effect read 3d sphere if it will cross wih other objects
    float3 center = sphCenter + look * radius;

    //first triangle
    output.Append(createCenterVertex(center, color));
    output.Append(createVertex(sphCenter, center + tangent * radius, color));
    tangent = normalize(mul(tangent, TBN));
    output.Append(createVertex(sphCenter, center + tangent * radius, color));

    for (float angle = 10; angle < 360; angle += 10) {
        tangent = normalize(mul(tangent, TBN));
        float3 pWorld = center + tangent * radius;

        output.Append(createCenterVertex(center, color));
        output.Append(createVertex(sphCenter, pWorld, color));
    }
}

@ignore@

[maxvertexcount(75)]//75
void main(point GSIn points[1], inout TriangleStream<GSOut> output) {
    float PI = 3.14159265359f;
    float radius = 1.5;

    float3 look = -normalize(v4LookDirection.xyz);
    float i = 10 * (PI / 180);
    GSOut fs = (GSOut)0;
    float3 sphCenter = points[0].position.xyz;
    float4 color = points[0].color;
    float3 center = sphCenter + look * radius;
    float3 tangent = cross(look, float3(1, 0, 0));

    float3 N = look;
    float3x3 TBN = fromAxisAngle3x3(i, N);

    output.Append(createVertex(sphCenter, center, color));
    output.Append(createVertex(sphCenter, center + tangent * radius, color));
    tangent = normalize(mul(tangent, TBN));
    output.Append(createVertex(sphCenter, center + tangent * radius, color));

    for (float angle = 10; angle < 360; angle += 10) {
        output.Append(createVertex(sphCenter, center, color));

        tangent = normalize(mul(tangent, TBN));

        output.Append(createVertex(sphCenter, center + tangent * radius, color));
    }    
}

void main(point GSIn points[1], inout TriangleStream<GSOut> output) {
    float pi = acos(-1);
    float radius = 2.5;

    float3 look = float3(0, 0, 1);// -normalize(v4LookDirection.xyz);
    float3 right = float3(1, 0, 0);// normalize(c ross(look, v4CameraUp.xyz));

    float3 sphCenter = points[0].positionW.xyz;
    float4 color = points[0].color;

    float3x3 TBN = fromAxisAngle3x3(10 * (pi / 180), look);

    float c, s;
    float3 tangent = right;
    float3 center = sphCenter + look * radius;
    for (float angle = 0; angle < 360; angle += 10) {
        GSOut fs = (GSOut)0;
        fs.position = toScreen(sphCenter);
        fs.normal = -v4LookDirection;
        fs.color = color;
        output.Append(fs);

        tangent = normalize(mul(tangent, TBN));
        // tangent = normalize(Transform(tangent, float4(look, 10 * (pi / 180))));

        float3 pWorld = sphCenter + tangent * radius;

        fs = (GSOut)0;
        fs.position = toScreen(pWorld);
        fs.normal = -v4LookDirection; //float4(fs.position - center, 0);
        fs.color = color;
        output.Append(fs);
    }
}

void main(point GSIn points[1], inout TriangleStream<GSOut> output) {
    float pi = acos(-1);
    float radius = 0.5;

    float3 look = -normalize(v4LookDirection.xyz);
    float3 up = v4CameraUp;
    float3 right = dot(look, up);

    float3 sphCenter = points[0].positionW.xyz;
    float4 color = points[0].color;

    float3x3 toLook = fromAxisAngle3x3(dot(float3(0, 0, 1), look), float3(1, 0, 0));
    //float3x3 toTangent = fromAxisAngle3x3(dot(float3(0, 0, 1), look), float3(1, 0, 0));


    float c, s;
    for (float angle = 0; angle < 360; angle += 10) {
        float rad = angle * pi / 180;
        sincos(rad, s, c);
        float x = sphCenter.x + (radius * c);
        float y = sphCenter.y + (radius * s);

        float3 p_XY_World = float3(x, y, sphCenter.z);
        float3 pWorld = (sphCenter + up * (radius * c)) + right * (radius * s);


        GSOut fs = (GSOut)0;
        fs.position = toScreen(sphCenter);
        fs.normal = v4LookDirection;
        fs.color = color;
        output.Append(fs);

        fs = (GSOut)0;
        fs.position = toScreen(pWorld);
        fs.normal = float4(fs.position - sphCenter, 0);
        fs.color = color;
        output.Append(fs);
    }
}