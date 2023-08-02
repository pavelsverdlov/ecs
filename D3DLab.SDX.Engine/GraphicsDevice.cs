using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.SDX.Engine.D2;
using D3DLab.SDX.Engine.Shader;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using D3DLab.SDX.Engine.ProxyDevice;

namespace D3DLab.SDX.Engine {
    public class GraphicsDeviceException : Exception {
        GraphicsDeviceException(string mess) : base(mess) { }
        public static Exception ResourseSlotAlreadyUsed(int slot) { return new GraphicsDeviceException($"Resourse Slot '{slot}' is already used."); }
        public static Exception ShaderAddedTwice() { return new GraphicsDeviceException($"Shader was added twise to DeviceContext."); }

        public static Exception NotDynamicBuffer =>
            new GraphicsDeviceException($"Can't be updated. Buffer must be CpuAccessFlags.Write & ResourceUsage.Dynamic.");
    }

    [Obsolete("Old", true)]
    class RenderToTexture : DirectX11Proxy {
        Texture2D targetTexture;
        public RenderToTexture(Adapter adapter, int width, int height) {
            var d = new SharpDX.Direct3D11.Device(adapter, DeviceCreationFlags.BgraSupport);
            D3DDevice = d.QueryInterface<Device5>();
            ImmediateContext = D3DDevice.ImmediateContext;

            //Resize(width, height);
        }
        public override Texture2D GetBackBuffer() => targetTexture;
        public override void Dispose() {
            base.Dispose();
            targetTexture.Dispose();
        }

        public override void Present() {

        }

        public override void Resize(int width, int height) {
            targetTexture = new Texture2D(D3DDevice, new Texture2DDescription() {
                Format = Format.B8G8R8A8_UNorm,//Format.B8G8R8A8_UNorm,//
                Width = width,//Bgra32
                Height = height,
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.Shared, //ResourceOptionFlags.None
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            });

            RenderTarget = new RenderTargetView(D3DDevice, targetTexture);
        }

        public override RasterizerState CreateRasterizerState(RasterizerStateDescription2 description2)
            => CreateRasterizerState(GraphicsDevice.ToRasterizerDesc0(description2));
    }

    [Obsolete]
    public interface IGraphicsDevice {
        //Texture2D GetBackBuffer();
        //MemoryStream CopyBackBufferMemoryStream();
        //SharpDX.Direct3D11.Device D3DDevice { get; }
       // RasterizerState CreateRasterizerState(RasterizerStateDescription2 description);
    }

    public class GraphicsDevice : IGraphicsDevice {

        internal const Format BackBufferTextureFormat = Format.R8G8B8A8_UNorm;

        /// <summary>
        /// class for controlling resourse slots for each shaders 
        /// to avoid setting to already occupied slot
        /// it is easy to miss 'cause directX doesn't rise any exceptions in this case
        /// </summary>
        class ResourseRegistrHash {
            readonly Dictionary<int, HashSet<int>> shaders;

            public ResourseRegistrHash() {
                shaders = new Dictionary<int, HashSet<int>>();
            }

            public void RegisterShader(int ptr) {
                if (shaders.ContainsKey(ptr)) {
                    throw GraphicsDeviceException.ShaderAddedTwice();
                }
                shaders.Add(ptr, new HashSet<int>());
            }
            public void RegisterResourseSlot(int ptr, int slot) {
                if (shaders[ptr].Contains(slot)) {
                    throw GraphicsDeviceException.ResourseSlotAlreadyUsed(slot);
                }
                shaders[ptr].Add(slot);
            }
            public void Clear() {
                shaders.Clear();
            }
        }

        public readonly D3DShaderCompilator Compilator;

        public SharpDX.Direct3D11.Device D3DDevice => directX.D3DDevice;
        public TextureLoader TexturedLoader { get; }
        public DeviceContext ImmediateContext => directX.ImmediateContext;
        public string VideoCardDescription { get; }
        public GraphicSurfaceSize Size { get; private set; }
        public AdapterDescription Adapter { get; }
        public DepthStencilView DepthStencilView { get; private set; }

        readonly ResourseRegistrHash resourseHash;
        readonly DirectX11Proxy directX;

        internal GraphicsDevice(DirectX11Proxy proxy, GraphicSurfaceSize size, AdapterDescription adapterDescription) {
            resourseHash = new ResourseRegistrHash();
            Compilator = new D3DShaderCompilator();
            Adapter = adapterDescription;

            int width =size.Width;
            int height = size.Height;

            directX = proxy;
            directX.Resize(width, height);
            CreateBuffers(width, height);

            TexturedLoader = new TextureLoader(directX.D3DDevice);
        }

