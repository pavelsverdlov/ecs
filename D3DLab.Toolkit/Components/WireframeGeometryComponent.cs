using D3DLab.ECS;
using System;

namespace D3DLab.Toolkit.Components {
    public readonly struct WireframeGeometryComponent : IGraphicComponent {
        public static WireframeGeometryComponent Create() => new WireframeGeometryComponent(true);

        public ElementTag Tag { get;  }
        public bool IsModified { get; }
        public bool IsValid { get; }
        public bool IsDisposed { get; }
        public WireframeGeometryComponent(bool isValid) : this() {
            IsValid = isValid;
            Tag = ElementTag.New();
        }
        public void Dispose() { }
    }
}
