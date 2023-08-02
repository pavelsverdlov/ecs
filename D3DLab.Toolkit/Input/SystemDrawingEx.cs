using D3DLab.ECS.Input;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace System.Drawing {
    public static class SystemDrawingEx {
        public static Vector2 ToNumericsV2(this System.Drawing.Point v2) {
            return new Vector2(v2.X, v2.Y);
        }

        public static System.Drawing.Point ToDrawingPoint(this WindowPoint p) {
            return new System.Drawing.Point(p.X, p.Y);
        }
        public static Vector2 ToNumericsV2(this System.Windows.Point p) {
            return new Vector2((float)p.X, (float)p.Y);
        }

        public static WindowPoint ToWindowPoint(this System.Drawing.Point p) {
            return new WindowPoint(p.X, p.Y);
        }
        public static WindowPoint ToWindowPoint(this System.Windows.Point p) {
            return new WindowPoint((int)p.X, (int)p.Y);
        }
    }
}
