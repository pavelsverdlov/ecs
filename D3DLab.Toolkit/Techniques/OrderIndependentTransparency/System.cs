using D3DLab.ECS;
using D3DLab.ECS.Filter;
using D3DLab.ECS.Shaders;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.SDX.Engine.Shader;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace D3DLab.Toolkit.Techniques.OrderIndependentTransparency {
    public class OITComponent : D3DRenderComponent {
        public Texture2D UnorderedViewTexture { get; set; }
        public UnorderedAccessView UnorderedView { get; set; }
    }
    public class CameraViewsRenderTechnique<TProperties> : NestedRenderTechniqueSystem<TProperties>,
        IRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {
        const string path = @"D3DLab.Toolkit.D3D.CameraViews.camera_views.hlsl";

        static readonly D3DShaderTechniquePass pass;
        static readonly VertexLayoutConstructor layconst;

        static CameraViewsRenderTechnique() {
            layconst = new VertexLayoutConstructor(Vertex.Size)
               .AddPositionElementAsVector3()
               .AddNormalElementAsVector3()
               .AddColorElementAsVector4();

            var d = new CombinedShadersLoader(new ECS.Common.ManifestResourceLoader(typeof(CameraViewsRenderTechnique<>)));
            pass = new D3DShaderTechniquePass(d.Load(path, "CameraViews_"));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector4 Color;
            public Vertex(Vector3 position, Vector3 normal, Vector4 color) {
                Position = position;
                Normal = normal;
                Color = color;
            }
            public static readonly int Size = Unsafe.SizeOf<Vertex>();
        }

        public CameraViewsRenderTechnique() { }
        public override bool IsAplicable(GraphicEntity entity) {
            throw new NotImplementedException();
        }

        protected override void Rendering(GraphicsDevice graphics, TProperties game) {
            throw new NotImplementedException();
        }

        public IEnumerable<IRenderTechniquePass> GetPass() => new[] { pass };

    }
}
