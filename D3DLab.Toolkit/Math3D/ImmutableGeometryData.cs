using D3DLab.ECS;
using D3DLab.ECS.Ext;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// https://devblogs.microsoft.com/dotnet/please-welcome-immutablearrayt/
    /// https://www.infoq.com/articles/For-Each-Performance/
    /// 
    /// Use ReadOnlyCollection, because creating a ReadOnlyCollection<T> wrapper is an O(1) operation, 
    /// and does not incur any performance cost for array or list
    /// 
    /// use ImmutableArray because iteration perfomance is almost best :) and allow to expand to new ImmutableArray
    /// </remarks>
    public class ImmutableGeometryData : IGeometryData {
        public ImmutableArray<Vector3> Positions { get; private set; }
        public ImmutableArray<int> Indices { get; private set; }
        public ImmutableArray<Vector3> Normals { get; private set; }
        public ImmutableArray<Vector2> TexCoor { get; private set; }
        public ImmutableArray<Vector4> Colors { get; private set; }
        public bool IsModified { get; set; }
        public bool IsDisposed { get; private set; }
        public GeometryPrimitiveTopologies Topology { get; private set; }

        public ImmutableGeometryData(IReadOnlyCollection<Vector3> positions, IReadOnlyCollection<int> indices) 
            : this(positions, null, indices, null) {
        }
        public ImmutableGeometryData(IReadOnlyCollection<Vector3> positions, IReadOnlyCollection<int> indices,
            IReadOnlyCollection<Vector4> colors) : this(positions, null, indices, null, colors) {
        }
        public ImmutableGeometryData(IReadOnlyCollection<Vector3> positions,
           IReadOnlyCollection<Vector3> normals, IReadOnlyCollection<int> indices) 
            : this(positions, normals, indices,null) {
        }
        public ImmutableGeometryData(IReadOnlyCollection<Vector3> positions,
            IReadOnlyCollection<Vector3> normals, IReadOnlyCollection<int> indices,
            IReadOnlyCollection<Vector2> texCoor) 
            :this(positions, normals, indices, texCoor, null){
        }
        public ImmutableGeometryData(IReadOnlyCollection<Vector3> positions,
            IReadOnlyCollection<Vector3> normals, IReadOnlyCollection<int> indices,
            IReadOnlyCollection<Vector2> texCoor, IReadOnlyCollection<Vector4> colors) {

            Positions = positions == null ? ImmutableArray<Vector3>.Empty : positions.ToImmutableArray();
            Normals = normals == null ? ImmutableArray<Vector3>.Empty : positions.ToImmutableArray();
            Indices = indices == null ? ImmutableArray<int>.Empty : indices.ToImmutableArray();
            TexCoor = texCoor == null ? ImmutableArray<Vector2>.Empty : texCoor.ToImmutableArray();
            Colors = colors == null ? ImmutableArray<Vector4>.Empty : colors.ToImmutableArray();
            IsModified = true;
            Topology = GeometryPrimitiveTopologies.TriangleList;
        }


        public void ReCalculateNormals() {
            Normals = Positions.ToList().CalculateNormals(Indices.ToList()).ToImmutableArray();
        }

        public virtual void Dispose() {
            IsDisposed = true;
            Positions = ImmutableArray<Vector3>.Empty;
            Normals = ImmutableArray<Vector3>.Empty;
            Indices = ImmutableArray<int>.Empty;
            TexCoor = ImmutableArray<Vector2>.Empty;
            Colors = ImmutableArray<Vector4>.Empty;
        }

        public ImmutableGeometryData Transform(Matrix4x4 matrix) {
            var p = Positions.ToArray().Transform(ref matrix);
            var n = Normals.ToArray().Transform(ref matrix);

            return new ImmutableGeometryData(p.AsReadOnly(), n.AsReadOnly(), Indices) {
                Colors = Colors,
                TexCoor = TexCoor,
                Topology = Topology,
                IsModified = true,
            };

        }

        public IEnumerable<HitResultLocal> HitByLocal(Ray rayLocal) {
            throw new NotImplementedException();
        }
    }
}