        public void Dispose() {
            directX.Dispose();
            DepthStencilView.Dispose();
            resourseHash.Clear();
        }

        public void Resize(float w, float h) {
            var width = (int)Math.Ceiling(w);
            var height = (int)Math.Ceiling(h);

            directX.RenderTarget.Dispose();
            DepthStencilView.Dispose();

            directX.Resize(width, height);
            CreateBuffers(width, height);
        }

        void CreateBuffers(int width, int height) {
            Size = new SurfaceSize(width, height);
            var zBufferTextureDescription = new Texture2DDescription {
                Format = Format.D32_Float_S8X24_UInt,//D24_UNorm_S8_UInt
                ArraySize = 1,
                MipLevels = 1,
                Width = width,
                Height = height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            };

            using (var zBufferTexture = new Texture2D(directX.D3DDevice, zBufferTextureDescription)) {
                DepthStencilView = new DepthStencilView(directX.D3DDevice, zBufferTexture);
            }

            var depthEnabledStencilState = new DepthStencilState(directX.D3DDevice, D3DDepthStencilStateDescriptions.DepthEnabled);

            var viewport = new SharpDX.Viewport(0, 0, width, height, 0f, 1.0f);
            ImmediateContext.Rasterizer.SetViewport(viewport);
            ImmediateContext.OutputMerger.SetTargets(DepthStencilView, directX.RenderTarget);

            //no zbuffer and DepthStencil
            //ImmediateContext.OutputMerger.SetRenderTargets(renderTargetView);

            //with zbuffer / DepthStencil
            ImmediateContext.OutputMerger.SetDepthStencilState(depthEnabledStencilState, 0);

            //var blendStateDesc = new BlendStateDescription();
            //blendStateDesc.RenderTarget[0].IsBlendEnabled = true;
            //blendStateDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            //blendStateDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            //blendStateDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
            //blendStateDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            //blendStateDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
            //blendStateDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
            //blendStateDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;
            //var blend = new BlendState(Device, blendStateDesc);

            //var blendFactor = new Color4(0, 0, 0, 0);
            //Device.ImmediateContext.OutputMerger.SetBlendState(blend, blendFactor, -1);
        }

        //[Obsolete("Store RasterizerState in render component")]
        //public void UpdateRasterizerState(RasterizerStateDescription2 descr) {
        //    ImmediateContext.Rasterizer.State = new RasterizerState2(directX.D3DDevice, descr);
        //}

        public void Refresh() {
            ImmediateContext.ClearDepthStencilView(DepthStencilView, 
                DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1f, 0);
            ImmediateContext.ClearRenderTargetView(directX.RenderTarget, new RawColor4(0, 0, 0, 0));
        }

        public static bool IsDirectX11Supported() {
            return global::SharpDX.Direct3D11.Device.GetSupportedFeatureLevel() == FeatureLevel.Level_11_0;
        }

        public void Present() {
            directX.Present();
            resourseHash.Clear();
        }

        public Texture2D GetBackBuffer() => directX.GetBackBuffer();

        public System.Drawing.Bitmap CopyBackBufferTexture() {
            using (var stream = new MemoryStream()) {
                //using (var tex = directX.GetBackBuffer()) { //for swapchain
                var tex = directX.GetBackBuffer();
                Copy(tex, stream, directX.D3DDevice);
                stream.Position = 0;
                var bmp = new System.Drawing.Bitmap(stream);
                return bmp;
            }
        }
        public MemoryStream CopyBackBufferMemoryStream() {
            var stream = new MemoryStream();
            //using (var tex = directX.GetBackBuffer()) { //for swapchain
            var tex = directX.GetBackBuffer();
            Copy(tex, stream, directX.D3DDevice);
            stream.Position = 0;
            return stream;
        }

        //TODO: check this way to getting back texture
        //public BitmapSource ToBitmap() {
        //    if (_d3D11Image == null)
        //        return null;

        //    // Copy back buffer to WriteableBitmap.
        //    int width = _d3D11Image.PixelWidth;
        //    int height = _d3D11Image.PixelHeight;
        //    var format = EnableAlpha ? PixelFormats.Bgra32 : PixelFormats.Bgr32;
        //    var writeableBitmap = new WriteableBitmap(width, height, 96, 96, format, null);
        //    writeableBitmap.Lock();
        //    try {
        //        uint[] data = new uint[width * height];
        //        _d3D11Image.TryGetData(data);

