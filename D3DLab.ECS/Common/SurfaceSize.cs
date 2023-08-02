using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Common {
    public readonly struct SurfaceSize {
        public SurfaceSize(float width, float height) {
            Width = width;
            Height = height;
        }

        public float Width { get; }
        public float Height { get; }

        public static implicit operator GraphicSurfaceSize(SurfaceSize s)
           => new GraphicSurfaceSize((int)s.Width, (int)s.Height);
        public static explicit operator SurfaceSize(GraphicSurfaceSize s)
            => new SurfaceSize(s.Width, s.Height);
    }
}
