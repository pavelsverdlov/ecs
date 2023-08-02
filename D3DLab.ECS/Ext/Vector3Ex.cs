using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.ECS.Ext {
    public static class Vector3Ex {

        public static Vector3 Add(this Vector3 v1, float x, float y, float z) 
            => new(v1.X + x, v1.Y + y, v1.Z + z);

        public static void Normalize(ref this Vector3 v) {
            var n = Vector3.Normalize(v);
            v.X = n.X;
            v.Y = n.Y;
            v.Z = n.Z;
        }
        public static Vector3 Normalized(this Vector3 v) {
            return Vector3.Normalize(v);

        }
        public static Vector3 Cross(this Vector3 v1, Vector3 v2) {
            return Vector3.Cross(v1, v2);
        }
        public static float AngleRad(this Vector3 u, Vector3 v) {
            return (float)Math.Atan2(Vector3.Cross(u, v).Length(), Vector3.Dot(u, v));
        }
        public static Vector4 ToVector4(this Vector3 v) {
            return new Vector4(v.X, v.Y, v.Z, 1);
        }
        public static Vector4 ToVector4(this Vector3 v, float w) {
            return new Vector4(v.X, v.Y, v.Z, w);
        }
        public static Vector3 XYZ(this Vector4 v) {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Vector3 Round(this Vector3 v, int round) {
            return new Vector3(
                (float)Math.Round(v.X, round),
                (float)Math.Round(v.Y, round),
                (float)Math.Round(v.Z, round));
        }
        public static bool IsZero(in this Vector3 v) {
            return Vector3.Zero == v;
        }


        public static Vector3 TransformedNormal(this Vector3 vector, in Matrix4x4 matrix) {
            return Vector3.TransformNormal(vector, matrix);
        }
        public static Vector3 TransformedCoordinate(this Vector3 vector, in Matrix4x4 matrix) {
            return Vector3.Transform(vector, matrix);
        }


        public static Matrix4x4 RotationAround(this Vector3 axis, float angleRadians) {
            return Matrix4x4.CreateFromAxisAngle(axis, angleRadians);
        }
        public static Matrix4x4 RotationAround(this Vector3 axis, float angle, in Vector3 center) {
            var m1 = Matrix4x4.CreateTranslation(center * -1f);
            var m2 = axis.RotationAround(angle);
            var m3 = Matrix4x4.CreateTranslation(center);
            var m = m1 * m2 * m3;
            return m;
        }
        public static Vector3 FindAnyPerpendicular(this Vector3 n) {
            n.Normalize();
            Vector3 u = Vector3.Cross(new Vector3(0, 1, 0), n);
            if (u.LengthSquared() < 1e-3) {
                u = Vector3.Cross(new Vector3(1, 0, 0), n);
            }

            return u;
        }
        //public static Vector4 ToVector4(this System.Windows.Media.Color color) {
        //    color.Clamp();
        //    return new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        //}
        public static bool EqualsRound(this Vector3 a, Vector3 b, int round) {
            return Math.Round(a.X, round) == Math.Round(b.X, round)
                && Math.Round(a.Y, round) == Math.Round(b.Y, round)
                && Math.Round(a.Z, round) == Math.Round(b.Z, round);
        }


        public static Vector3 GetCenter(this IReadOnlyList<Vector3> points) {
            var pathcenter = Vector3.Zero;
            for (var i = 0; i < points.Count; i++) {
                var vector3 = points[i];
                pathcenter = Vector3.Add(vector3, pathcenter);
            }
            return pathcenter / points.Count;
        }
    }
}
