using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine.Rendering {
    public class GraphicsFrame : IDisposable {
        public readonly GraphicsDevice Graphics;
        readonly Stopwatch sw;
        TimeSpan spendTime;

        public GraphicsFrame(GraphicsDevice device) {
            this.Graphics = device;
            sw = new Stopwatch();
            sw.Start();
            device.Refresh();
        }
        public void Dispose() {
            Graphics.Present();
            OnDispose();
            sw.Stop();
            spendTime = sw.Elapsed;
        }
        protected virtual void OnDispose() { }
    }
    public class GraphicsFrameWithSurface : GraphicsFrame {
        readonly IFrameRenderableSurface surface;
        public GraphicsFrameWithSurface(GraphicsDevice device, IFrameRenderableSurface surface) : base(device) {
            this.surface = surface;
            surface.StartFrame(Graphics);
        }
        protected override void OnDispose() {
            surface.EndFrame(Graphics);
        }
    }
}
