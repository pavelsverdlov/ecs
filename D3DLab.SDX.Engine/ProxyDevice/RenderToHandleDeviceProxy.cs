using D3DLab.ECS;
using D3DLab.ECS.Common;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine.ProxyDevice {
    class RenderToHandleDeviceProxy : DirectX11Proxy {
        readonly SwapChain swapChain;
        readonly SwapChain1 swapChain1;
        readonly SwapChain3 swapChain3;

        readonly SharpDX.Direct3D11.Device3 device3;

        public RenderToHandleDeviceProxy(Adapter adapter, IntPtr handle, GraphicSurfaceSize size) {

            var backBufferDesc = new ModeDescription(size.Width, size.Height, new Rational(60, 1), GraphicsDevice.BackBufferTextureFormat);
          
            // Descriptor for the swap chain
            var swapChainDesc = new SwapChainDescription() {
                ModeDescription = backBufferDesc,
                SampleDescription = new SampleDescription(1, 0),
                BufferCount = 2,
                IsWindowed = true,
                OutputHandle = handle,
                Usage = Usage.RenderTargetOutput,
                Flags = SwapChainFlags.None,
               // SwapEffect = SwapEffect.FlipSequential
            };
            // Create device and swap chain
            var flags = DeviceCreationFlags.None;
#if DEBUG
            //flags |= DeviceCreationFlags.Debug;
#endif
            if (SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(adapter, FeatureLevel.Level_11_1)) { //update win->dxdiag
                                                                                                       //flags |= DeviceCreationFlags.Debuggable;
            }
            if (SharpDX.Direct3D11.Device.IsSupportedFeatureLevel(adapter, FeatureLevel.Level_12_0)) {

            }

            //using (var factory = adapter.GetParent<Factory2>()) {
            //    var device11 = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.None,
            //       new[] {
            //               FeatureLevel.Level_12_1,
            //               FeatureLevel.Level_12_0,
            //               FeatureLevel.Level_11_1,
            //               FeatureLevel.Level_11_0,
            //       });
            //    var desc1 = new SwapChainDescription1() {
            //        Width = width,
            //        Height = height,
            //        Format = BackBufferTextureFormat,
            //        Stereo = false,
            //        SampleDescription = new SampleDescription(1, 0),
            //        Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
            //        BufferCount = 1,
            //        Scaling = Scaling.Stretch,
            //        SwapEffect = SwapEffect.Discard,

            //    };
            //    swapChain = new SwapChain1(factory, device11, handle, ref desc1,
            //        new SwapChainFullScreenDescription() {
            //            RefreshRate = new Rational(60, 1),
            //            Scaling = DisplayModeScaling.Centered,
            //            Windowed = true
            //        },
            //        // Restrict output to specific Output (monitor)
            //        null);
            //}

            SharpDX.Direct3D11.Device.CreateWithSwapChain(adapter, flags, swapChainDesc, out var d3dDevice, out var sch);

            var wc = sch.QueryInterfaceOrNull<SwapChain1>();
            if (wc != null) {
                swapChain1 = wc;
            }
            var sc3 = sch.QueryInterfaceOrNull<SwapChain3>();
            if (sc3 != null) {
                swapChain3 = sc3;
            }

            swapChain = sch;//.QueryInterface<SwapChain4>();//

            var d3 = d3dDevice.QueryInterfaceOrNull<SharpDX.Direct3D11.Device3>(); //Device5
            if (d3 != null) {
                device3 = d3;
            }
            D3DDevice = d3dDevice;

            ImmediateContext = d3dDevice.ImmediateContext;

            // Enable object tracking
            //SharpDX.Configuration.EnableObjectTracking = true;
        }

        public override Texture2D GetBackBuffer() => swapChain.GetBackBuffer<Texture2D>(0);

        public override void Dispose() {
            base.Dispose();
            swapChain.Dispose();
            swapChain1.Dispose();
            swapChain3.Dispose();
            device3.Dispose();
            //ImmediateContext.Dispose();
        }

        public override void Present() {
            if (swapChain1 == null) {
                swapChain.Present(1, PresentFlags.None);
            } else {
                swapChain1.Present(1, PresentFlags.None, new PresentParameters());
            }

            try {//only for Window 10
                if (swapChain3 != null) {
                    WaitForSingleObjectEx(swapChain3.FrameLatencyWaitableObject.ToInt32(), 1000, true);
                }
            } catch { }
            // Output the current active Direct3D objects
            //System.Diagnostics.Debug.Write(SharpDX.Diagnostics.ObjectTracker.ReportActiveObjects());
        }

        public override void Resize(int width, int height) {
            swapChain.ResizeBuffers(2, width, height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);

            using (var backBuffer = swapChain.GetBackBuffer<Texture2D>(0)) {
                RenderTarget = new RenderTargetView(D3DDevice, backBuffer);
            }
        }

        public override RasterizerState CreateRasterizerState(RasterizerStateDescription2 description2) {
            if (device3 == null) {
                return base.CreateRasterizerState(GraphicsDevice.ToRasterizerDesc0(description2));
            }
            return new RasterizerState2(device3, description2);
        }


        void Resize(uint width, uint height) {
            float _pixelScale = 1;
            uint actualWidth = (uint)(width * _pixelScale);
            uint actualHeight = (uint)(height * _pixelScale);
            swapChain.ResizeBuffers(2, (int)actualWidth, (int)actualHeight, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

            // Get the backbuffer from the swapchain
            //using (Texture2D backBufferTexture = swapChain.GetBackBuffer<Texture2D>(0)) {
            //    if (_depthFormat != null) {
            //        TextureDescription depthDesc = new TextureDescription(
            //            actualWidth, actualHeight, 1, 1, 1,
            //            _depthFormat.Value,
            //            TextureUsage.DepthStencil,
            //            TextureType.Texture2D);
            //        _depthTexture = new D3D11Texture(_device, ref depthDesc);
            //    }

            //    D3D11Texture backBufferVdTexture = new D3D11Texture(backBufferTexture);
            //    FramebufferDescription desc = new FramebufferDescription(_depthTexture, backBufferVdTexture);
            //    _framebuffer = new D3D11Framebuffer(_device, ref desc);
            //    _framebuffer.Swapchain = this;
            //}
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WaitForSingleObjectEx(int hHandle, int dwMilliseconds, bool bAlertable);
    }
}
