using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.SDX.Engine.Rendering;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Numerics;

namespace D3DLab.SDX.Engine.Components {
    
    public class D3DRenderComponent : GraphicComponent {
        public bool CanRender { get; set; }

        /// <summary>
        /// SharpDX.RasterizerState must not be keeped in componet it should created new one each time on frame by descriptor.
        /// That is why component has only RasterizerStateDescription not SharpDX.RasterizerState object
        /// </summary>
        /// <remarks>
        /// RenderComponent must have only D3D resources and not be avaliable outside of render systems, move desctiptor to other components to allow change it in realtime 
        /// </remarks>
        public D3DRasterizerState RasterizerStateDescription { get; set; }

        public PrimitiveTopology PrimitiveTopology { get; set; }

        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> TransformWorldBuffer { get; set; }
        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> VertexBuffer { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> IndexBuffer { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<DepthStencilState> DepthStencilState { get; private set; }
        [IgnoreDebuging]
        public DisposableSetter<BlendState> BlendingState { get; private set; }

        [IgnoreDebuging]
        public DisposableSetter<SharpDX.Direct3D11.Buffer> MaterialBuffer { get; }


        [IgnoreDebuging]
        public EnumerableDisposableSetter<ShaderResourceView[]> TextureResources { get; set; }
        [IgnoreDebuging]
        public DisposableSetter<SamplerState> SampleState { get; set; }

        protected readonly DisposeObserver disposer;
        Matrix4x4 prevMatrix;

        public D3DRenderComponent() {
            CanRender = true;
            IsModified = true;
            disposer = new DisposeObserver();
            TransformWorldBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            VertexBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            IndexBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            DepthStencilState = new DisposableSetter<DepthStencilState>(disposer);
            BlendingState = new DisposableSetter<BlendState>(disposer);
            MaterialBuffer = new DisposableSetter<SharpDX.Direct3D11.Buffer>(disposer);
            SampleState = new DisposableSetter<SamplerState>(disposer);
            TextureResources = new EnumerableDisposableSetter<ShaderResourceView[]>(disposer);
        }

        public override void Dispose() {
            base.Dispose();
            disposer.Dispose();
            CanRender = false;
            IsModified = false;
        }

        public virtual void ClearBuffers() {
            disposer.DisposeObservables();
            CanRender = true;
            IsModified = true;
        }

        /// <summary>
        /// Update matrix of this render components 
        /// TRUE - if matrix has diff and was updated
        /// FALSE - matrix is the same
        /// </summary>
        public bool TryUpdateMatrix(Matrix4x4 newpne) {
            if(prevMatrix == newpne) return false; 
            prevMatrix = newpne;
            return true;
        }

    }

}
