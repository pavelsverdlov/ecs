using System;
using System.Collections.Generic;
using System.Text;

namespace D3DLab.ECS.Common {
    public readonly struct GraphicSurfaceSize {
        public GraphicSurfaceSize(int width, int height) {
            Width = width;
            Height = height;
        }

        public int Width { get; }
        public int Height { get; }

        public static implicit operator SurfaceSize(GraphicSurfaceSize s)
            => new SurfaceSize(s.Width, s.Height);
        public static explicit operator GraphicSurfaceSize(SurfaceSize s)
            => new GraphicSurfaceSize((int)s.Width, (int)s.Height);
    }
}
