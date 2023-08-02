using System;
using System.Numerics;

namespace D3DLab.ECS.Ext {
    public static class PlaneEx {
        public static void Normalize(this Plane plane) {
            var normal = plane.Normal;
            float magnitude = 1.0f / (float)(Math.Sqrt((normal.X * normal.X) + (normal.Y * normal.Y) + (normal.Z * normal.Z)));

            plane.Normal.X *= magnitude;
            plane.Normal.Y *= magnitude;
            plane.Normal.Z *= magnitude;
            plane.D *= magnitude;
        }
    }
}
