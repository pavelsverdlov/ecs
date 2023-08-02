using System;

namespace D3DLab.ECS.Ext {
    public static class ConvertorsEx {
        public static float ToRad(this float degrees) {
            return (float)(degrees * Math.PI / 180f);
        }
        public static float ToDeg(this float radians) {
            return (float)(radians * 180f / Math.PI);
        }
    }
}
