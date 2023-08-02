using D3DLab.ECS;
using D3DLab.SDX.Engine.ProxyDevice;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine {
    public static class GraphicsDeviceFactory {

        static Adapter GetAdapter() {
            var factory = new Factory1();
            return AdapterFactory.GetBestAdapter(factory);
        }

        public static GraphicsDevice CreateOutputHandleDevice(IRenderableWindow window) {
            var adapter = GetAdapter();
            var proxy = new RenderToHandleDeviceProxy(adapter, window.Handle, window.Size);
            return new GraphicsDevice(proxy, window.Size, adapter.Description);
        }

        public static GraphicsDevice CreateOutputTextureDevice(IRenderableSurface window) {
            var adapter = GetAdapter();
            var proxy = new RenderToTextureDeviceProxy(adapter, window.Size);
            return new GraphicsDevice(proxy, window.Size, adapter.Description);
        }

        public static GraphicsDevice CreateOutputTargetView(IFrameRenderableSurface surface) {
            var adapter = GetAdapter();
            var proxy = new RenderToTargetViewDeviceProxy(adapter, surface.Size);
            return new GraphicsDevice(proxy, surface.Size, adapter.Description);
        }


    }

}
