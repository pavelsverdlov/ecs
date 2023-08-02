using D3DLab.ECS;
using D3DLab.ECS.Common;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace D3DLab.SDX.Engine.ProxyDevice {
    class RenderToTargetViewDeviceProxy : DirectX11Proxy {
        static readonly FeatureLevel[] levels = new FeatureLevel[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 };

        readonly SharpDX.Direct3D11.Device3 device3;
        Texture2D TargetTexture;

        public RenderToTargetViewDeviceProxy(Adapter adapter, GraphicSurfaceSize size) {
            D3DDevice = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.BgraSupport, levels);
            ImmediateContext = D3DDevice.ImmediateContext;

            Resize(size.Width,size.Height);
        }

        public override RasterizerState CreateRasterizerState(RasterizerStateDescription2 description2) {
            if (device3 == null) {
                return base.CreateRasterizerState(GraphicsDevice.ToRasterizerDesc0(description2));
            }
            return new RasterizerState2(device3, description2);
        }

        public override Texture2D GetBackBuffer() => TargetTexture;

        public override void Present() {
            ImmediateContext.Flush();
        }

        public override void Resize(int width, int height) {
            TargetTexture?.Dispose();

            TargetTexture = new Texture2D(D3DDevice, new Texture2DDescription() {
                Format = Format.B8G8R8A8_UNorm,
                //Format = Format.R8G8B8A8_UNorm,
                Width = width,
                Height = height,
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.Shared,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            });

            RenderTarget = new RenderTargetView(D3DDevice, TargetTexture);
        }

        public override void Dispose() {
            base.Dispose();
            TargetTexture?.Dispose();
            TargetTexture = null;
            device3?.Dispose();
        }
    }
}
