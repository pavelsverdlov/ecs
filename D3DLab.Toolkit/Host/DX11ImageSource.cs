using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace D3DLab.Toolkit.Host {
    /// <summary>
    /// Image for hosting DX11ImageSource
    /// 
    /// <OutputType>Library</OutputType>
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    /// 
    public sealed class DX11ImageHost : System.Windows.Controls.Image {

        public static readonly DependencyProperty OverlayProperty =
          DependencyProperty.Register(nameof(OverlayProperty), typeof(FrameworkElement), typeof(DX11ImageHost));

        public static readonly DependencyProperty HostLoadedProperty =
           DependencyProperty.Register(nameof(HostLoaded), typeof(ICommand), typeof(DX11ImageHost));
        public static readonly DependencyProperty HostUnLoadedProperty =
           DependencyProperty.Register(nameof(HostUnLoaded), typeof(ICommand), typeof(DX11ImageHost));
        
        public ICommand HostLoaded {
            get { return (ICommand)this.GetValue(HostLoadedProperty); }
            set { this.SetValue(HostLoadedProperty, value); }
        }
        public ICommand HostUnLoaded {
            get { return (ICommand)this.GetValue(HostUnLoadedProperty); }
            set { this.SetValue(HostUnLoadedProperty, value); }
        }

        public FrameworkElement Overlay {
            get { return (FrameworkElement)this.GetValue(OverlayProperty); }
            set { this.SetValue(OverlayProperty, value); }
        }


        public event Action<Size> RenderSizeChanged;

        Size PreviousSize;

        public DX11ImageHost() {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            HostUnLoaded?.Execute(sender);
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            HostLoaded?.Execute(sender);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            if (sizeInfo.NewSize.Width != 0 && sizeInfo.NewSize.Height != 0 && PreviousSize != sizeInfo.NewSize) {
                PreviousSize = sizeInfo.NewSize;
                RenderSizeChanged?.Invoke(sizeInfo.NewSize);
            }
        }
    }
    /// <summary>
    /// usage: Image.Source = new DX11ImageSource();
    /// </summary>
    public sealed class DX11ImageSource : D3DImage, IDisposable {
        private SharpDX.Direct3D9.Direct3DEx context;
        private SharpDX.Direct3D9.DeviceEx device;

        private readonly int adapterIndex;
        private SharpDX.Direct3D9.Texture renderTarget;

        public bool IsInitilized { get; private set; }
        
        public DX11ImageSource(int adapterIndex = 0) {
            this.adapterIndex = adapterIndex;
            this.StartD3D();
        }

        /// <summary>
        /// Invoke after each frame
        /// </summary>
        public void InvalidateD3DImage() {
            if (this.renderTarget != null) {
                base.Lock();
                base.AddDirtyRect(new Int32Rect(0, 0, base.PixelWidth, base.PixelHeight));
                base.Unlock();
            }
        }
        /// <summary>
        /// Initialization, invoke in change directx device or surface size
        /// </summary>
        /// <param name="target"></param>
        public void SetRenderTargetDX11(Texture2D target) {
            if (this.renderTarget != null) {
                renderTarget.Dispose();
                renderTarget = null;
                base.Lock();
                base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                base.Unlock();
            }
            IsInitilized = target != null;
            if (target == null)
                return;

            if (!IsShareable(target))
                throw new ArgumentException("Texture must be created with ResourceOptionFlags.Shared");

            var format = TranslateFormat(target);
            if (format == SharpDX.Direct3D9.Format.Unknown)
                throw new ArgumentException("Texture format is not compatible with OpenSharedResource");

            var handle = GetSharedHandle(target);
            if (handle == IntPtr.Zero)
                throw new ArgumentNullException("Handle");

            try {
                this.renderTarget = new SharpDX.Direct3D9.Texture(device,
                    target.Description.Width, target.Description.Height, 1,
                    SharpDX.Direct3D9.Usage.RenderTarget, format,
                    SharpDX.Direct3D9.Pool.Default, ref handle);

                using (var surface = this.renderTarget.GetSurfaceLevel(0)) {
                    base.Lock();
#if NET40
                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
#else
                    // "enableSoftwareFallback = true" makes Remote Desktop possible.
                    // See: http://msdn.microsoft.com/en-us/library/hh140978%28v=vs.110%29.aspx
                    base.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer, true);
#endif
                    base.Unlock();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
           
        }
       
        void StartD3D() {
            context = new SharpDX.Direct3D9.Direct3DEx();
            // Ref: https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/wpf-and-direct3d9-interoperation
            var presentparams = new SharpDX.Direct3D9.PresentParameters {
                Windowed = true,
                SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
                //DeviceWindowHandle = GetDesktopWindow(),
                PresentationInterval = SharpDX.Direct3D9.PresentInterval.Default,
                BackBufferHeight = 1,
                BackBufferWidth = 1,
                BackBufferFormat = SharpDX.Direct3D9.Format.Unknown
            };

            device = new SharpDX.Direct3D9.DeviceEx(context, this.adapterIndex,
                SharpDX.Direct3D9.DeviceType.Hardware, IntPtr.Zero,
                SharpDX.Direct3D9.CreateFlags.HardwareVertexProcessing 
                    | SharpDX.Direct3D9.CreateFlags.Multithreaded 
                    | SharpDX.Direct3D9.CreateFlags.FpuPreserve, 
                presentparams);
        }

        public void EndD3D() {
            renderTarget.Dispose();
            device.Dispose();
            context.Dispose();
            renderTarget = null;
            device = null;
            context = null;
        }

        static IntPtr GetSharedHandle(Texture2D sharedTexture) {
            using (var resource = sharedTexture.QueryInterface<global::SharpDX.DXGI.Resource>()) {
                IntPtr result = resource.SharedHandle;
                return result;
            }
        }

        static SharpDX.Direct3D9.Format TranslateFormat(Texture2D sharedTexture) {
            switch (sharedTexture.Description.Format) {
                case global::SharpDX.DXGI.Format.R10G10B10A2_UNorm:
                    return SharpDX.Direct3D9.Format.A2B10G10R10;

                case global::SharpDX.DXGI.Format.R16G16B16A16_Float:
                    return SharpDX.Direct3D9.Format.A16B16G16R16F;

                case global::SharpDX.DXGI.Format.B8G8R8A8_UNorm:
                    return SharpDX.Direct3D9.Format.A8R8G8B8;

                default:
                    return SharpDX.Direct3D9.Format.Unknown;
            }
        }

        static bool IsShareable(Texture2D sharedTexture) {
            return (sharedTexture.Description.OptionFlags & ResourceOptionFlags.Shared) != 0;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        [SuppressMessage("Microsoft.Usage", "CA2213: Disposable fields should be disposed", Justification = "False positive.")]
        void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    EndD3D();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DX11ImageSource() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

    public static class NativeMethods {
        [DllImport("user32.dll", SetLastError = false)]
        private static extern IntPtr GetDesktopWindow();
    }
}
