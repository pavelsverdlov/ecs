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
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.TriangleColored {
    public class TriangleColoredVertexRenderTechnique<TProperties> :
        NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties>, IGraphicSystemContextDependent
        where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.Techniques.TriangleColored.colored_vertex.hlsl";
        const string gs_flat_shading = @"D3DLab.Toolkit.Techniques.TriangleColored.gs_flat_shading.hlsl";
        const string wireframe = @"D3DLab.Toolkit.Techniques.TriangleColored.wireframe.hlsl";

        readonly D3DShaderTechniquePass pass;
        readonly D3DShaderTechniquePass flatShadingPass;
        readonly D3DShaderTechniquePass wireframePass;
        readonly VertexLayoutConstructor layconst;
        //readonly ShaderDebugMode debug1;
        //static readonly ShaderDebugMode debug2;

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

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass, flatShadingPass, wireframePass };

        public TriangleColoredVertexRenderTechnique() {

            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3();

            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(TriangleColoredVertexRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "CV_"));
            flatShadingPass = new D3DShaderTechniquePass(d.Load(gs_flat_shading, "FCV_"));
            wireframePass = new D3DShaderTechniquePass(d.Load(wireframe, "WCV_"));

            //debug1 = new ShaderDebugMode(new DirectoryInfo(@"D:\Storage_D\trash\archive\shaders\"), pass);
            //debug1.Activate();
            //debug2 = new ShaderDebugMode(new DirectoryInfo(@"D:\Storage_D\trash\archive\shaders\"), flatShadingPass);
            //debug2.Activate();

            depthStencilStateDesc = D3DDepthStencilDefinition.DepthEnabled;
            blendStateDesc = D3DBlendStateDescriptions.BlendStateEnabled;
            // blendStateDesc.AlphaToCoverageEnable = true;

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            flatShadingGS = new DisposableSetter<GeometryShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);

            wireframePS = new DisposableSetter<PixelShader>(disposer);
            wireframeGS = new DisposableSetter<GeometryShader>(disposer);
        }

        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<InputLayout> inputLayout;

        readonly DisposableSetter<GeometryShader> flatShadingGS;

        readonly DisposableSetter<PixelShader> wireframePS;
        readonly DisposableSetter<GeometryShader> wireframeGS;

        readonly BlendStateDescription blendStateDesc;
        readonly D3DDepthStencilDefinition depthStencilStateDesc;

        public IContextState ContextState { private get; set; }

        protected override void Rendering(GraphicsDevice graphics, TProperties props) {
            BeforeRender(graphics);

            foreach (var en in entities) {
                RenderEach(graphics, props, en);
            }
        }

        public override void CleanupRenderCache() {
            pass.ClearCache();
            flatShadingPass.ClearCache();
            wireframePass.ClearCache();
            base.CleanupRenderCache();
        }

        void DrawWithAlfaBlending(GraphicsDevice graphics,
            RasterizerStateDescription2 stated,
            IGeometryData geo) {
            var context = graphics.ImmediateContext;

            var mode = stated.CullMode;



            if (mode == CullMode.Back || mode == CullMode.None) {
                stated.CullMode = CullMode.Back;

                using (var rasterizerState = graphics.CreateRasterizerState(stated)) {
                    context.Rasterizer.State = rasterizerState;

                    graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                }
            }

            if (mode == CullMode.Front || mode == CullMode.None) {
                stated.CullMode = CullMode.Front;

                using (var rasterizerState = graphics.CreateRasterizerState(stated)) {
                    context.Rasterizer.State = rasterizerState;

                    graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                }
            }
        }

        #region 

        public void BeforeRender(GraphicsDevice graphics) {
            var device = graphics.D3DDevice;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();
                var inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

                inputLayout.Set(new InputLayout(device, inputSignature, layconst.ConstuctElements()));
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));

                if (pass.PixelShader != null) {
                    pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
                }
            }

            if (!flatShadingPass.IsCompiled) {
                flatShadingPass.Compile(graphics.Compilator);
                if (flatShadingPass.GeometryShader != null) {
                    flatShadingGS.Set(new GeometryShader(device, flatShadingPass.GeometryShader.ReadCompiledBytes()));
                }
            }

            if (!wireframePass.IsCompiled) {
                wireframePass.Compile(graphics.Compilator);
                if (wireframePass.GeometryShader != null) {
                    wireframeGS.Set(new GeometryShader(device, wireframePass.GeometryShader.ReadCompiledBytes()));
                }
                if (wireframePass.GeometryShader != null) {
                    wireframePS.Set(new PixelShader(device, wireframePass.PixelShader.ReadCompiledBytes()));
                }
            }
        }

        public void RenderEach(GraphicsDevice graphics, TProperties props, GraphicEntity en) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!en.TryGetComponent<D3DRenderComponent>(out var render)) {
                render = new D3DRenderComponent();
                en.AddComponent(render);
            }

            var renderable = en.GetComponent<RenderableComponent>();
            var color = en.GetComponent<MaterialColorComponent>();
            

            var geo = ContextState.GetGeometryPool().GetGeometry<IGeometryData>(en);

            if (renderable.DepthStencilStateDefinition.IsValid) {
                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice,
                        renderable.DepthStencilStateDefinition.Description));
                }
            } else { //TODO remake this by checking CullMode.None
                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice,
                        color.HasAlpha ? D3DDepthStencilDefinition.DepthDisabled.Description : depthStencilStateDesc.Description));
                } else if (color.HasAlpha && render.DepthStencilState.Get().Description.IsDepthEnabled) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice,
                        D3DDepthStencilDefinition.DepthDisabled.Description));
                } else if (!color.HasAlpha && !render.DepthStencilState.Get().Description.IsDepthEnabled) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice, depthStencilStateDesc.Description));
                }
            }

            if (!render.BlendingState.HasValue) {
                render.BlendingState.Set(new BlendState(graphics.D3DDevice, renderable.BlendStateDescription));
            }

            {
                graphics.ClearAllShader();
                context.VertexShader.Set(vertexShader.Get());
                context.GeometryShader.Set(null);
                context.PixelShader.Set(pixelShader.Get());

                if (en.Contains<FlatShadingGeometryComponent>()) {
                    context.GeometryShader.Set(flatShadingGS.Get());
                } else if (en.Contains<WireframeGeometryComponent>()) {
                    context.GeometryShader.Set(wireframeGS.Get());
                    context.PixelShader.Set(wireframePS.Get());
                }
            }

            var topology = renderable.PrimitiveTopology;

            if (geo.IsModified || (!render.VertexBuffer.HasValue && !render.IndexBuffer.HasValue)) {
                Vertex[] vertex= null;
                switch (geo.Topology) {
                    case GeometryPrimitiveTopologies.TriangleList:
                        topology = PrimitiveTopology.TriangleList;
                        vertex = new Vertex[geo.Positions.Length];
                        for (var index = 0; index < vertex.Length; index++) {
                            vertex[index] = new Vertex(geo.Positions[index], geo.Normals[index]);
                        }
                        break;

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
                context.InputAssembler.PrimitiveTopology = topology;

                context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
                context.OutputMerger.SetBlendState(render.BlendingState.Get(),
                    new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);
            }

            var rasterizerDesc = renderable.RasterizerStateDescription;

            if (color.HasAlpha) {
                DrawWithAlfaBlending(graphics, rasterizerDesc, geo);
            } else {
                using (var rasterizerState = graphics.CreateRasterizerState(rasterizerDesc)) {
                    context.Rasterizer.State = rasterizerState;

                    graphics.ImmediateContext.DrawIndexed(geo.Indices.Length, 0, 0);
                }
            }
        }

        #endregion

        public override bool IsAplicable(GraphicEntity entity) {
            return entity.TryGetComponent<RenderableComponent>(out var ren)
                && ren.IsRenderable
                && ren.Technique == RenderTechniques.TriangleColored
                && entity.Contains(
                    typeof(GeometryPoolComponent),
                    typeof(TransformComponent),
                    typeof(MaterialColorComponent));
        }
    }
}
