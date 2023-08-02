using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine.ProxyDevice {
    abstract class DirectX11Proxy {
        public Device D3DDevice;
        public DeviceContext ImmediateContext;//readonly
        public RenderTargetView RenderTarget;

        public virtual void Dispose() {
            RenderTarget.Dispose();

            if (!ImmediateContext.IsDisposed) {
                ImmediateContext.ClearState();
                ImmediateContext.Flush();
                ImmediateContext.Dispose();
            }

            D3DDevice.Dispose();
        }
        public abstract Texture2D GetBackBuffer();
        public abstract void Present();
        public abstract void Resize(int width, int height);

        protected RasterizerState CreateRasterizerState(RasterizerStateDescription description) {
            return new RasterizerState(D3DDevice, description);
        }
        public abstract RasterizerState CreateRasterizerState(RasterizerStateDescription2 description2);
    }
}
