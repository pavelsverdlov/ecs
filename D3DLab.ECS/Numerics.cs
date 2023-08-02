using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace System.Numerics {
    public readonly struct Ray {
        public readonly Vector3 Position;
        public readonly Vector3 Direction;

        public Ray(Vector3 or, Vector3 dir) {
            Position = or;
            Direction = dir;
        }
        public Ray Transformed(Matrix4x4 m) {
            var or = Vector3.Transform(Position, m);
            var dir = Vector3.TransformNormal(Direction, m);
            return new Ray(or, dir);
        }

        public Ray Inverted() {
            return new Ray(Position, -Direction);
        }
    }
    
}
