using D3DLab.Toolkit.Components;
using System.Numerics;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit._CommonShaders {

    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialStructBuffer {
        public static MaterialStructBuffer From(MaterialColorComponent color) {
            var material = new MaterialStructBuffer();

            material.ColorDiffuse = color.Diffuse;
            material.ColorAmbient = color.Ambient;
            material.ColorSpecular = color.Specular;
            material.ColorReflection = color.Reflection;
            material.SpecularFactor = color.SpecularFactor;

            return material;
        }

        public const int RegisterResourceSlot = 3;

        public Vector4 ColorAmbient;
        public Vector4 ColorDiffuse;
        public Vector4 ColorSpecular;
        public Vector4 ColorReflection;
        public float SpecularFactor;

        Vector3 offset;
    }
}
