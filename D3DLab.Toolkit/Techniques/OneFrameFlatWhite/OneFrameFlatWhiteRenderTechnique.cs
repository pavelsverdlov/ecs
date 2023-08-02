using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit;
using D3DLab.Toolkit._CommonShaders;
using D3DLab.Toolkit.Components;
using D3DLab.Toolkit.Techniques.TriangleColored;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.Toolkit.Techniques.BlackAndWhite {
    public readonly struct BlackAndWhiteRenderComponent : IGraphicComponent {
      
        public static BlackAndWhiteRenderComponent Create() {
            return new BlackAndWhiteRenderComponent(true);
        }
        public ElementTag Tag { get; }
        public bool IsModified { get; }
        public bool IsValid { get; }
        public bool IsDisposed { get;  }
        BlackAndWhiteRenderComponent(bool isValid) : this() {
            IsValid = isValid;
            Tag = ElementTag.New();
        }

        public void Dispose() {
        }
    }
    public class OneFrameFlatWhiteRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties>
        where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.Techniques.OneFrameFlatWhite.shader.hlsl";

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public Vertex(Vector3 position) {
                Position = position;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        readonly D3DShaderTechniquePass pass;
        readonly VertexLayoutConstructor layconst;
        readonly DisposableSetter<InputLayout> inputLayout;
        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly RasterizerStateDescription2 rasterizerStateDescription;

        public OneFrameFlatWhiteRenderTechnique() {
            var d = new CombinedShadersLoader(new ManifestResourceLoader(typeof(OneFrameFlatWhiteRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "BW_"));

            layconst = new VertexLayoutConstructor(Vertex.Size)
              .AddPositionElementAsVector3();

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            inputLayout = new DisposableSetter<InputLayout>(disposer);

            rasterizerStateDescription = new RasterizerStateDescription2() {
                CullMode = CullMode.Front,
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

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };
        public override void CleanupRenderCache() {
            base.CleanupRenderCache();
            //pass.ClearCache();
        }

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

            DepthStencilState depthStencilState = null;
            BlendState blendingState = null;

            try {
                depthStencilState = new DepthStencilState(graphics.D3DDevice, D3DDepthStencilDefinition.DepthDisabled.Description);
                blendingState = new BlendState(graphics.D3DDevice, D3DBlendStateDescriptions.BlendStateEnabled);

                foreach (var en in entities) {
                    if (!en.Contains<BlackAndWhiteRenderComponent>()) {
                        continue;
                    }
                    var geo = en.GetComponent<GeometryComponent>();
                    var renderable = en.GetComponent<RenderableComponent>();
                    var transform = en.GetComponent<TransformComponent>();

                    graphics.ClearAllShader();
                    graphics.SetVertexShader(vertexShader);
                    graphics.SetPixelShader(pixelShader);

                    SharpDX.Direct3D11.Buffer vertexBuffer = null;
                    SharpDX.Direct3D11.Buffer indexBuffer = null;
                    SharpDX.Direct3D11.Buffer transformWorldBuffer = null;

                    try {
                        var vertex = new Vertex[geo.Positions.Length];
                        for (var index = 0; index < vertex.Length; index++) {
                            vertex[index] = new Vertex(geo.Positions[index]);
                        }

                        vertexBuffer = graphics.CreateBuffer(BindFlags.VertexBuffer, vertex);
                        indexBuffer = graphics.CreateBuffer(BindFlags.IndexBuffer, geo.Indices.ToArray());
                        transformWorldBuffer = CreateTransformWorldBuffer(graphics, ref transform);

                        context.VertexShader.SetConstantBuffer(GameStructBuffer.RegisterResourceSlot, props.Game);
                        context.VertexShader.SetConstantBuffer(TransforStructBuffer.RegisterResourceSlot, transformWorldBuffer);

                        context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, layconst.VertexSize, 0));
                        context.InputAssembler.SetIndexBuffer(indexBuffer, SharpDX.DXGI.Format.R32_UInt, 0);

                        context.InputAssembler.InputLayout = inputLayout.Get();
                        context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;

                        context.OutputMerger.SetDepthStencilState(depthStencilState);
                        context.OutputMerger.SetBlendState(blendingState,
                            new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);

                        using (var rasterizerState = graphics.CreateRasterizerState(rasterizerStateDescription)) {
                            context.Rasterizer.State = rasterizerState;
                            context.DrawIndexed(geo.Indices.Length, 0, 0);
                        }

                    } finally {
                        vertexBuffer?.Dispose();
                        indexBuffer?.Dispose();
                        transformWorldBuffer?.Dispose();
                    }
                }
            } finally {
                depthStencilState?.Dispose();
                blendingState?.Dispose();
                blendingState = null;
                depthStencilState = null;
            }
        }

        public override bool IsAplicable(GraphicEntity entity) {
            return entity.TryGetComponent<RenderableComponent>(out var ren)
                && ren.IsRenderable
                && ren.Technique == RenderTechniques.OneFrameFlatWhite
                && entity.Contains(
                    typeof(BlackAndWhiteRenderComponent), 
                    typeof(TransformComponent));
        }
    }
}
