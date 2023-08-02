using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit._CommonShaders;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Techniques.TriangleColored;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.OrderIndependentTransparency {
    public class OITTriangleColoredVertexRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
        //const string path = @"D3DLab.Toolkit.D3D.OrderIndependentTransparency.oit_colored_vertex.hlsl";
        const string oit = @"OrderIndependentTransparency.helix_oit.hlsl";
        const string quard = @"OrderIndependentTransparency.helix_oit_quard.hlsl";

        readonly D3DShaderTechniquePass pass;
        readonly D3DShaderTechniquePass quardPass;
        readonly VertexLayoutConstructor layconst;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public Vertex(Vector3 position, Vector3 normal) {
                Position = position;
                Normal = normal;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }


        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass, quardPass };


        public OITTriangleColoredVertexRenderTechnique() {

            layconst = new VertexLayoutConstructor(Vertex.Size)
              .AddPositionElementAsVector3()
              .AddNormalElementAsVector3()
              .AddColorElementAsVector4();

            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(OITTriangleColoredVertexRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(oit, "OIT_"));
            quardPass = new D3DShaderTechniquePass(d.Load(quard, "QOIT_"));

            depthStencilStateDesc = D3DDepthStencilDefinition.DepthEnabled.Description;
            blendStateDesc = D3DBlendStateDescriptions.BlendStateEnabled;

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);

            vertexShaderQuard = new DisposableSetter<VertexShader>(disposer);
            pixelShaderQuard = new DisposableSetter<PixelShader>(disposer);

            colorTargetTex2D = new DisposableSetter<Texture2D>(disposer);
            alphaTargetTex2D = new DisposableSetter<Texture2D>(disposer);
            colorTargetNoMSAATexture2D = new DisposableSetter<Texture2D>(disposer);
            alphaTargetNoMSAATexture2D = new DisposableSetter<Texture2D>(disposer);

            colorTargetNoMSAA = new DisposableSetter<ShaderResourceView>(disposer);
            alphaTargetNoMSAA = new DisposableSetter<ShaderResourceView>(disposer);

            colorTargetView = new DisposableSetter<RenderTargetView>(disposer);
            alphaTargetView = new DisposableSetter<RenderTargetView>(disposer);
            quardTargetView = new DisposableSetter<RenderTargetView>(disposer);

            targetSampler = new DisposableSetter<SamplerState>(disposer);
        }

        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<InputLayout> inputLayout;

        readonly DisposableSetter<VertexShader> vertexShaderQuard;
        readonly DisposableSetter<PixelShader> pixelShaderQuard;

        readonly BlendStateDescription blendStateDesc;
        readonly DepthStencilStateDescription depthStencilStateDesc;

        DisposableSetter<Texture2D> colorTargetTex2D;
        DisposableSetter<Texture2D> alphaTargetTex2D;
        DisposableSetter<Texture2D> colorTargetNoMSAATexture2D;
        DisposableSetter<Texture2D> alphaTargetNoMSAATexture2D;

        DisposableSetter<ShaderResourceView> colorTargetNoMSAA;
        DisposableSetter<ShaderResourceView> alphaTargetNoMSAA;

        DisposableSetter<RenderTargetView> colorTargetView;
        DisposableSetter<RenderTargetView> alphaTargetView;

        DisposableSetter<RenderTargetView> quardTargetView;

        DisposableSetter<SamplerState> targetSampler;

        Texture2DDescription colorDesc = new Texture2DDescription() {
            Format = Format.R16G16B16A16_Float,
            OptionFlags = ResourceOptionFlags.None,
            MipLevels = 1,
            ArraySize = 1,
            Usage = ResourceUsage.Default,
            CpuAccessFlags = CpuAccessFlags.None,
        };
        Texture2DDescription alphaDesc = new Texture2DDescription() {
            Format = Format.A8_UNorm,
            OptionFlags = ResourceOptionFlags.None,
            MipLevels = 1,
            ArraySize = 1,
            Usage = ResourceUsage.Default,
            CpuAccessFlags = CpuAccessFlags.None,
        };
        SamplerStateDescription LinearSamplerWrapAni1 = new SamplerStateDescription() {
            AddressU = TextureAddressMode.Wrap,
            AddressV = TextureAddressMode.Wrap,
            AddressW = TextureAddressMode.Wrap,
            Filter = Filter.MinMagLinearMipPoint,
            MaximumLod = float.MaxValue
        };
        readonly static BlendStateDescription BSOITBlend = new BlendStateDescription() { IndependentBlendEnable = true };
        readonly static DepthStencilStateDescription DSSLessNoWrite = new DepthStencilStateDescription() {
            IsDepthEnabled = true,
            DepthWriteMask = DepthWriteMask.Zero,
            DepthComparison = Comparison.Less,
            IsStencilEnabled = false
        };

        #region OIT

        [StructLayout(LayoutKind.Sequential)]
        struct ListNode {
            public uint packedColor;
            public uint depthAndCoverage;
            public uint next;
            public uint temp;
        };

        Texture2D UnorderedViewTexture;
        UnorderedAccessView headBuffer;
        SharpDX.Direct3D11.Buffer buf;
        UnorderedAccessView fragmentsList;

        Texture2D RWTexture2D_V3;
        UnorderedAccessView UnorderedRWTexture2D_V4;
        Texture2D RWTexture2D_V1;
        UnorderedAccessView UnorderedRWTexture2D_V1;

        UnorderedAccessView[] accessViews;


        #endregion

        protected override void Rendering(GraphicsDevice graphics, TProperties props) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();
                var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                inputLayout.Set(new InputLayout(device, inputSignature, layconst.ConstuctElements()));
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));
                pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
            }

            if (!quardPass.IsCompiled) {
                quardPass.Compile(graphics.Compilator);
                //var inputSignature = ShaderSignature.GetInputSignature(quardPass.VertexShader.ReadCompiledBytes());
                //inputLayoutQuard = new InputLayout(device, inputSignature, null);

                vertexShaderQuard.Set(new VertexShader(device, quardPass.VertexShader.ReadCompiledBytes()));
                pixelShaderQuard.Set(new PixelShader(device, quardPass.PixelShader.ReadCompiledBytes()));
            }

            //clear shaders off prev. technique 
            graphics.ClearAllShader();

            if (!colorTargetTex2D.HasValue) {
                colorDesc.BindFlags = alphaDesc.BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource;
                colorDesc.SampleDescription =
                    alphaDesc.SampleDescription =
                    new SampleDescription(1, 0);

                colorDesc.Width = alphaDesc.Width = (int)graphics.Size.Width;
                colorDesc.Height = alphaDesc.Height = (int)graphics.Size.Height;

                colorTargetTex2D.Set( new Texture2D(device, colorDesc));
                alphaTargetTex2D.Set( new Texture2D(device, alphaDesc));

                {
                    colorTargetNoMSAATexture2D.Set(new Texture2D(device, colorDesc));
                    alphaTargetNoMSAATexture2D.Set(new Texture2D(device, alphaDesc));

                    colorTargetNoMSAA.Set(new ShaderResourceView(device, colorTargetNoMSAATexture2D.Get()));
                    alphaTargetNoMSAA.Set(new ShaderResourceView(device, alphaTargetNoMSAATexture2D.Get()));
                }

                targetSampler.Set(new SamplerState(device, LinearSamplerWrapAni1));

                colorTargetView.Set( new RenderTargetView(device, colorTargetTex2D.Get()));
                alphaTargetView.Set(new RenderTargetView(device, alphaTargetTex2D.Get()));

                var d = colorDesc;
                d.BindFlags = BindFlags.RenderTarget;
                quardTargetView.Set(new RenderTargetView(device, new Texture2D(device, d)));
            }

            //RenderingAlfa(entities, graphics, props);

            RenderingQuard(graphics, props);
        }

        public override void CleanupRenderCache() {
            base.CleanupRenderCache();

            pass.ClearCache();
            quardPass.ClearCache();
        }


        void RenderingAlfa(IEnumerable<GraphicEntity> entities, GraphicsDevice graphics, TProperties props) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            context.ClearRenderTargetView(colorTargetView.Get(), SharpDX.Color.Zero);
            context.ClearRenderTargetView(alphaTargetView.Get(), SharpDX.Color.White);
            //context.SetRenderTargets(context.RenderHost.DepthStencilBufferView,
            //    new RenderTargetView[] { colorTargetView, alphaTargetView });
            context.OutputMerger.SetTargets(colorTargetView.Get(), alphaTargetView.Get());

            foreach (var en in entities) {
                var render = en.GetComponent<D3DRenderComponent>();
                var geo = en.GetComponent<GeometryComponent>();
                var color = en.GetComponent<MaterialColorComponent>();
                var transform = en.GetComponent<TransformComponent>();

                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, depthStencilStateDesc));
                }

                if (!render.BlendingState.HasValue) {
                    render.BlendingState.Set(new BlendState(graphics.D3DDevice, blendStateDesc));
                }

                {
                    context.VertexShader.Set(vertexShader.Get());
                    context.PixelShader.Set(pixelShader.Get());
                }

                if (geo.IsModified || (!render.VertexBuffer.HasValue && !render.IndexBuffer.HasValue)) {
                    var vertex = new Vertex[geo.Positions.Length];
                    for (var index = 0; index < vertex.Length; index++) {
                        vertex[index] = new Vertex(geo.Positions[index], geo.Normals[index]);
                    }

                    render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                    render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

                    geo.IsModified = false;
                }
                if (!render.VertexBuffer.HasValue && !render.IndexBuffer.HasValue) {
                    throw RenderTechniqueException.NoVertexAndIndexBuffers;
                }

                if (color.IsValid) {
                    var material = MaterialStructBuffer.From(color);

                    if (render.MaterialBuffer.HasValue) {
                        var buff = render.MaterialBuffer.Get();
                        graphics.UpdateDynamicBuffer(ref material, buff);
                    } else {
                        var buff = graphics.CreateDynamicBuffer(ref material, Unsafe.SizeOf<MaterialStructBuffer>());
                        render.MaterialBuffer.Set(buff);
                    }
                }

                UpdateTransformWorld(graphics, render, en);

                if (!render.TransformWorldBuffer.HasValue) {
                    throw RenderTechniqueException.NoWorldTransformBuffers;
                }

                {//Update constant buffers
                    context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, props.Game);
                    context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, render.TransformWorldBuffer.Get());

                    context.PixelShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, props.Game);
                    context.PixelShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, props.Lights);
                    if (render.MaterialBuffer.HasValue) {
                        context.PixelShader.SetConstantBuffer(MaterialStructBuffer.RegisterResourceSlot, render.MaterialBuffer.Get());
                    }
                }
                {
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(),
                        layconst.VertexSize, 0));
                    context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

                    context.InputAssembler.InputLayout = inputLayout.Get();
                    context.InputAssembler.PrimitiveTopology = render.PrimitiveTopology;

                    context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
                    context.OutputMerger.SetBlendState(render.BlendingState.Get(), new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);
                }

                var stated = render.RasterizerStateDescription.GetDescription();
                using (var rasterizerState = graphics.CreateRasterizerState(stated)) {
                    context.Rasterizer.State = rasterizerState;

                    graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                }
            }
        }
        void RenderingQuard(GraphicsDevice graphics, TProperties props) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            context.ResolveSubresource(colorTargetTex2D.Get(), 0, colorTargetNoMSAA.Get().Resource, 0, colorDesc.Format);
            context.ResolveSubresource(alphaTargetTex2D.Get(), 0, alphaTargetNoMSAA.Get().Resource, 0, alphaDesc.Format);

            context.VertexShader.Set(vertexShaderQuard.Get());
            context.PixelShader.Set(pixelShaderQuard.Get());

            context.PixelShader.SetShaderResources(10, new[] {
                    colorTargetNoMSAA.Get(), alphaTargetNoMSAA.Get()
                });
            context.PixelShader.SetSampler(0, targetSampler.Get());

            var inputSignature = ShaderSignature.GetInputSignature(quardPass.VertexShader.ReadCompiledBytes());
            context.InputAssembler.InputLayout = new InputLayout(device, inputSignature, 
                new[] { new InputElement("SV_VERTEXID", 0, Format.R32_UInt,0)  });
            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;

            var BSAlphaBlend = new BlendStateDescription();
            BSAlphaBlend.RenderTarget[0] = new RenderTargetBlendDescription() {
                AlphaBlendOperation = BlendOperation.Add,
                BlendOperation = BlendOperation.Add,
                SourceBlend = BlendOption.SourceAlpha,
                DestinationBlend = BlendOption.InverseSourceAlpha,

                SourceAlphaBlend = BlendOption.SourceAlpha,
                DestinationAlphaBlend = BlendOption.DestinationAlpha,
                IsBlendEnabled = true,
                RenderTargetWriteMask = ColorWriteMaskFlags.All
            };
            var DSSNoDepthNoStencil = new DepthStencilStateDescription() {
                IsDepthEnabled = false,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthComparison = Comparison.Always,
                FrontFace = new DepthStencilOperationDescription() {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },
                BackFace = new DepthStencilOperationDescription() {
                    Comparison = Comparison.Always,
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Keep,
                    PassOperation = StencilOperation.Keep
                },
                StencilReadMask = 0,
                StencilWriteMask = 0
            };

            context.OutputMerger.SetDepthStencilState(new DepthStencilState(device, DSSNoDepthNoStencil), 0);
            context.OutputMerger.SetBlendState(new BlendState(device, BSAlphaBlend),
                new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);

            //context.OutputMerger.SetTargets(quardTargetView.Get());

            var rester = new RasterizerStateDescription2 {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                DepthBias = 0,
                DepthBiasClamp = 0,
                SlopeScaledDepthBias = 0,
                IsFrontCounterClockwise = false,
                IsMultisampleEnabled = false,
                IsAntialiasedLineEnabled = false,
                IsDepthClipEnabled = false,
                IsScissorEnabled = true
            };
            using (var rasterizerState = graphics.CreateRasterizerState(rester)) {
                context.Rasterizer.State = rasterizerState;
                graphics.ImmediateContext.Draw(4, 0);
            }
        }

        public override bool IsAplicable(GraphicEntity entity) {
            throw new System.NotImplementedException();
        }
    }
}
