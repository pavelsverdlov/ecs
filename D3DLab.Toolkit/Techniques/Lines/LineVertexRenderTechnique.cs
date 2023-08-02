using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit;
using D3DLab.Toolkit._CommonShaders;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Math3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.Toolkit.Techniques.Lines {
    public class LineVertexRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>, 
        IRenderTechnique<TProperties>, IGraphicSystemContextDependent
        where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.Techniques.Lines.simple_lines.hlsl";


        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector4 Position;
            public readonly Vector4 Color;

            public Vertex(Vector3 position, Vector4 color) {
                Position = new Vector4(position, 1);
                Color = color;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        readonly VertexLayoutConstructor layconst;
        readonly D3DShaderTechniquePass pass;
        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<InputLayout> inputLayout;

        public IContextState ContextState { set; private get; }

        public LineVertexRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
              .AddPositionElementAsVector4()
              .AddColorElementAsVector4();

            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(LineVertexRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "L_"));

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);
        }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();

                inputLayout.Set(graphics.CreateInputLayout(vertexShaderByteCode, layconst.ConstuctElements()));

                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));
                pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
            }

            { //clear shaders off prev. technique 
                graphics.ClearAllShader();
                //all shaders shared for all entity with LineVertexRenderComponent
                context.VertexShader.Set(vertexShader.Get());
                context.PixelShader.Set(pixelShader.Get());

                //Update constant buffers ones becase shaders will not changed
                context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, game.Game);
                //shared for all entity
                context.InputAssembler.InputLayout = inputLayout.Get();
            }

            foreach (var en in entities) {
                if (!en.TryGetComponent<D3DRenderComponent>(out var d3drender)) {
                    d3drender = new D3DRenderComponent();
                    en.AddComponent(d3drender);
                }

                en.TryGetComponent<ColorComponent>(out var color);
                var renderable = en.GetComponent<RenderableComponent>();
                var transform = en.GetComponent<TransformComponent>();
                var geoId = en.GetComponent<GeometryPoolComponent>();

                var geo = ContextState.GetGeometryPool().GetGeometry<IGeometryData>(geoId);

                if (!d3drender.DepthStencilState.HasValue) {
                    d3drender.DepthStencilState.Set(
                        new DepthStencilState(graphics.D3DDevice, renderable.DepthStencilStateDefinition.Description));
                }

                if (!d3drender.BlendingState.HasValue) {
                    d3drender.BlendingState.Set(new BlendState(graphics.D3DDevice, renderable.BlendStateDescription));
                }

                UpdateTransformWorld(graphics, d3drender, en);

                if (geo.IsModified) {
                    var pos = geo.Positions;

                    var vertex = new Vertex[pos.Length];
                    for (var i = 0; i < pos.Length; i++) {
                        var c = color.IsValid ? color.Color : geo.Colors[i];
                        vertex[i] = new Vertex(pos[i], c);
                    }

                    d3drender.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                    geo.IsModified = false;
                }

                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot,
                        d3drender.TransformWorldBuffer.Get());

                {
                    context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(d3drender.VertexBuffer.Get(),
                        layconst.VertexSize, 0));

                    context.InputAssembler.PrimitiveTopology = renderable.PrimitiveTopology;

                    context.OutputMerger.SetDepthStencilState(d3drender.DepthStencilState.Get(), 0);
                    context.OutputMerger.SetBlendState(d3drender.BlendingState.Get(),
                        new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);
                }

                using (var rasterizerState = graphics.CreateRasterizerState(renderable.RasterizerStateDescription)) {
                    context.Rasterizer.State = rasterizerState;
                    context.Draw(geo.Positions.Length, 0);
                }

            }

        }

        public override void CleanupRenderCache() {
            base.CleanupRenderCache();
            pass.ClearCache();

        }

        public override bool IsAplicable(GraphicEntity entity) {
            return entity.TryGetComponent<RenderableComponent>(out var ren)
                && ren.IsRenderable
                && ren.Technique == RenderTechniques.Lines
                && entity.Contains(
                    typeof(GeometryPoolComponent),
                    typeof(TransformComponent));
        }
    }
}
