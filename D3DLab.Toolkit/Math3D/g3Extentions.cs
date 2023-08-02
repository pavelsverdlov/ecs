using g3;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    static class g3Extentions {
        public static Vector3f ToVector3f(this Vector3 v) {
            return new Vector3f(v.X, v.Y, v.Z);
        }
         public static Vector3d ToVector3d(this Vector3 v) {
            return new Vector3d(v.X, v.Y, v.Z);
        }
        public static Vector3 ToVector3(this Vector3f v) {
            return new Vector3(v.x, v.y, v.z);
        }
        public static Vector3 ToVector3(this Vector3d v) {
            return new Vector3((float)v.x, (float)v.y, (float)v.z);
        }


        public static Vector3f[] ConvertToVector3f(this ImmutableArray<Vector3> pos) {
            var v3f = new Vector3f[pos.Length];
            for (var i = 0; i < v3f.Length; ++i) {
                v3f[i] = pos[i].ToVector3f();
            }
            return v3f;
        }
    }
}
