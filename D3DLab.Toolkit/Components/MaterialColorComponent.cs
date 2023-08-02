using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace D3DLab.Toolkit.Components {
    public readonly struct MaterialColorComponent : IGraphicComponent {
        
        public static MaterialColorComponent CreateTransparent(float specularFactor = 400f) {
            var color = Vector4.Zero; 
            return new MaterialColorComponent(color, color, color, color, 400f, false, true);
        }
        public static MaterialColorComponent Create(Vector4 color, float specularFactor =  400f) {
            return new MaterialColorComponent(color, color, color, color, specularFactor, color.W < 1, true);
        }

        public MaterialColorComponent Clone() {
            return new MaterialColorComponent(Ambient, Diffuse, Specular, Reflection, SpecularFactor, HasAlpha, true);
        }

        public Vector4 Ambient { get; }
        public Vector4 Diffuse { get; }
        public Vector4 Specular { get; }
        public Vector4 Reflection { get; }
        public float SpecularFactor { get; }

        public bool HasAlpha { get;  }

        public ElementTag Tag { get; }
        public bool IsModified { get;}
        public bool IsValid { get; }
        public bool IsDisposed { get;  }

        public MaterialColorComponent(
            Vector4 ambient, Vector4 diffuse, Vector4 specular, Vector4 reflection,
            float specularFactor, bool hasAlpha, bool isModified) : this() {
            Tag = ElementTag.New();
            IsValid = true;
            IsModified = isModified;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Reflection = reflection;
            SpecularFactor = specularFactor;
            HasAlpha = hasAlpha;
        }

        public void Dispose() {
        }
        public MaterialColorComponent ApplyAlpha(float alfa) {
            var a = Ambient;
            var d = Diffuse;
            var s = Specular;
            a.W = alfa;
            d.W = alfa;
            s.W = alfa;
            return new MaterialColorComponent(a,d,s,Reflection,SpecularFactor, alfa < 1, true);
        }
    }
}
