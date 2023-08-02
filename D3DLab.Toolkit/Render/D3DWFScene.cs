using D3DLab.ECS;
using D3DLab.ECS.Common;
using D3DLab.ECS.Components;
using D3DLab.ECS.Context;
using D3DLab.ECS.Input;
using D3DLab.Toolkit.Host;
using D3DLab.Toolkit.Input;

using SharpDX.DXGI;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms.Integration;

namespace D3DLab.Toolkit.Render {
    public class WpfWindows : IRenderableWindow {
        readonly WinFormsD3DControl control;

        public WpfWindows(WinFormsD3DControl control, WindowsFormsHost host, DefaultInputObserver input) {
            InputManager = new InputManager(input);
            this.control = control;
            this.control.Resize += OnControlResized;
            this.control.Paint += OnControlPaint;
            host.SizeChanged += OnHostSizeChanged;

            var width = (float)control.Width;
            var height = (float)control.Height;
            Size = new SurfaceSize(width, height);
        }

        private void OnControlPaint(object sender, System.Windows.Forms.PaintEventArgs e) {
            Invalidated();
        }

        private void OnHostSizeChanged(object sender, EventArgs e) {
            var width = (float)control.Width;
            var height = (float)control.Height;
            Size = new SurfaceSize(width, height);
            Resized();
        }

        private void OnControlResized(object sender, EventArgs e) {
            var width = (float)control.Width;
            var height = (float)control.Height;
            Size = new SurfaceSize(width, height);
            Resized();
        }

        public event Action Resized = () => { };
        public event Action Invalidated = () => { };


        public bool IsActive => true;
        public IntPtr Handle => control.Handle;
        public IInputManager InputManager { get; }
        public SurfaceSize Size { get; private set; }

        public void Dispose() {
            this.control.Resize -= OnControlResized;
        }

        public WaitHandle BeginInvoke(Action action) {
            return control.BeginInvoke(action).AsyncWaitHandle;
        }

    }
    public abstract class D3DWFScene {
        readonly object loker;
        readonly FormsHost host;
        readonly FrameworkElement overlay;
        protected readonly EngineNotificator notify;

        DefaultInputObserver input;

        public RenderEngine Engine { get; private set; }

        public IContextState Context { get; }
        public WpfWindows Surface { get; private set; }
        public IInputManager Input => Surface.InputManager;

        public D3DWFScene(FormsHost host, FrameworkElement overlay, ContextStateProcessor context) :
            this(host, overlay, context, new EngineNotificator()) {
        }

        public D3DWFScene(FormsHost host, FrameworkElement overlay, ContextStateProcessor context, EngineNotificator notify) {
            this.host = host;
            this.overlay = overlay;
            host.HandleCreated += OnHandleCreated;
            host.Unloaded += OnUnloaded;
            this.Context = context;
            this.notify = notify;
            loker = new object();
        }

        public AdapterDescription GetAdapterDescription() => Engine.Graphics.Device.Adapter;
        public PerfomanceComponent GetPerfomanceState() {
            return Context.GetEntityManager().GetEntity(Engine.WorldTag).GetComponent<PerfomanceComponent>();
        }
        private void OnUnloaded(object sender, RoutedEventArgs e) {
            Dispose();
        }

        protected void OnHandleCreated(WinFormsD3DControl win) {
            lock (loker) {
                input = CreateInputObserver(win, overlay);
                Surface = new WpfWindows(win, host, input);
                Engine = RenderEngine.Create(Surface, Surface.InputManager, Context, notify);
                Engine.Run(notify);
            }
            SceneInitialization(Context, Engine, Engine.CameraTag);

        }

        protected abstract DefaultInputObserver CreateInputObserver(WinFormsD3DControl win, FrameworkElement overlay);
        protected abstract void SceneInitialization(IContextState context, RenderEngine engine, ElementTag camera);


        public virtual void Dispose() {
            host.HandleCreated -= OnHandleCreated;
            host.Unloaded -= OnUnloaded;
            lock (loker) {
                Engine?.Dispose();
                input?.Dispose();
                //Window.Dispose();
                Context?.Dispose();
            }
        }

    }

    
}
