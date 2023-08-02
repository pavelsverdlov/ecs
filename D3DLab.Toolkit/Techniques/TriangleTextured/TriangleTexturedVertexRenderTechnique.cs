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
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.TriangleTextured {

    public class TriangleTexturedVertexRenderTechnique<TProperties> :
        NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties>, IGraphicSystemContextDependent
        where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.Techniques.TriangleTextured.textured_vertex.hlsl";
        const string gs_flat_shading = @"D3DLab.Toolkit.Techniques.TriangleTextured.gs_flat_shading.hlsl";

        readonly D3DShaderTechniquePass pass;
        readonly D3DShaderTechniquePass flatShadingPass;
        readonly VertexLayoutConstructor layconst;
        //readonly ShaderDebugMode debug;

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector2 TexCoor;
            public Vertex(Vector3 position, Vector3 normal, Vector2 texCoor) {
                Position = position;
                Normal = normal;
                TexCoor = texCoor;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }


        public IContextState ContextState { get; set; }
        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };

  //      ShaderDebugMode debug;
        public TriangleTexturedVertexRenderTechnique()  {

            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddTexCoorElementAsVector2();

            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(TriangleTexturedVertexRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "TV_"));
            flatShadingPass = new D3DShaderTechniquePass(d.Load(gs_flat_shading, "TV_"));
            //debug = new ShaderDebugMode(new DirectoryInfo(@"D:\"), pass);
            //debug.Activate();

            depthStencilStateDesc = D3DDepthStencilDefinition.DepthEnabled;
            blendStateDesc = D3DBlendStateDescriptions.BlendStateEnabled;

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);
            flatShadingGS = new DisposableSetter<GeometryShader>(disposer);
        }

        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<InputLayout> inputLayout;
        readonly DisposableSetter<GeometryShader> flatShadingGS;

        readonly BlendStateDescription blendStateDesc;
        readonly D3DDepthStencilDefinition depthStencilStateDesc;
        

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            BeforeRender(graphics);
            foreach (var en in entities) {
                RenderEach(graphics, game, en);
            }
        }

        public override void CleanupRenderCache() {
            pass.ClearCache();
            flatShadingPass.ClearCache();
            base.CleanupRenderCache();
        }


        public void BeforeRender(GraphicsDevice graphics) {
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

            if (!flatShadingPass.IsCompiled) {
                flatShadingPass.Compile(graphics.Compilator);
                flatShadingGS.Set(new GeometryShader(device, flatShadingPass.GeometryShader.ReadCompiledBytes()));
            }
        }
        public void RenderEach(GraphicsDevice graphics, TProperties game, GraphicEntity en) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if(!en.TryGetComponent<D3DRenderComponent>(out var render)) {
                render = new D3DRenderComponent();
                en.AddComponent(render);
            }
            
            var renderable = en.GetComponent<RenderableComponent>();
            var geo = ContextState.GetGeometryPool().GetGeometry<IGeometryData>(en);
            //optional
            var hasColor = en.TryGetComponent<MaterialColorComponent>(out var color);
            var hasTexture = en.TryGetComponent<D3DTexturedMaterialSamplerComponent>(out var texture);

            if (!render.DepthStencilState.HasValue) {
                render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, 
                    renderable.DepthStencilStateDefinition.Description));
            }

            if (!render.BlendingState.HasValue) {
                render.BlendingState.Set(new BlendState(graphics.D3DDevice, renderable.BlendStateDescription));
            }

            UpdateTransformWorld(graphics, render, en);

            if (geo.IsModified) {
                var vertex = new Vertex[geo.Positions.Length];
                for (var index = 0; index < vertex.Length; index++) {
                    vertex[index] = new Vertex(
                        geo.Positions[index], geo.Normals[index], geo.TexCoor[index]);
                }

                render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, vertex));
                render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray()));

                geo.IsModified = false;
            }

            if (hasColor) {
                var material = MaterialStructBuffer.From(color);

                if (render.MaterialBuffer.HasValue) {
                    var buff = render.MaterialBuffer.Get();
                    graphics.UpdateDynamicBuffer(ref material, buff);
                } else {
                    var buff = graphics.CreateDynamicBuffer(ref material, Unsafe.SizeOf<MaterialStructBuffer>());
                    render.MaterialBuffer.Set(buff);
                }
            }

            if (hasTexture && texture.IsModified) {
                render.TextureResources.Set(ConvertToResources(texture, graphics.TexturedLoader));
                render.SampleState.Set(graphics.CreateSampler(texture.SampleDescription));
                texture.IsModified = false;
            }

            {
                graphics.ClearAllShader();
                context.VertexShader.Set(vertexShader.Get());
                if (en.Contains<FlatShadingGeometryComponent>()) {
                    context.GeometryShader.Set(flatShadingGS.Get());
                }
                context.PixelShader.Set(pixelShader.Get());

                context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, game.Game);
                context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot,
                    render.TransformWorldBuffer.Get());

                context.PixelShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, game.Game);
                context.PixelShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, game.Lights);
                if (render.MaterialBuffer.HasValue) {
                    context.PixelShader.SetConstantBuffer(MaterialStructBuffer.RegisterResourceSlot,
                        render.MaterialBuffer.Get());
                }
                if (render.TextureResources.HasValue && render.SampleState.HasValue) {
                    context.PixelShader.SetShaderResources(0, render.TextureResources.Get());
                    context.PixelShader.SetSampler(0, render.SampleState.Get());
                }
            }

            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(),
                layconst.VertexSize, 0));
            context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

            context.InputAssembler.InputLayout = inputLayout.Get();
            context.InputAssembler.PrimitiveTopology = renderable.PrimitiveTopology;

            context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
            context.OutputMerger.SetBlendState(render.BlendingState.Get(),
                new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);

            using (var rasterizerState = graphics.CreateRasterizerState(renderable.RasterizerStateDescription)) {
                context.Rasterizer.State = rasterizerState;

                graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
            }
        }

        public override bool IsAplicable(GraphicEntity entity) {
            return entity.TryGetComponent<RenderableComponent>(out var ren)
                && ren.IsRenderable
                && ren.Technique == RenderTechniques.TriangleTextured
                && entity.Contains(
                    typeof(GeometryPoolComponent),
                    typeof(TransformComponent));
        }
    }
}
