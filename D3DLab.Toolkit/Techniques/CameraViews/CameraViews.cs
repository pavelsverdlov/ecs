using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Ext;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using D3DLab.Toolkit.Components;

using SharpDX.Direct3D11;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace D3DLab.Toolkit.Techniques.CameraViews {
    readonly struct CameraViewsComponent : IGraphicComponent {

        public static CameraViewsComponent Create() {
            return new CameraViewsComponent(0.15f) {
            };
        }

        public ElementTag Tag { get; }
        public bool IsValid { get; }
        public float Size { get; }

        public CameraViewsComponent(float size) : this() {
            Size = size;
        }
        public void Dispose() {
        }
    }

    public class CameraViewsRenderTechnique<TProperties>
        : NestedRenderTechniqueSystem<TProperties>, IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.D3D.CameraViews.camera_views.hlsl";
        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public Vertex(Vector3 position) {
                Position = position;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }


        readonly D3DShaderTechniquePass pass;
        readonly DisposableSetter<VertexShader> vertexShader;
        readonly DisposableSetter<PixelShader> pixelShader;
        readonly DisposableSetter<DepthStencilState> depthStencilState;
        readonly RasterizerStateDescription2 rasterizerStateDescription;
        readonly VertexLayoutConstructor layconst;

        public CameraViewsRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3();

            var d = new CombinedShadersLoader(new ECS.Common.ManifestResourceLoader(typeof(CameraViewsRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "CameraViews_"));

            vertexShader = new DisposableSetter<VertexShader>(disposer);
            pixelShader = new DisposableSetter<PixelShader>(disposer);
            depthStencilState = new DisposableSetter<DepthStencilState>(disposer);

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

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };

        #region render

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            var device = graphics.D3DDevice;
            var context = graphics.ImmediateContext;

            if (!pass.IsCompiled) {
                pass.Compile(graphics.Compilator);
                var vertexShaderByteCode = pass.VertexShader.ReadCompiledBytes();
                vertexShader.Set(new VertexShader(device, vertexShaderByteCode));
                pixelShader.Set(new PixelShader(device, pass.PixelShader.ReadCompiledBytes()));
            }

            if (!depthStencilState.HasValue) {
                depthStencilState.Set(new DepthStencilState(graphics.D3DDevice, D3DDepthStencilDefinition.DepthDisabled.Description));
            }
            foreach(var en in entities) {
                if (en.TryGetComponent<CameraViewsComponent>(out var com)) {

                }
            }

        }

        public override bool IsAplicable(GraphicEntity entity) {
            return entity.Contains<CameraViewsComponent>();
        }


        #endregion
    }

    public class CameraViewsObject {
        public static CameraViewsObject Create(IEntityManager manager) {

            throw new NotImplementedException();

            var cvcom = CameraViewsComponent.Create();

            //var halfSize = cvcom.Size * 0.5f;
            //var boxgeo = GeometryBuilder.BuildGeoBox(new AxisAlignedBox(new Vector3(-halfSize, -halfSize, -halfSize), new Vector3(halfSize, halfSize, halfSize)));

            //var move = Matrix4x4.CreateTranslation(new Vector3(1, 0, 0));
            //var geoc = new SimpleGeometryComponent();
            //geoc.Positions = boxgeo.Positions.ToArray()
            //   /// .Transform(ref move)
            //    .ToImmutableArray();
            //geoc.Indices = boxgeo.Indices.ToImmutableArray();
            //geoc.Normals = boxgeo.Positions.CalculateNormals(boxgeo.Indices).ToImmutableArray();



            //manager.CreateEntity(new ElementTag("CameraViews"))
            //    .AddComponents(
            //        new CameraViewsRenderComponent(),
            //        cvcom,
            //        geoc,
            //        ColorComponent.CreateAmbient(V4Colors.Blue).ApplyOpacity(0.2f)
            //    );


            return new CameraViewsObject();
        }
    }

}
