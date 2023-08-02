using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Components {
    public readonly struct FlatShadingGeometryComponent : IGraphicComponent {
        public static FlatShadingGeometryComponent Create() => new FlatShadingGeometryComponent(true);

        public ElementTag Tag { get;  }
        public bool IsModified { get;}
        public bool IsValid { get; }
        public bool IsDisposed { get; }

        public void Dispose() {}

        FlatShadingGeometryComponent(bool isValid) : this() {
            Tag = ElementTag.New();
            IsValid = isValid;
        }
    }
}
