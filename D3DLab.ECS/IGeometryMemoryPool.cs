using D3DLab.ECS.Components;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS {
    public enum GeometryPrimitiveTopologies {
        Undefined,
        TriangleList,
        TriangleFan,
        LineStrip,
        LineList
    }

    public readonly struct HitResultLocal {
        public static HitResultLocal Empty() => new HitResultLocal(Vector3.Zero, -1, true);
        public readonly Vector3 Point;
        public readonly float Distance;
        public readonly bool IsHitted;

        public HitResultLocal(Vector3 point, float distance, bool isHitted) {
            Point = point;
            Distance = distance;
            IsHitted = isHitted;
        }
    }

    /// <summary>
    /// ImmutableArray is immutable and thus inherently thread safe.
    /// </summary>
    public interface IGeometryData : IDisposable {
        ImmutableArray<Vector3> Positions { get; }
        ImmutableArray<int> Indices { get; }
        ImmutableArray<Vector4> Colors { get; }
        ImmutableArray<Vector3> Normals { get; }
        ImmutableArray<Vector2> TexCoor { get; }
        GeometryPrimitiveTopologies Topology { get; } 
        bool IsModified { get; set; }
        bool IsDisposed { get; }

        IEnumerable<HitResultLocal> HitByLocal(Ray rayLocal);
    }

    public interface IGeometryMemoryPool : IDisposable {
        TGeoData GetGeometry<TGeoData>(GraphicEntity entity) where TGeoData : IGeometryData;
        TGeoData GetGeometry<TGeoData>(GeometryPoolComponent com) where TGeoData : IGeometryData;
        GeometryPoolComponent AddGeometry<TGeoData>(TGeoData geo) where TGeoData : IGeometryData;
        void AddGeometry<TGeoData>(GraphicEntity entity, TGeoData data) where TGeoData : IGeometryData;

        GeometryPoolComponent UpdateGeometry<TGeoData>(GeometryPoolComponent old, TGeoData newGeo)
            where TGeoData : IGeometryData;
    }
}
