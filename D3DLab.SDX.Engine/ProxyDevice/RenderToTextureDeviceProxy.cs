using D3DLab.ECS;
using D3DLab.ECS.Common;

using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D3DLab.SDX.Engine.ProxyDevice {
    class RenderToTextureDeviceProxy : DirectX11Proxy {
        static readonly FeatureLevel[] levels = new FeatureLevel[] { FeatureLevel.Level_11_1, FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 };

        readonly SharpDX.Direct3D11.Device3 device3;
        readonly Texture2D TargetTexture;

        public RenderToTextureDeviceProxy(Adapter adapter, GraphicSurfaceSize size) {
            D3DDevice = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.None, levels);

            TargetTexture = new Texture2D(D3DDevice, new Texture2DDescription() {
                Format = Format.R8G8B8A8_UNorm,
                Width = size.Width,
                Height = size.Height,
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            });

            RenderTarget = new RenderTargetView(D3DDevice, TargetTexture);

            ImmediateContext = D3DDevice.ImmediateContext;
        }

        public override RasterizerState CreateRasterizerState(RasterizerStateDescription2 description2) {
            if (device3 == null) {
                return base.CreateRasterizerState(GraphicsDevice.ToRasterizerDesc0(description2));
            }
            return new RasterizerState2(device3, description2);
        }

        public override Texture2D GetBackBuffer() => TargetTexture;

        public override void Present() {
            
        }

        public override void Resize(int width, int height) {
           
        }

        public override void Dispose() {
            base.Dispose();
            device3?.Dispose();
            TargetTexture.Dispose();
        }
    }
}