        //        // Get a pointer to the back buffer.
        //        unsafe {
        //            uint* pBackbuffer = (uint*)writeableBitmap.BackBuffer;
        //            for (int i = 0; i < data.Length; i++)
        //                pBackbuffer[i] = data[i];
        //        }

        //        writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
        //    } finally {
        //        writeableBitmap.Unlock();
        //    }

        //    return writeableBitmap;
        //}


        static void Copy(Texture2D texture, Stream stream, SharpDX.Direct3D11.Device device) {
            var desc = new Texture2DDescription {
                Width = (int)texture.Description.Width,
                Height = (int)texture.Description.Height,
                MipLevels = 1,
                ArraySize = 1,
                Format = texture.Description.Format,
                Usage = ResourceUsage.Staging,
                SampleDescription = new SampleDescription(1, 0),
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None
            };



            using (var factory = new SharpDX.WIC.ImagingFactory()) {
                using (var textureCopy = new Texture2D(device, desc)) {
                    device.ImmediateContext.CopyResource(texture, textureCopy);

                    var dataBox = device.ImmediateContext.MapSubresource(
                        textureCopy,
                        0,
                        0,
                        MapMode.Read,
                        global::SharpDX.Direct3D11.MapFlags.None,
                        out SharpDX.DataStream dataStream);
                    using (dataStream) {
                        var t = dataStream.ReadByte(); //ReadFloat();

                        var dataRectangle = new SharpDX.DataRectangle {
                            DataPointer = dataStream.DataPointer,
                            Pitch = dataBox.RowPitch
                        };
                        //https://github.com/sharpdx/Toolkit/blob/master/Source/Toolkit/SharpDX.Toolkit.Graphics/WICHelper.cs
                        using (var bitmap = new SharpDX.WIC.Bitmap(factory, textureCopy.Description.Width, textureCopy.Description.Height,
                            SharpDX.WIC.PixelFormat.Format32bppRGBA, dataRectangle, 0)) {

                            stream.Position = 0;
                            using (var bitmapEncoder = new SharpDX.WIC.PngBitmapEncoder(factory, stream)) {
                                using (var bitmapFrameEncode = new SharpDX.WIC.BitmapFrameEncode(bitmapEncoder)) {
                                    bitmapFrameEncode.Initialize();
                                    bitmapFrameEncode.SetSize(bitmap.Size.Width, bitmap.Size.Height);
                                    var pixelFormat = SharpDX.WIC.PixelFormat.FormatDontCare;
                                    bitmapFrameEncode.SetPixelFormat(ref pixelFormat);
                                    bitmapFrameEncode.WriteSource(bitmap);
                                    bitmapFrameEncode.Commit();
                                    bitmapEncoder.Commit();
                                }
                            }

                        }
                        device.ImmediateContext.UnmapSubresource(textureCopy, 0);
                    }
                }
            }
        }

        public SharpDX.Direct3D11.Texture2D CreateTexture2D(GraphicSurfaceSize size) {
            return new Texture2D(D3DDevice, new Texture2DDescription() {
                Format =  Format.R32G32B32A32_Float,
                Width = size.Width,
                Height = size.Height,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.GenerateMipMaps,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            });
        }

