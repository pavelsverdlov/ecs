using D3DLab.ECS.Ext;
using D3DLab.Toolkit.Math3D;

using g3;

using SharpDX;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace System.Numerics {
    public enum AlignedBoxContainmentType {
        Disjoint,
        Contains,
        Intersects,
    }

    public struct BoundingBoxCorners {
        public Vector3 NearTopLeft;
        public Vector3 NearTopRight;
        public Vector3 NearBottomLeft;
        public Vector3 NearBottomRight;
        public Vector3 FarTopLeft;
        public Vector3 FarTopRight;
        public Vector3 FarBottomLeft;
        public Vector3 FarBottomRight;

        public Vector3[] ToArray() {
            return new[] {
                NearTopLeft,
                NearTopRight,
                NearBottomLeft,
                NearBottomRight,
                FarTopLeft,
                FarTopRight,
                FarBottomLeft,
                FarBottomRight
            };
        }
    }

    public readonly struct AxisAlignedBox {
        public static AxisAlignedBox Zero => new AxisAlignedBox();

        public bool HasNaN =>
            float.IsNaN(Minimum.X) || float.IsNaN(Minimum.Y) || float.IsNaN(Minimum.Z)
            || float.IsNaN(Maximum.X) || float.IsNaN(Maximum.Y) || float.IsNaN(Maximum.Z);
        public bool IsZero => Maximum.IsZero() && Minimum.IsZero();

        public Vector3 Size() {
            return new Vector3(
                Math.Abs(Minimum.X - Maximum.X),
                Math.Abs(Minimum.Y - Maximum.Y),
                Math.Abs(Minimum.Z - Maximum.Z));
        }

        public readonly Vector3 Minimum;
        public readonly Vector3 Maximum;
        public readonly Vector3 Center;
        public readonly Vector3 Dimensions;
        public readonly float Diagonal;

        readonly AxisAlignedBox3f boxf;
        readonly AxisAlignedBox3d boxd;


        public AxisAlignedBox(float w, float h, float l, Vector3 center) {
            var halfW = w * 0.5f;
            var halfH = h * 0.5f;
            var halfL = l * 0.5f;
            Center = center;
            Maximum = new Vector3(Center.X + halfW, Center.Y + halfL, Center.Z + halfH);
            Minimum = new Vector3(Center.X - halfW, Center.Y - halfL, Center.Z - halfH);
            Dimensions = Maximum - Minimum;
            Diagonal = Dimensions.Length();
            boxf = new AxisAlignedBox3f(Minimum.X, Minimum.Y, Minimum.Z, Maximum.X, Maximum.Y, Maximum.Z);
            boxd = boxf;
        }
        public AxisAlignedBox(Vector3 minimum, Vector3 maximum) {
            Minimum = minimum;
            Maximum = maximum;
            Center = (Maximum + Minimum) / 2f;
            Dimensions = Maximum - Minimum;
            Diagonal = Dimensions.Length();
            boxf = new AxisAlignedBox3f(Minimum.X, Minimum.Y, Minimum.Z, Maximum.X, Maximum.Y, Maximum.Z);
            boxd = boxf;
        }
        internal AxisAlignedBox(AxisAlignedBox3d box3d) {
            Minimum = box3d.Min.ToVector3();
            Maximum = box3d.Max.ToVector3();
            boxf = new AxisAlignedBox3f(Minimum.X, Minimum.Y, Minimum.Z, Maximum.X, Maximum.Y, Maximum.Z);
            boxd = box3d;
            Center = box3d.Center.ToVector3();
            Dimensions = Maximum - Minimum;
            Diagonal = Dimensions.Length();
        }

        //public bool Contains(ref Vector3 p) {
        //    return boxf.Contains(p.ToVector3f());
        //}
        public AlignedBoxContainmentType Contains(AxisAlignedBox other) {
            return Contains(ref other);
        }

        public AlignedBoxContainmentType ContainsSphere(Vector3 center, float radius) {
            var result = Vector3.Clamp(center, Minimum, Maximum);
            if (Vector3.DistanceSquared(center, result) > radius * radius) {
                return AlignedBoxContainmentType.Disjoint;
            }

            if (Minimum.X + radius <= center.X 
                && center.X <= Maximum.X - radius 
                && Maximum.X - Minimum.X > radius 
                && Minimum.Y + radius <= center.Y 
                && center.Y <= Maximum.Y - radius 
                && Maximum.Y - Minimum.Y > radius 
                && Minimum.Z + radius <= center.Z 
                && center.Z <= Maximum.Z - radius 
                && Maximum.Z - Minimum.Z > radius) {
                return AlignedBoxContainmentType.Contains;
            }

            return AlignedBoxContainmentType.Intersects;
        }


        public AlignedBoxContainmentType Contains(ref AxisAlignedBox other) {
            if (Maximum.X < other.Minimum.X || Minimum.X > other.Maximum.X
                || Maximum.Y < other.Minimum.Y || Minimum.Y > other.Maximum.Y
                || Maximum.Z < other.Minimum.Z || Minimum.Z > other.Minimum.Z) {
                return AlignedBoxContainmentType.Disjoint;
            } else if (Minimum.X <= other.Minimum.X && Maximum.X >= other.Maximum.X
                  && Minimum.Y <= other.Minimum.Y && Maximum.Y >= other.Maximum.Y
                  && Minimum.Z <= other.Minimum.Z && Maximum.Z >= other.Maximum.Z) {
                return AlignedBoxContainmentType.Contains;
            } else {
                return AlignedBoxContainmentType.Intersects;
            }
        }
        public bool Intersects(ref AxisAlignedBox bb) {
            return boxf.Intersects(bb.boxf);
        }
        public bool Intersects(ref Ray ray, out float distance)
            => RayIntersectsBox(ref ray, out distance);
        public AxisAlignedBox Merge(in AxisAlignedBox box) {
            return new AxisAlignedBox(
                   Vector3.Min(this.Minimum, box.Minimum),
                   Vector3.Max(this.Maximum, box.Maximum)
               );
        }

        public static bool operator ==(AxisAlignedBox first, AxisAlignedBox second) {
            return first.Equals(second);
        }

        public static bool operator !=(AxisAlignedBox first, AxisAlignedBox second) {
            return !first.Equals(second);
        }

        public bool Equals(AxisAlignedBox other) {
            return Minimum == other.Minimum && Maximum == other.Maximum;
        }
        public override string ToString() {
            return string.Format("Min:{0}, Max:{1}", Minimum, Maximum);
        }
        public override bool Equals(object obj) {
            return obj is AxisAlignedBox && ((AxisAlignedBox)obj).Equals(this);
        }
        public override int GetHashCode() {
            int h1 = Minimum.GetHashCode();
            int h2 = Maximum.GetHashCode();
            uint shift5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
            return ((int)shift5 + h1) ^ h2;
        }


        public Vector3[] GetCorners() {
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(Minimum.X, Maximum.Y, Maximum.Z);
            corners[1] = new Vector3(Maximum.X, Maximum.Y, Maximum.Z);
            corners[2] = new Vector3(Maximum.X, Minimum.Y, Maximum.Z);
            corners[3] = new Vector3(Minimum.X, Minimum.Y, Maximum.Z);
            corners[4] = new Vector3(Minimum.X, Maximum.Y, Minimum.Z);
            corners[5] = new Vector3(Maximum.X, Maximum.Y, Minimum.Z);
            corners[6] = new Vector3(Maximum.X, Minimum.Y, Minimum.Z);
            corners[7] = new Vector3(Minimum.X, Minimum.Y, Minimum.Z);
            return corners;
        }

        public unsafe AxisAlignedBox Transform(in Matrix4x4 mat) {
            var corners = GetCorners();

            Vector3 min = Vector3.Transform(corners[0], mat);
            Vector3 max = Vector3.Transform(corners[0], mat);

            for (int i = 1; i < 8; i++) {
                min = Vector3.Min(min, Vector3.Transform(corners[i], mat));
                max = Vector3.Max(max, Vector3.Transform(corners[i], mat));
            }

            return new AxisAlignedBox(min, max);
        }


        public static unsafe AxisAlignedBox CreateFrom(IReadOnlyList<Vector3> vertices) {
            Vector3 min = vertices[0];
            Vector3 max = vertices[0];

            for (int i = 1; i < vertices.Count; i++) {
                Vector3 pos = vertices[i];

                if (min.X > pos.X)
                    min.X = pos.X;
                if (max.X < pos.X)
                    max.X = pos.X;

                if (min.Y > pos.Y)
                    min.Y = pos.Y;
                if (max.Y < pos.Y)
                    max.Y = pos.Y;

                if (min.Z > pos.Z)
                    min.Z = pos.Z;
                if (max.Z < pos.Z)
                    max.Z = pos.Z;
            }

            return new AxisAlignedBox(min, max);
        }

        public static AxisAlignedBox FromSphere(Vector3 center, float radius) {

            Vector3 min = center + Vector3.UnitZ * -radius;
            Vector3 max = center + Vector3.UnitZ * radius;

            return new AxisAlignedBox(min, max);
        }

        public BoundingBoxCorners GetCornersBox() {
            BoundingBoxCorners corners;
            corners.NearBottomLeft = new Vector3(Minimum.X, Minimum.Y, Maximum.Z);
            corners.NearBottomRight = new Vector3(Maximum.X, Minimum.Y, Maximum.Z);
            corners.NearTopLeft = new Vector3(Minimum.X, Maximum.Y, Maximum.Z);
            corners.NearTopRight = new Vector3(Maximum.X, Maximum.Y, Maximum.Z);

            corners.FarBottomLeft = new Vector3(Minimum.X, Minimum.Y, Minimum.Z);
            corners.FarBottomRight = new Vector3(Maximum.X, Minimum.Y, Minimum.Z);
            corners.FarTopLeft = new Vector3(Minimum.X, Maximum.Y, Minimum.Z);
            corners.FarTopRight = new Vector3(Maximum.X, Maximum.Y, Minimum.Z);

            return corners;
        }

        #region math methods 

        bool RayIntersectsBox(ref Ray ray, out float distance) {
            //Source: Real-Time Collision Detection by Christer Ericson
            //Reference: Page 179

            distance = 0f;
            float tmax = float.MaxValue;

            if (D3DLab.ECS.MathUtil.IsZero(ray.Direction.X)) {
                if (ray.Position.X < Minimum.X || ray.Position.X > Maximum.X) {
                    distance = 0f;
                    return false;
                }
            } else {
                float inverse = 1.0f / ray.Direction.X;
                float t1 = (Minimum.X - ray.Position.X) * inverse;
                float t2 = (Maximum.X - ray.Position.X) * inverse;

                if (t1 > t2) {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = MathF.Max(t1, distance);
                tmax = MathF.Min(t2, tmax);

                if (distance > tmax) {
                    distance = 0f;
                    return false;
                }
            }

            if (D3DLab.ECS.MathUtil.IsZero(ray.Direction.Y)) {
                if (ray.Position.Y < Minimum.Y || ray.Position.Y > Maximum.Y) {
                    distance = 0f;
                    return false;
                }
            } else {
                float inverse = 1.0f / ray.Direction.Y;
                float t1 = (Minimum.Y - ray.Position.Y) * inverse;
                float t2 = (Maximum.Y - ray.Position.Y) * inverse;

                if (t1 > t2) {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = MathF.Max(t1, distance);
                tmax = MathF.Min(t2, tmax);

                if (distance > tmax) {
                    distance = 0f;
                    return false;
                }
            }

            if (D3DLab.ECS.MathUtil.IsZero(ray.Direction.Z)) {
                if (ray.Position.Z < Minimum.Z || ray.Position.Z > Maximum.Z) {
                    distance = 0f;
                    return false;
                }
            } else {
                float inverse = 1.0f / ray.Direction.Z;
                float t1 = (Minimum.Z - ray.Position.Z) * inverse;
                float t2 = (Maximum.Z - ray.Position.Z) * inverse;

                if (t1 > t2) {
                    float temp = t1;
                    t1 = t2;
                    t2 = temp;
                }

                distance = MathF.Max(t1, distance);
                tmax = MathF.Min(t2, tmax);

                if (distance > tmax) {
                    distance = 0f;
                    return false;
                }
            }

            return true;
        }

        #endregion
    }

    public readonly struct OrientedBoundingBox {
        //https://www.geometrictools.com/Samples/Geometrics.html#MinimumVolumeBox3D
        //https://www.geometrictools.com/GTE/Samples/Geometrics/MinimumVolumeBox3D/MinimumVolumeBox3DWindow3.cpp
        public static OrientedBoundingBox Zero => new OrientedBoundingBox(Box3d.Empty);

        public static unsafe OrientedBoundingBox CreateFrom(IReadOnlyList<Vector3> vertices) {

            var box = new ContOrientedBox3(vertices.Select(x => x.ToVector3d()));
            if (box.ResultValid) {
                return new OrientedBoundingBox(box.Box);
            }
            return Zero;
        }

        readonly Box3d box3d;

        OrientedBoundingBox(Box3d box3d) {
            this.box3d = box3d;
        }

        public BoundingBoxCorners GetCornersBox() {
            BoundingBoxCorners corners;
            var center = box3d.Center;
            var axisX = box3d.AxisX;
            var axisY = box3d.AxisY;
            var axisZ = box3d.AxisZ;
            var extends = box3d.Extent;

            var A = center - extends.z * axisZ - extends.x * axisX - axisY * extends.y;
            var B = center - extends.z * axisZ + extends.x * axisX - axisY * extends.y;
            var C = center - extends.z * axisZ + extends.x * axisX + axisY * extends.y;
            var D = center - extends.z * axisZ - extends.x * axisX + axisY * extends.y;

            var E = center + extends.z * axisZ - extends.x * axisX - axisY * extends.y;
            var F = center + extends.z * axisZ + extends.x * axisX - axisY * extends.y;
            var G = center + extends.z * axisZ + extends.x * axisX + axisY * extends.y;
            var H = center + extends.z * axisZ - extends.x * axisX + axisY * extends.y;


            corners.NearBottomLeft = A.ToVector3();
            corners.NearBottomRight = B.ToVector3();
            corners.NearTopLeft = C.ToVector3();
            corners.NearTopRight = D.ToVector3();

            corners.FarBottomLeft = E.ToVector3();
            corners.FarBottomRight = F.ToVector3();
            corners.FarTopLeft = G.ToVector3();
            corners.FarTopRight = H.ToVector3();

            return corners;
        }
    }
}
