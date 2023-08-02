using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.Toolkit.Math3D {
    public static class MathUtil {
        public const float ZeroTolerance = 1e-6f; // Value a 8x higher than 1.19209290E-07F

        public static bool IsZero(float a) => MathF.Abs(a) < ZeroTolerance;
    }
}
