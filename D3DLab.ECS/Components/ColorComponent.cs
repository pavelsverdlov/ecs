using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Components {
    public interface IColoringMaterialComponent : IGraphicComponent {
        
    }


    public enum ColorTypes {
        Undefined,
        Ambient,
        Diffuse,
        Specular,
        Reflection,
    }

    /// <summary>
    /// Uses in shaider as it is, no lighting or shading just exact color,
    /// general uses for VertexBuffer
    /// </summary>
    public readonly struct ColorComponent : IColoringMaterialComponent {
        public static ColorComponent CreateDiffuse(Vector4 color) {
            return new ColorComponent(color, ColorTypes.Diffuse);
        }
        public static ColorComponent CreateAmbient(Vector4 color) {
            return new ColorComponent(color, ColorTypes.Ambient);
        }
        public static ColorComponent CreateSpecular(Vector4 color) {
            return new ColorComponent(color, ColorTypes.Specular);
        }

        public Vector4 Color { get; }
        public ColorTypes Type { get; }

        public ElementTag Tag { get; }
        public bool IsModified { get; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        ColorComponent(Vector4 color, ColorTypes type) : this() {
            IsValid = true;
            Tag = ElementTag.New();
            Color = color;
            Type = type;
        }

        public void Dispose() {
        }

        public ColorComponent ApplyOpacity(float op) {
            return new ColorComponent(new Vector4(Color.X, Color.Y, Color.Z, op), Type);
        }
    }
}