        public SharpDX.Direct3D11.Buffer CreateUnorderedRWStructuredBuffer<T>(ref T[] range) where T : struct {
            BufferStaticVerifications.CheckSizeInBytes<T>();

            var desc = new BufferDescription() {
                SizeInBytes = Unsafe.SizeOf<T>(),
                CpuAccessFlags = GetCpuAccessFlagsFromUsage(ResourceUsage.Default),
                Usage = ResourceUsage.Default,
            };

            desc.OptionFlags |= ResourceOptionFlags.BufferStructured;
            desc.BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource;

            // We keep the element size in the structure byte stride, even if it is not a structured buffer
            desc.StructureByteStride = desc.SizeInBytes / range.Length;


            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, range, desc);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(BindFlags flags, ref T range)
            where T : struct {
            BufferStaticVerifications.CheckSizeInBytes<T>();
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, flags, ref range);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(BindFlags flags, T[] range)
           where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, flags, range);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(T[] range, BufferDescription desc)
          where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, range, desc);
        }
        public SharpDX.Direct3D11.Buffer CreateBuffer<T>(ref T data, BufferDescription desc)
         where T : struct {
            BufferStaticVerifications.CheckSizeInBytes(desc.SizeInBytes);
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, ref data, desc);
        }

        public SharpDX.Direct3D11.Buffer CreateDynamicBuffer<T>(ref T range, int sizeInBytes)
        where T : struct {
            BufferStaticVerifications.CheckSizeInBytes(sizeInBytes);

            var des = new BufferDescription() {
                Usage = ResourceUsage.Dynamic,
                SizeInBytes = sizeInBytes,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                //    StructureByteStride = 0, 
            };
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, ref range, des);
        }

        /// <summary>
        /// For referrence types and any arrays
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="range"></param>
        /// <param name="structureByteStride"></param>
        /// <param name="sizeInBytes"></param>
        /// <returns></returns>
        public SharpDX.Direct3D11.Buffer CreateDynamicBuffer<T>(T[] range, int sizeInBytes)
     where T : struct {
            return SharpDX.Direct3D11.Buffer.Create(D3DDevice, range, new BufferDescription {
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Dynamic,
                //  StructureByteStride = structureByteStride,
                SizeInBytes = sizeInBytes
            });
        }
        //SharpDX.DataBox src = context.MapSubresource(lightDataBuffer, 1, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out SharpDX.DataStream stream);
        //stream.Write(light.GetStructLayoutResource());
        //context.UnmapSubresource(lightDataBuffer, LightStructLayout.RegisterResourceSlot);

        public void UpdateDynamicBuffer<T>(T[] newdata, SharpDX.Direct3D11.Buffer buffer) where T : struct {
            SharpDX.DataStream stream = null;
            try {
                SharpDX.DataBox src = ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard,
                    SharpDX.Direct3D11.MapFlags.None, out stream);

                // src.DataPointer
                stream.WriteRange(newdata);
            } finally {
                // Every buffer is defined as a single subresource (unlike textures). So subresource is always equals 0.
                ImmediateContext.UnmapSubresource(buffer, 0);
            }
        }

        public void UpdateDynamicBuffer<T>(ref T newdata, SharpDX.Direct3D11.Buffer buffer) where T : struct {
            var desc = buffer.Description;
            if (desc.CpuAccessFlags != CpuAccessFlags.Write && desc.Usage != ResourceUsage.Dynamic) {
                throw GraphicsDeviceException.NotDynamicBuffer;
            }
            try {
                //  Disable GPU access to the vertex buffer data.
                ImmediateContext.MapSubresource(buffer, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None,
                    out var mappedResource);
                mappedResource.Write(newdata);
            } finally {
                //  Reenable GPU access to the vertex buffer data.
                // Every buffer is defined as a single subresource (unlike textures). So subresource is always equals 0.
                ImmediateContext.UnmapSubresource(buffer, 0);
            }


            //try {
            //    var box = ImmediateContext.MapSubresource(buffer, slot, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
            //    SharpDX.Utilities.WriteAndPosition(box.DataPointer, ref newdata);
            //} catch (Exception ex) {
            //    ex.ToString();
            //} finally {
            //    ImmediateContext.UnmapSubresource(buffer, slot);
            //}
        }

        public void UpdateArraySubresource<T>(T[] data, SharpDX.Direct3D11.Buffer buff) where T : struct {
            ImmediateContext.UpdateSubresource(data, buff);
        }
        public void UpdateSubresource<T>(ref T data, SharpDX.Direct3D11.Buffer buff) where T : struct {
            ImmediateContext.UpdateSubresource(ref data, buff);
        }

        public void RegisterConstantBuffer(CommonShaderStage stage, int slot, SharpDX.Direct3D11.Buffer buff) {
            stage.SetConstantBuffer(slot, buff);
            resourseHash.RegisterResourseSlot(stage.GetHashCode(), slot);
        }
        public void RegisterConstantBuffer(CommonShaderStage stage, int slot, DisposableSetter<SharpDX.Direct3D11.Buffer> buff) {
            RegisterConstantBuffer(stage, slot, buff.Get());
        }

        public InputLayout CreateInputLayout(byte[] vertexShaderByteCode, InputElement[] elements) {
            var inputSignature = SharpDX.D3DCompiler.ShaderSignature.GetInputSignature(vertexShaderByteCode);
            return new InputLayout(directX.D3DDevice, inputSignature, elements);
        }
        public void SetVertexShader(DisposableSetter<VertexShader> shader) {
            ImmediateContext.VertexShader.Set(shader.Get());
            resourseHash.RegisterShader(ImmediateContext.VertexShader.GetHashCode());
        }
        public void SetPixelShader(DisposableSetter<PixelShader> shader) {
            ImmediateContext.PixelShader.Set(shader.Get());
            resourseHash.RegisterShader(ImmediateContext.PixelShader.GetHashCode());
        }
        public void SetGeometryShader(DisposableSetter<GeometryShader> shader) {
            ImmediateContext.GeometryShader.Set(shader.Get());
            resourseHash.RegisterShader(ImmediateContext.GeometryShader.GetHashCode());
        }
        public void ClearAllShader() {
            resourseHash.Clear();
            ImmediateContext.VertexShader.Set(null);
            ImmediateContext.GeometryShader.Set(null);
            ImmediateContext.ComputeShader.Set(null);
            ImmediateContext.HullShader.Set(null);
            ImmediateContext.DomainShader.Set(null);
            ImmediateContext.PixelShader.Set(null);
        }
        public void DisableIndexVertexBuffers() {
            ImmediateContext.InputAssembler.InputLayout = null;
            ImmediateContext.InputAssembler.SetVertexBuffers(0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            ImmediateContext.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);
        }

        public SamplerState CreateSampler(SamplerStateDescription desc) {
            return new SamplerState(D3DDevice, desc);
        }

        public RasterizerState CreateRasterizerState(RasterizerStateDescription2 description2) {
            var bs = directX.ImmediateContext.OutputMerger.BlendState;
            if (bs != null && bs.Description.AlphaToCoverageEnable && !description2.IsMultisampleEnabled) {
               // throw new Exception("BlendState.AlphaToCoverageEnable must be with RasterizerState.IsMultisampleEnabled");
                //description2.IsMultisampleEnabled = true;
            }

            return directX.CreateRasterizerState(description2);
        }

        #region RenderTargets

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderTargetView"></param>
        /// <param name="startSlot"></param>
        /// <param name="unorderedAccessViews"></param>
        /// <unmanaged>void ID3D11DeviceContext::OMSetRenderTargetsAndUnorderedAccessViews([In] unsigned int NumRTVs,[In, Buffer, Optional] const ID3D11RenderTargetView** ppRenderTargetViews,[In, Optional] ID3D11DepthStencilView* pDepthStencilView,[In] unsigned int UAVStartSlot,[In] unsigned int NumUAVs,[In, Buffer, Optional] const ID3D11UnorderedAccessView** ppUnorderedAccessViews,[In, Buffer, Optional] const unsigned int* pUAVInitialCounts)</unmanaged>	
        /// <unmanaged-short>ID3D11DeviceContext::OMSetRenderTargetsAndUnorderedAccessViews</unmanaged-short>	
        //public void SetRenderTargetsAndUnorderedAccessViews(RenderTargetView renderTargetView, DepthStencilView depthStencil, int startSlot, int count, UnorderedAccessView[] unorderedAccessViews) {
        //   // ImmediateContext.OutputMerger.SetTargets(renderTargetView, startSlot, unorderedAccessViews);
        //    ImmediateContext.OutputMerger.SetTargets(depthStencil, renderTargetView, startSlot, unorderedAccessViews, count);
        //}

        #endregion


        static CpuAccessFlags GetCpuAccessFlagsFromUsage(ResourceUsage usage) {
            switch (usage) {
                case ResourceUsage.Dynamic:
                    return CpuAccessFlags.Write;
                case ResourceUsage.Staging:
                    return CpuAccessFlags.Read | CpuAccessFlags.Write;
            }
            return CpuAccessFlags.None;
        }
        internal static RasterizerStateDescription ToRasterizerDesc0(RasterizerStateDescription2 description2) {
            return new RasterizerStateDescription {
                CullMode = description2.CullMode,
                DepthBias = description2.DepthBias,
                DepthBiasClamp = description2.DepthBiasClamp,
                FillMode = description2.FillMode,
                IsAntialiasedLineEnabled = description2.IsAntialiasedLineEnabled,
                IsDepthClipEnabled = description2.IsDepthClipEnabled,
                IsFrontCounterClockwise = description2.IsFrontCounterClockwise,
                IsMultisampleEnabled = description2.IsMultisampleEnabled,
                IsScissorEnabled = description2.IsScissorEnabled,
                SlopeScaledDepthBias = description2.SlopeScaledDepthBias,
            };
        }
    }
    /*
    
    D3D11_MAP_WRITE_NO_OVERWRITE if you plan on writing to the buffer more than once per frame
    MapMode.WriteNoOverwrite


     */
}
