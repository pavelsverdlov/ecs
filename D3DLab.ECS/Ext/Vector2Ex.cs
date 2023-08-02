using System;
using System.Numerics;

namespace D3DLab.ECS.Ext {
    public static class Vector2Ex {
        public static Vector2 Normalized(this Vector2 v) {
            return Vector2.Normalize(v);
        }
        public static float AngleRad(this Vector2 u, Vector2 v) {
            return (float)(Math.Atan2(v.Y, v.X) - Math.Atan2(u.Y, u.X));
        }
        public static Vector3 ToVector3(this Vector2 v, float z = 0) {
            return new Vector3(v, z);
        }
    }
}
