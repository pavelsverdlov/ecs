using D3DLab.ECS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Components {
    /// <summary>
    /// Necessary for Octree
    /// </summary>
    public readonly struct GeometryBoundsComponent : IGraphicComponent {
        public static GeometryBoundsComponent Create(AxisAlignedBox bounds) {
            return new GeometryBoundsComponent(bounds);
        }
        public static GeometryBoundsComponent Create(IReadOnlyList<Vector3> vertices) {
            return new GeometryBoundsComponent(AxisAlignedBox.CreateFrom(vertices));
        }


        public AxisAlignedBox Bounds { get; }

        GeometryBoundsComponent(AxisAlignedBox bounds) : this() {
            Tag = ElementTag.New();
            Bounds = bounds;
            IsValid = true;
        }

        public ElementTag Tag { get; }
        public bool IsModified { get;  }
        public bool IsValid { get;  }
        public bool IsDisposed { get; }

        public void Dispose() {
        }

        public GeometryBoundsComponent ApplyTransform(in Matrix4x4 mat) {
            return Create(Bounds.Transform(mat));
        }
    }
}
