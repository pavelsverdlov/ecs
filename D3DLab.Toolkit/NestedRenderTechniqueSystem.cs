using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Filter;
using D3DLab.SDX.Engine;
using D3DLab.SDX.Engine.Components;
using D3DLab.SDX.Engine.Rendering;
using D3DLab.Toolkit._CommonShaders;
using D3DLab.Toolkit.Components;

using SharpDX.Direct3D11;

using System;
using System.Runtime.CompilerServices;

namespace D3DLab.Toolkit {

    public class RenderTechniqueException : Exception {
        public RenderTechniqueException(string mess) : base(mess) { }
        public static RenderTechniqueException NoVertexAndIndexBuffers =>
            new RenderTechniqueException("Should be initialized at least one Vertex OR Index buffer.");
        public static RenderTechniqueException NoWorldTransformBuffers =>
           new RenderTechniqueException("Should be initialized World Transform buffer.");
    }

    public abstract class NestedRenderTechniqueSystem<TProperties> : D3DAbstractRenderTechnique<TProperties> where TProperties : IToolkitFrameProperties {

        protected NestedRenderTechniqueSystem() {
            disposer = new DisposeObserver();
        }

        protected void UpdateMaterial(GraphicsDevice graphics, D3DRenderComponent render, MaterialColorComponent material) {
            if (material.IsValid) {
                var buf = MaterialStructBuffer.From(material);

                if (render.MaterialBuffer.HasValue) {
                    var buff = render.MaterialBuffer.Get();
                    graphics.UpdateDynamicBuffer(ref buf, buff);
                } else {
                    var buff = graphics.CreateDynamicBuffer(ref buf, Unsafe.SizeOf<MaterialStructBuffer>());
                    render.MaterialBuffer.Set(buff);
                }
            }
        }
        protected void UpdateTransformWorld(GraphicsDevice graphics, D3DRenderComponent render, GraphicEntity en) {
            var transform = en.GetComponent<TransformComponent>();
            var matrixWorld = transform.MatrixWorld;

            //var isChanged = false;
            //if(en.TryGetComponent<MovingComponent>(out var moving)) {
            //    isChanged = true;
            //    matrixWorld *= moving.MovingMatrix;
            //    en.RemoveComponent(moving);
            //    en.UpdateComponent(TransformComponent.Create(matrixWorld));
            //}

            if (!render.TransformWorldBuffer.HasValue || render.TryUpdateMatrix(matrixWorld)) {
                var tr = TransforStructBuffer.ToTranspose(matrixWorld);

                if (render.TransformWorldBuffer.HasValue) {
                    var buff = render.TransformWorldBuffer.Get();
                    graphics.UpdateDynamicBuffer(ref tr, buff);
                } else {
                    var buff = graphics.CreateDynamicBuffer(ref tr, Unsafe.SizeOf<TransforStructBuffer>());
                    render.TransformWorldBuffer.Set(buff);
                }
            }
        }
        protected void DefaultUpdateInputOutput(GraphicsDevice graphics, D3DRenderComponent render,
            VertexLayoutConstructor layconst, DisposableSetter<InputLayout> inputLayout, RenderableComponent renderable) {
            var context = graphics.ImmediateContext;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(render.VertexBuffer.Get(),
                       layconst.VertexSize, 0));
            context.InputAssembler.SetIndexBuffer(render.IndexBuffer.Get(), SharpDX.DXGI.Format.R32_UInt, 0);

            context.InputAssembler.InputLayout = inputLayout.Get();
            context.InputAssembler.PrimitiveTopology = renderable.PrimitiveTopology;

            context.OutputMerger.SetDepthStencilState(render.DepthStencilState.Get(), 0);
            context.OutputMerger.SetBlendState(render.BlendingState.Get(),
                new SharpDX.Mathematics.Interop.RawColor4(0, 0, 0, 0), -1);
        }


        protected SharpDX.Direct3D11.Buffer CreateTransformWorldBuffer(GraphicsDevice graphics, ref TransformComponent transform) {
            var tr = TransforStructBuffer.ToTranspose(transform.MatrixWorld);
            return graphics.CreateDynamicBuffer(ref tr, Unsafe.SizeOf<TransforStructBuffer>());
        }

        protected readonly DisposeObserver disposer;

        public override void CleanupRenderCache() {
            disposer.DisposeObservables();
            base.CleanupRenderCache();
        }

    }
}
