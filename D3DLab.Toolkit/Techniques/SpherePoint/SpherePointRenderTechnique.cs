using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit._CommonShaders;
using D3DLab.Toolkit.Components;

using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.Toolkit.Techniques.SpherePoint {
    readonly struct SpherePointComponent : IGraphicComponent{
     
        public static SpherePointComponent Create(Vector3 center,float radius) {
            return new SpherePointComponent(center, radius);
        }

        public Vector3 Center { get; }
        public float Radius { get; }
        public ElementTag Tag { get; }
        public bool IsValid { get; }
        public SpherePointComponent(Vector3 center,  float radius) : this() {
            Center = center;
            Radius = radius;
            IsValid = true;
            Tag = ElementTag.New();
        }
        public void Dispose() { }
    }
    public class SpherePointRenderTechnique<TProperties> :
        NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties>, IGraphicSystemContextDependent
        where TProperties : IToolkitFrameProperties {

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector4 Position;
            public readonly Vector4 DiffuseColor;
            public Vertex(Vector3 position, Vector4 color) {
                Position = position.ToVector4();
                DiffuseColor = color;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        const string path = @"D3DLab.Toolkit.Techniques.SpherePoint.sphere_point.hlsl";
        readonly D3DShaderTechniquePass pass;
        readonly VertexLayoutConstructor layconst;
        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<GeometryShader> geometryShader;
        readonly DisposableSetter<DepthStencilState> depthStencilState;
        readonly DisposableSetter<InputLayout> inputLayout;
        readonly RasterizerStateDescription2 rasterizerStateDescription;

        public SpherePointRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
                .AddPositionElementAsVector4()
                .AddColorElementAsVector4();
            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(SpherePointRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "SpP_"));

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            geometryShader = new DisposableSetter<GeometryShader>(disposer);
            depthStencilState = new DisposableSetter<DepthStencilState>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);

            rasterizerStateDescription = new RasterizerStateDescription2() {
                CullMode = CullMode.None,
                FillMode = FillMode.Solid,
                IsMultisampleEnabled = false,

                IsFrontCounterClockwise = false,
                IsScissorEnabled = false,
                IsAntialiasedLineEnabled = false,
                DepthBias = 0,
                DepthBiasClamp = .0f,
                SlopeScaledDepthBias = .0f
            };
        }

        public IContextState ContextState { set; private get; }

        public IEnumerable<IRenderTechniquePass> GetPass() => pass.ToEnumerable();

        public override bool IsAplicable(GraphicEntity entity) =>
            entity.TryGetComponent<RenderableComponent>(out var ren)
            && ren.IsRenderable
            && ren.Technique == RenderTechniques.SpherePoints
            && entity.Contains<SpherePointComponent>();


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
                geometryShader.Set(new GeometryShader(device, pass.GeometryShader.ReadCompiledBytes()));
            }

            if (!depthStencilState.HasValue) {
                depthStencilState.Set(new DepthStencilState(graphics.D3DDevice, D3DDepthStencilDefinition.DepthDisabled.Description));
            }


            foreach (var entity in entities) {
                var point = entity.GetComponent<SpherePointComponent>();
                var renderable = entity.GetComponent<RenderableComponent>();
                var material = entity.GetComponent<MaterialColorComponent>();

                if (!entity.TryGetComponent<D3DRenderComponent>(out var render)) {
                    render = new D3DRenderComponent();
                    entity.AddComponent(render);
                }

                if (!render.DepthStencilState.HasValue) {
                    render.DepthStencilState.Set(new DepthStencilState(graphics.D3DDevice,
                        renderable.DepthStencilStateDefinition.Description));
                }
                if (!render.BlendingState.HasValue) {
                    render.BlendingState.Set(new BlendState(graphics.D3DDevice, renderable.BlendStateDescription));
                }

                graphics.ClearAllShader();
                graphics.SetVertexShader(vertexShader);
                graphics.SetGeometryShader(geometryShader);
                graphics.SetPixelShader(pixelShader);

                render.VertexBuffer.Set(graphics.CreateBuffer(BindFlags.VertexBuffer, 
                    new Vertex[] { 
                        new Vertex(point.Center, material.Diffuse)
                    }));
                render.IndexBuffer.Set(graphics.CreateBuffer(BindFlags.IndexBuffer, new []{ 0 }));

                UpdateTransformWorld(graphics, render, entity);

                {//Update constant buffers
                    context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, props.Game);
                    context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, render.TransformWorldBuffer.Get());

                    context.GeometryShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, props.Game);
                    context.GeometryShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, render.TransformWorldBuffer.Get());

                    context.PixelShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, props.Game);
                    context.PixelShader.SetConstantBuffer(LightStructBuffer.RegisterResourceSlot, props.Lights);
                    if (render.MaterialBuffer.HasValue) {
                        context.PixelShader.SetConstantBuffer(MaterialStructBuffer.RegisterResourceSlot, render.MaterialBuffer.Get());
                    }
                }

                DefaultUpdateInputOutput(graphics,render, layconst, inputLayout, renderable);

                using (var rasterizerState = graphics.CreateRasterizerState(renderable.RasterizerStateDescription)) {
                    context.Rasterizer.State = rasterizerState;
                    graphics.ImmediateContext.Draw(1, 0);
                }
            }
        }
    }
}
