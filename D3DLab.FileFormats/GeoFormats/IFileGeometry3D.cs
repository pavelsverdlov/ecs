using D3DLab.ECS;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;

namespace D3DLab.FileFormats.GeoFormats {
    /// <summary>
    /// A ReadOnlyCollection can support multiple readers concurrently,
    /// as long as the collection is not modified. Even so, 
    /// enumerating through a collection is intrinsically not a thread-safe procedure.
    /// To guarantee thread safety during enumeration,
    /// you can lock the collection during the entire enumeration.
    /// </summary>
    public interface IFileGeometry3D {
        string Name { get; }
        ReadOnlyCollection<Vector3> Positions { get; }
        ReadOnlyCollection<Vector3> Normals { get; }
        ReadOnlyCollection<int> Indices { get; }
        ReadOnlyCollection<Vector2> TextureCoors { get; }
        ReadOnlyCollection<Vector4> Colors { get; }
        GeometryPrimitiveTopologies Topology { get; }

        IFileGeometry3D ApplyMatrix(ref Matrix4x4 matrix);
    }
}
