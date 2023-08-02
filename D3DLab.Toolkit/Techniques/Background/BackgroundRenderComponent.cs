using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using SharpDX.Direct3D11;
using System;

namespace D3DLab.Toolkit.Techniques.Background {
    [Obsolete("TODO remake")]
    public class BackgroundRenderComponent : IGraphicComponent {
        public static BackgroundRenderComponent Create() {
            return new BackgroundRenderComponent {
                IsValid = true,
                IsModified = true,
                canrender = true,
                Tag = new ElementTag("BackgroundRenderComponent"),

            };
        }
        public ElementTag Tag { get; private set; }
        public ElementTag EntityTag { get; set; }
        public bool IsModified { get; set; }
        public bool IsValid { get; private set; }
        public bool IsDisposed { get; }
        public bool CanRender {
            get => canrender;
            set => throw new InvalidOperationException();
        }
        bool canrender;

        [IgnoreDebuging]
        internal EnumerableDisposableSetter<ShaderResourceView[]> TextureResources { get; }
        [IgnoreDebuging]
        internal DisposableSetter<SamplerState> SampleState { get; }

        readonly DisposeObserver disposer;
        BackgroundRenderComponent() {
            disposer = new DisposeObserver();
            TextureResources = new EnumerableDisposableSetter<ShaderResourceView[]>(disposer);
            SampleState = new DisposableSetter<SamplerState>(disposer);
        }

        public void ClearBuffers() {
            disposer.DisposeObservables();
            canrender = true;
            IsModified = true;
        }

        public void Dispose() {
            disposer.Dispose();
            canrender = false;
            IsModified = false;
        }
    }
}
