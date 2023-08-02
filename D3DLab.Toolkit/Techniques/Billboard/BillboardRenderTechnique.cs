using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit;
using D3DLab.Toolkit._CommonShaders;
using D3DLab.Toolkit.Components;

using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.Billboard {
    public class BillboardRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>,
        IRenderTechnique<TProperties>, IGraphicSystemContextDependent
        where TProperties : IToolkitFrameProperties {

        const string path_VS_FS = @"D3DLab.Toolkit.Techniques.Billboard.billboard_updatable.hlsl";
        const string path_GS_ScreenFixed = @"D3DLab.Toolkit.Techniques.Billboard.billboard_gs_screenfixed.hlsl";
        const string path_GS_SizeFixed = @"D3DLab.Toolkit.Techniques.Billboard.billboard_gs_sizefixed.hlsl";

        readonly D3DShaderTechniquePass VS_FS;
        readonly D3DShaderTechniquePass GS_ScreenFixed;
        readonly D3DShaderTechniquePass GS_SizeFixed;
        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<GeometryShader> gsScreenFixed;
        readonly DisposableSetter<GeometryShader> gsSizeFixed;
        readonly DisposableSetter<InputLayout> inputLayout;
        readonly VertexLayoutConstructor layconst;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public Vector4 Position;
            public Vector3 ImageSize;


            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        public BillboardRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
              .AddPositionElementAsVector4()
              .AddColorElementAsVector3()
              ;

            var d = new CombinedShadersLoader(new ManifestResourceLoader(this.GetType()));

            VS_FS = new D3DShaderTechniquePass(d.Load(path_VS_FS, "BILL_"));
            GS_ScreenFixed = new D3DShaderTechniquePass(d.Load(path_GS_ScreenFixed, "BILL_"));
            GS_SizeFixed = new D3DShaderTechniquePass(d.Load(path_GS_SizeFixed, "BILL_"));

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            gsScreenFixed = new DisposableSetter<GeometryShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);
            gsSizeFixed = new DisposableSetter<GeometryShader>(disposer);
        }


        public IContextState ContextState { set; private get; }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { VS_FS, GS_SizeFixed, GS_ScreenFixed };

        public override bool IsAplicable(GraphicEntity entity) {
            return entity.Contains(typeof(BillboardTextComponent));
        }
        public override void CleanupRenderCache() {
            VS_FS.ClearCache();
            GS_ScreenFixed.ClearCache();
            GS_SizeFixed.ClearCache();
            base.CleanupRenderCache();
        }

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            var context = graphics.ImmediateContext;
            var device = graphics.D3DDevice;

            if (!VS_FS.IsCompiled) {
                VS_FS.Compile(graphics.Compilator);

                var vertexShaderByteCode = VS_FS.VertexShader.ReadCompiledBytes();
                var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                inputLayout.Set(new InputLayout(device, inputSignature, layconst.ConstuctElements()));

                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));
                pixelShader.Set(new PixelShader(device, VS_FS.PixelShader.ReadCompiledBytes()));
            }
            if (!GS_ScreenFixed.IsCompiled) {
                GS_ScreenFixed.Compile(graphics.Compilator);
                gsScreenFixed.Set(new GeometryShader(device, GS_ScreenFixed.GeometryShader.ReadCompiledBytes()));
            }
            if (!GS_SizeFixed.IsCompiled) {
                GS_SizeFixed.Compile(graphics.Compilator);
                gsSizeFixed.Set(new GeometryShader(device, GS_SizeFixed.GeometryShader.ReadCompiledBytes()));
            }

            foreach (var en in entities) {
                var renderable = en.GetComponent<RenderableComponent>();
                var billboard = en.GetComponent<BillboardTextComponent>();
                var transform = en.GetComponent<TransformComponent>();

                if (!en.TryGetComponent<D3DRenderComponent>(out var render)) {
                    render = new D3DRenderComponent();
                    en.AddComponent(render);
                }

                if (!en.TryGetComponent<InternalBillboardRenderedTextComponent>(out var renderedText)
                    || !renderedText.IsRendered(billboard)) {

                    renderedText = new InternalBillboardRenderedTextComponent();
                    renderedText.Render(billboard);
                    en.UpdateComponent(renderedText);

                    var image = renderedText.RenderedBitmapText;

                    var v = new Vertex() {
                        Position = billboard.Position.ToVector4(),
                        ImageSize = new Vector3(
                            new Vector2(image.Width, image.Height) * renderedText.Scale,
                            0)
                    };

                    render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, new[] { v }));
                    render.TextureResources.Set(new[] { ConvertToResource(image, graphics.TexturedLoader) });
                    if (!render.SampleState.HasValue) {
                        render.SampleState.Set(graphics.CreateSampler(SamplerStateDescriptions.Default));
                    }
                }

                if (!render.BlendingState.HasValue) {
                    render.BlendingState.Set(new BlendState(graphics.D3DDevice, renderable.BlendStateDescription));
                }
                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice,
                        renderable.DepthStencilStateDefinition.Description));
                }

                base.UpdateTransformWorld(graphics, render, en);

                graphics.ClearAllShader();
                {
                    context.VertexShader.Set(vertexShader.Get());
                    context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, game.Game);
                    context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot,
                        render.TransformWorldBuffer.Get());

                    switch (billboard.SizeMode) {
                        case BillboardSizeModes.SceenFixed:
                            context.GeometryShader.Set(gsScreenFixed.Get());
                            break;
                        case BillboardSizeModes.SizeFixed:
                            context.GeometryShader.Set(gsSizeFixed.Get());
                            break;
                    }
                    context.GeometryShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, game.Game);

                    context.PixelShader.Set(pixelShader.Get());
                    context.PixelShader.SetShaderResources(0, render.TextureResources.Get());
                    context.PixelShader.SetSampler(0, render.SampleState.Get());
                }

                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(),
                    layconst.VertexSize, 0));
                context.InputAssembler.SetIndexBuffer(null, Format.R32_UInt, 0);

                context.InputAssembler.InputLayout = inputLayout.Get();
                context.InputAssembler.PrimitiveTopology = renderable.PrimitiveTopology;

                context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
                context.OutputMerger.SetBlendState(render.BlendingState.Get(),
                    new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);

                using (var rasterizerState = graphics.CreateRasterizerState(renderable.RasterizerStateDescription)) {
                    context.Rasterizer.State = rasterizerState;
                    graphics.ImmediateContext.Draw(1, 0);
                }
            }
        }
    }
}
