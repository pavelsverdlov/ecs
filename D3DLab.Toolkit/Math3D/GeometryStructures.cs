using D3DLab.ECS;
using D3DLab.ECS.Ext;
using D3DLab.FileFormats.GeoFormats;

using g3;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.Toolkit.Math3D {
    public class GeometryStructures<TFormat> : GeometryStructures
        where TFormat : IFileGeometry3D {

        public TFormat OriginGeometry { get; }

        public GeometryStructures(TFormat geo) {
            OriginGeometry = geo;
            Positions = geo.Positions.ToImmutableArray();
            Indices = geo.Indices.ToImmutableArray();
            Normals = geo.Normals.ToImmutableArray();
            TexCoor = geo.TextureCoors.ToImmutableArray();
            Topology = geo.Topology;
        }
    }


    class ParametricSphereStructures : ParametricSurfaceStructures {
        readonly Vector3 center;
        readonly float radius;
        public ParametricSphereStructures(Vector3 center, float radius) {
            this.center = center;
            this.radius = radius;
        }

        public override IEnumerable<HitResultLocal> HitByLocal(Ray rayLocal) {
            var dir = rayLocal.Direction;
            var pos = rayLocal.Position;

            var v0 = center - pos;
            v0.Normalize();

            var v1 = pos + dir * Vector3.Dot(v0, dir);

            var a = (v1 - center).Length();
            if (a < radius) {
                var b = MathF.Sqrt(MathF.Pow(a,2) - MathF.Pow(radius, 2));
                var crossPoint = v1 + dir * -b;

                return new[] {
                    new HitResultLocal(crossPoint, (crossPoint -pos).Length(), true)
                };
            }

            return Enumerable.Empty<HitResultLocal>();
        }
    }
    public abstract class ParametricSurfaceStructures : IGeometryData {

        public static IGeometryData AsParametricSphere(Vector3 center, float radius) {
            //  hit = x => Enumerable.Empty<HitResultLocal>();
            var geo = GeometryBuilder.BuildSphere(center, radius);

            return new ParametricSphereStructures(center, radius) {
                Positions = geo.Positions,
                Indices = geo.Indices,
                Colors = geo.Colors,
                Normals = geo.Normals,
                TexCoor = geo.TexCoor,
                Topology = geo.Topology,
                IsModified = true,
            };
        }

        public ImmutableArray<Vector3> Positions { get; protected set; }
        public ImmutableArray<int> Indices { get; protected set; }
        public ImmutableArray<Vector4> Colors { get; protected set; }
        public ImmutableArray<Vector3> Normals { get; protected set; }
        public ImmutableArray<Vector2> TexCoor { get; protected set; }
        public GeometryPrimitiveTopologies Topology { get; protected set; }

        public bool IsModified { get; set; }
        public bool IsDisposed { get; protected set; }
        public virtual void Dispose() {
          
        }

        public abstract IEnumerable<HitResultLocal> HitByLocal(Ray rayLocal);
    }

    public abstract class GeometryStructures : IGeometryData {
        public ImmutableArray<Vector3> Positions { get; protected set; }
        public ImmutableArray<int> Indices { get; protected set; }
        public ImmutableArray<Vector4> Colors { get; protected set; }
        public ImmutableArray<Vector3> Normals { get; protected set; }
        public ImmutableArray<Vector2> TexCoor { get; protected set; }
        public GeometryPrimitiveTopologies Topology { get; protected set; }

        public bool IsModified { get; set; }
        public bool IsDisposed { get; private set; }
        public bool IsBuilt { get; private set; }
        public AxisAlignedBox Bounds { get; private set; }


        DMeshAABBTree3 TreeLocal;
        DMesh3 DMeshLocal;

        public Task BuildTreeAsync() {
            return Task.Run(() => {
                try {
                    var norm = Normals.ConvertToVector3f();
                    DMeshLocal = DMesh3Builder.Build(Positions.ConvertToVector3f(), Indices, norm);

                    TreeLocal = new DMeshAABBTree3(DMeshLocal);
                    TreeLocal.Build();

                    Bounds = new AxisAlignedBox(DMeshLocal.GetBounds());

                    IsBuilt = true;
                } catch (Exception ex) {
                    Debug.WriteLine($"BuildTreeAsync {ex.Message}");
                }
                return this;
            });
        }
        public IEnumerable<HitResultLocal> HitByLocal(Ray rayLocal) {
            HitResultLocal res = default;
            var ray = new Ray3d(rayLocal.Position.ToVector3f(), rayLocal.Direction.ToVector3f());
            try {
                int hit_tid = TreeLocal.FindNearestHitTriangle(ray);
                if (hit_tid == DMesh3.InvalidID) {
                    return Enumerable.Empty<HitResultLocal>();
                }

                var intr = MeshQueries.TriangleIntersection(DMeshLocal, hit_tid, ray);
                var distance = (float)ray.Origin.Distance(ray.PointAt(intr.RayParameter));
                var point = intr.Triangle.V1.ToVector3();
                res = new HitResultLocal(point, distance, true);
            } catch (Exception ex) {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }

            return new[] { res };
        }
        public void ReCalculateNormals() {
            Normals = Positions.ToList().CalculateNormals(Indices.ToList()).ToImmutableArray();
        }

        public virtual void Dispose() {
            IsDisposed = true;
            TreeLocal = null;
            DMeshLocal = null;
            IsBuilt = false;
            Bounds = AxisAlignedBox.Zero;
        }

    }
}
