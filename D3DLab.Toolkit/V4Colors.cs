using D3DLab.ECS;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace D3DLab.Toolkit {
    public static class V4Colors {
        public static readonly Vector4 White = new(1.0f, 1.0f, 1.0f, 1.0f);
        public static readonly Vector4 Green = new(0, 1, 0, 1);
        public static readonly Vector4 DarkGreen = new(0, 0.5019608f, 0, 1);
        public static readonly Vector4 Red = new(1, 0, 0, 1);
        public static readonly Vector4 Blue = new(0, 0, 1, 1);
        public static readonly Vector4 Yellow = new(1, 1, 0, 1);
        public static readonly Vector4 Magenta = new(1, 0, 1, 1);
        public static readonly Vector4 Violet = new(0.93333334f, 0.50980395f,0.93333334f, 1);
        public static readonly Vector4 Transparent = Vector4.Zero;


        public static Vector4 NextColor(this Random random) {
            
            return new Vector4(random.NextFloat(0.0f, 1.0f), random.NextFloat(0.0f, 1.0f), random.NextFloat(0.0f, 1.0f), 1.0f);
        }
    }
}
